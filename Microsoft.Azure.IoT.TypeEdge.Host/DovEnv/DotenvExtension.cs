using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;

namespace Microsoft.Azure.IoT.TypeEdge.Host.DovEnv
{
    public static class DotenvExtension
    {
        public static IConfigurationBuilder AddDotenvFile(this IConfigurationBuilder builder)
        {
            return AddDotenvFile(builder, null, string.Empty, false, false);
        }

        public static IConfigurationBuilder AddDotenvFile(this IConfigurationBuilder builder, string path)
        {
            return AddDotenvFile(builder, null, path, false, false);
        }

        public static IConfigurationBuilder AddDotenvFile(this IConfigurationBuilder builder, string path, bool optional)
        {
            return AddDotenvFile(builder, null, path, optional, false);
        }

        public static IConfigurationBuilder AddDotenvFile(this IConfigurationBuilder builder, string path, bool optional, bool reloadOnChange)
        {
            return AddDotenvFile(builder, null, path, optional, reloadOnChange);
        }

        public static IConfigurationBuilder AddDotenvFile(this IConfigurationBuilder builder, IFileProvider provider, string path, bool optional, bool reloadOnChange)
        {
            // Bail if builder is null.
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            // Use the default value if value is empty.
            if (string.IsNullOrEmpty(path))
            {
                path = Dotenv.DefaultPath;
                path = path.Replace("./", "");
            }

#if NET451
            var basePath1 = AppDomain.CurrentDomain.GetData("APP_CONTEXT_BASE_DIRECTORY") as string
                ?? AppDomain.CurrentDomain.BaseDirectory
                ?? string.Empty;
#else
            var basePath1 = AppContext.BaseDirectory ?? string.Empty;
#endif
            var basePaths = new List<string>() { basePath1 };

            // Since we shouldn't relay on asp.net core hosting package
            // and then we can't get content root path and because of
            // https://github.com/aspnet/FileSystem/issues/232 
            // we have to create two different paths that we can try to read the dotenv file from.
            if (basePath1.Contains("/bin/"))
            {
                var basePath2 = basePath1.Split(new string[] { "bin" }, StringSplitOptions.None)[0];
                basePaths.Add(basePath2.TrimEnd('/'));
            }

            if (provider == null)
            {
                //--
                // The below is still needed for .NET Core 1.x
                //--
                var fileExists = false;
                foreach (var basePath in basePaths)
                {
                    var testPath = string.Join("/", new string[] { basePath, path });
                    if (File.Exists(testPath))
                    {
                        fileExists = true;
                        path = testPath;
                        break;
                    }
                }
                if (!fileExists && !optional)
                {
                    throw new Exception($"The .env configuration file '{path}' was not found");
                }
                if (Path.IsPathRooted(path))
                {
                    // Real PhysicalFileProvider has a bug that don't allow dot files:
                    // https://github.com/aspnet/FileSystem/issues/232
                    provider = new FileProvider.PhysicalFileProvider(Path.GetDirectoryName(path));
                    path = Path.GetFileName(path);
                }
            }
            else
            {
                //--
                // For .NET Core 2.0 and above, the PhysicalFileProvider has ways to deal
                // with hidden files.
                // See the change: https://github.com/aspnet/FileSystem/pull/280/files
                // This also allowed MockFileProvider to be plugged in for unit testing.
                //--
                if (!provider.GetFileInfo(path).Exists)
                {
                    throw new Exception($"The configuration file {path} could not be found.");
                }
            }

            var source = new DotenvConfigurationSource
            {
                Path = path,
                Optional = optional,
                FileProvider = provider,
                ReloadOnChange = reloadOnChange
            };
            builder.Add(source);
            return builder;
        }
    }
}