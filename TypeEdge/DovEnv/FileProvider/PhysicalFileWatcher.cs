﻿using System;
using System.Collections.Concurrent;
using System.IO;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.Primitives;

namespace TypeEdge.DovEnv.FileProvider
{
    public class PhysicalFilesWatcher : IDisposable
    {
        private readonly ConcurrentDictionary<string, ChangeTokenInfo> _filePathTokenLookup =
            new ConcurrentDictionary<string, ChangeTokenInfo>(StringComparer.OrdinalIgnoreCase);
        private readonly ConcurrentDictionary<string, ChangeTokenInfo> _wildcardTokenLookup =
            new ConcurrentDictionary<string, ChangeTokenInfo>(StringComparer.OrdinalIgnoreCase);

        private readonly FileSystemWatcher _fileWatcher;
        private readonly object _fileWatcherLock = new object();
        private readonly string _root;
        private readonly bool _pollForChanges;

        /// <summary>
        /// Initializes an instance of <see cref="PhysicalFilesWatcher" /> that watches files in <paramref name="root" />.
        /// Wraps an instance of <see cref="System.IO.FileSystemWatcher" />
        /// </summary>
        /// <param name="root">Root directory for the watcher</param>
        /// <param name="fileSystemWatcher">The wrapped watcher that is watching <paramref name="root" /></param>
        /// <param name="pollForChanges">
        /// True when the watcher should use polling to trigger instances of
        /// <see cref="IChangeToken" /> created by <see cref="CreateFileChangeToken(string)" />
        /// </param>
        public PhysicalFilesWatcher(
            string root,
            FileSystemWatcher fileSystemWatcher,
            bool pollForChanges)
        {
            _root = root;
            _fileWatcher = fileSystemWatcher;
            _fileWatcher.IncludeSubdirectories = true;
            _fileWatcher.Created += OnChanged;
            _fileWatcher.Changed += OnChanged;
            _fileWatcher.Renamed += OnRenamed;
            _fileWatcher.Deleted += OnChanged;
            _fileWatcher.Error += OnError;

            _pollForChanges = pollForChanges;
        }

        /// <summary>
        ///     <para>
        ///     Creates an instance of <see cref="IChangeToken" /> for all files and directories that match the
        ///     <paramref name="filter" />
        ///     </para>
        ///     <para>
        ///     Globbing patterns are relative to the root directory given in the constructor
        ///     <seealso cref="PhysicalFilesWatcher(string, FileSystemWatcher, bool)" />. Globbing patterns
        ///     are interpreted by <seealso cref="Microsoft.Extensions.FileSystemGlobbing.Matcher" />.
        ///     </para>
        /// </summary>
        /// <param name="filter">A globbing pattern for files and directories to watch</param>
        /// <returns>A change token for all files that match the filter</returns>
        /// <exception cref="ArgumentNullException">When <paramref name="filter" /> is null</exception>
        public IChangeToken CreateFileChangeToken(string filter)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            filter = NormalizePath(filter);

            var changeToken = GetOrAddChangeToken(filter);
            TryEnableFileSystemWatcher();

            return changeToken;
        }

        private IChangeToken GetOrAddChangeToken(string pattern)
        {
            IChangeToken changeToken;
            var isWildCard = pattern.IndexOf('*') != -1;
            if (isWildCard || IsDirectoryPath(pattern))
            {
                changeToken = GetOrAddWildcardChangeToken(pattern);
            }
            else
            {
                changeToken = GetOrAddFilePathChangeToken(pattern);
            }

            return changeToken;
        }

        private IChangeToken GetOrAddFilePathChangeToken(string filePath)
        {
            ChangeTokenInfo tokenInfo;
            if (!_filePathTokenLookup.TryGetValue(filePath, out tokenInfo))
            {
                var cancellationTokenSource = new CancellationTokenSource();
                var cancellationChangeToken = new CancellationChangeToken(cancellationTokenSource.Token);
                tokenInfo = new ChangeTokenInfo(cancellationTokenSource, cancellationChangeToken);
                tokenInfo = _filePathTokenLookup.GetOrAdd(filePath, tokenInfo);
            }

            IChangeToken changeToken = tokenInfo.ChangeToken;
            if (_pollForChanges)
            {
                // The expiry of CancellationChangeToken is controlled by this type and consequently we can cache it.
                // PollingFileChangeToken on the other hand manages its own lifetime and consequently we cannot cache it.
                changeToken = new CompositeChangeToken(
                    new[]
                    {
                        changeToken,
                        new PollingFileChangeToken(new FileInfo(filePath))
                    });
            }

            return changeToken;
        }

        private IChangeToken GetOrAddWildcardChangeToken(string pattern)
        {
            ChangeTokenInfo tokenInfo;
            if (!_wildcardTokenLookup.TryGetValue(pattern, out tokenInfo))
            {
                var cancellationTokenSource = new CancellationTokenSource();
                var cancellationChangeToken = new CancellationChangeToken(cancellationTokenSource.Token);
                var matcher = new Matcher(StringComparison.OrdinalIgnoreCase);
                matcher.AddInclude(pattern);
                tokenInfo = new ChangeTokenInfo(cancellationTokenSource, cancellationChangeToken, matcher);
                tokenInfo = _wildcardTokenLookup.GetOrAdd(pattern, tokenInfo);
            }

            IChangeToken changeToken = tokenInfo.ChangeToken;
            if (_pollForChanges)
            {
                // The expiry of CancellationChangeToken is controlled by this type and consequently we can cache it.
                // PollingFileChangeToken on the other hand manages its own lifetime and consequently we cannot cache it.
                changeToken = new CompositeChangeToken(
                    new[]
                    {
                        changeToken,
                        new PollingWildCardChangeToken(_root, pattern)
                    });
            }

            return changeToken;
        }

        /// <summary>
        /// Disposes the file watcher
        /// </summary>
        public void Dispose()
        {
            _fileWatcher.Dispose();
        }

        private void OnRenamed(object sender, RenamedEventArgs e)
        {
            // For a file name change or a directory's name change notify registered tokens.
            OnFileSystemEntryChange(e.OldFullPath);
            OnFileSystemEntryChange(e.FullPath);

            if (Directory.Exists(e.FullPath))
            {
                try
                {
                    // If the renamed entity is a directory then notify tokens for every sub item.
                    foreach (
                        var newLocation in
                        Directory.EnumerateFileSystemEntries(e.FullPath, "*", SearchOption.AllDirectories))
                    {
                        // Calculated previous path of this moved item.
                        var oldLocation = Path.Combine(e.OldFullPath, newLocation.Substring(e.FullPath.Length + 1));
                        OnFileSystemEntryChange(oldLocation);
                        OnFileSystemEntryChange(newLocation);
                    }
                }
                catch (Exception ex) when (
                    ex is IOException ||
                    ex is SecurityException ||
                    ex is DirectoryNotFoundException ||
                    ex is UnauthorizedAccessException)
                {
                    // Swallow the exception.
                }
            }
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            OnFileSystemEntryChange(e.FullPath);
        }

        private void OnError(object sender, ErrorEventArgs e)
        {
            // Notify all cache entries on error.
            foreach (var path in _filePathTokenLookup.Keys)
            {
                ReportChangeForMatchedEntries(path);
            }
        }

        private void OnFileSystemEntryChange(string fullPath)
        {
            try
            {
                var relativePath = fullPath.Substring(_root.Length);
                ReportChangeForMatchedEntries(relativePath);
            }
            catch (Exception ex) when (
                ex is IOException ||
                ex is SecurityException ||
                ex is UnauthorizedAccessException)
            {
                // Swallow the exception.
            }
        }

        private void ReportChangeForMatchedEntries(string path)
        {
            path = NormalizePath(path);

            var matched = false;
            ChangeTokenInfo matchInfo;
            if (_filePathTokenLookup.TryRemove(path, out matchInfo))
            {
                CancelToken(matchInfo);
                matched = true;
            }

            foreach (var wildCardEntry in _wildcardTokenLookup)
            {
                var matchResult = wildCardEntry.Value.Matcher.Match(path);
                if (matchResult.HasMatches &&
                    _wildcardTokenLookup.TryRemove(wildCardEntry.Key, out matchInfo))
                {
                    CancelToken(matchInfo);
                    matched = true;
                }
            }

            if (matched)
            {
                TryDisableFileSystemWatcher();
            }
        }

        private void TryDisableFileSystemWatcher()
        {
            lock (_fileWatcherLock)
            {
                if (_filePathTokenLookup.IsEmpty &&
                    _wildcardTokenLookup.IsEmpty &&
                    _fileWatcher.EnableRaisingEvents)
                {
                    // Perf: Turn off the file monitoring if no files to monitor.
                    _fileWatcher.EnableRaisingEvents = false;
                }
            }
        }

        private void TryEnableFileSystemWatcher()
        {
            lock (_fileWatcherLock)
            {
                if ((!_filePathTokenLookup.IsEmpty || !_wildcardTokenLookup.IsEmpty) &&
                    !_fileWatcher.EnableRaisingEvents)
                {
                    // Perf: Turn off the file monitoring if no files to monitor.
                    _fileWatcher.EnableRaisingEvents = true;
                }
            }
        }

        private static string NormalizePath(string filter) => filter = filter.Replace('\\', '/');

        private static bool IsDirectoryPath(string path)
        {
            return path.Length > 0 &&
                (path[path.Length - 1] == Path.DirectorySeparatorChar ||
                path[path.Length - 1] == Path.AltDirectorySeparatorChar);
        }

        private static void CancelToken(ChangeTokenInfo matchInfo)
        {
            if (matchInfo.TokenSource.IsCancellationRequested)
            {
                return;
            }

            Task.Run(() =>
            {
                try
                {
                    matchInfo.TokenSource.Cancel();
                }
                catch
                {
                }
            });
        }

        private struct ChangeTokenInfo
        {
            public ChangeTokenInfo(
                CancellationTokenSource tokenSource,
                CancellationChangeToken changeToken)
                : this(tokenSource, changeToken, matcher: null)
            {
            }

            public ChangeTokenInfo(
                CancellationTokenSource tokenSource,
                CancellationChangeToken changeToken,
                Matcher matcher)
            {
                TokenSource = tokenSource;
                ChangeToken = changeToken;
                Matcher = matcher;
            }

            public CancellationTokenSource TokenSource { get; }

            public CancellationChangeToken ChangeToken { get; }

            public Matcher Matcher { get; }
        }
    }
}