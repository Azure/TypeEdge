using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.TypeEdge.Attributes;
using Microsoft.Azure.TypeEdge.DovEnv;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;

namespace Microsoft.Azure.TypeEdge
{
    public static class Extensions
    {
        public static IConfigurationBuilder AddDotΕnv(this IConfigurationBuilder builder)
        {
            return AddDotΕnv(builder, null, string.Empty, false, false);
        }

        public static IConfigurationBuilder AddDotΕnv(this IConfigurationBuilder builder, string path)
        {
            return AddDotΕnv(builder, null, path, false, false);
        }

        public static IConfigurationBuilder AddDotΕnv(this IConfigurationBuilder builder, string path,
            bool optional)
        {
            return AddDotΕnv(builder, null, path, optional, false);
        }

        public static IConfigurationBuilder AddDotΕnv(this IConfigurationBuilder builder, string path,
            bool optional, bool reloadOnChange)
        {
            return AddDotΕnv(builder, null, path, optional, reloadOnChange);
        }

        public static IConfigurationBuilder AddDotΕnv(this IConfigurationBuilder configurationBuilder,
            IFileProvider provider, string filePath, bool optional, bool reloadOnChange)
        {
            if (configurationBuilder == null)
                throw new ArgumentNullException(nameof(configurationBuilder));

            if (string.IsNullOrEmpty(filePath))
                filePath = DotΕnv.DefaultPath;

            var lookUpPaths = new List<string> {AppContext.BaseDirectory, ""};

            if (provider == null)
            {
                var exists = false;
                foreach (var lookUpPath in lookUpPaths)
                {
                    var fullPath = Path.Join(lookUpPath, filePath);
                    if (File.Exists(fullPath))
                    {
                        exists = true;
                        filePath = fullPath;
                        break;
                    }
                }

                if (!exists && !optional)
                    throw new Exception($"Could not locate {filePath}");
            }
            else if (!provider.GetFileInfo(filePath).Exists)
            {
                throw new Exception($"Could not locate {filePath}");
            }

            if (Path.IsPathRooted(filePath))
            {
                provider = new PhysicalFileProvider(Path.GetDirectoryName(filePath), ExclusionFilters.None);
                filePath = Path.GetFileName(filePath);
            }

            var source = new DotΕnvConfigurationSource
            {
                Path = filePath,
                Optional = optional,
                FileProvider = provider,
                ReloadOnChange = reloadOnChange
            };
            configurationBuilder.Add(source);
            return configurationBuilder;
        }

        public static Dictionary<TKey, TValue> DeepClone<TKey, TValue>
            (this Dictionary<TKey, TValue> original) where TValue : ICloneable
        {
            var ret = new Dictionary<TKey, TValue>(original.Count,
                original.Comparer);
            foreach (var entry in original) ret.Add(entry.Key, (TValue) entry.Value.Clone());
            return ret;
        }

        public static Task WhenCanceled(this CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            cancellationToken.Register(s => ((TaskCompletionSource<bool>) s).SetResult(true), tcs);
            return tcs.Task;
        }

        public static string GetModuleName(this Type type)
        {
            return type.Name.Substring(1).ToLower(CultureInfo.CurrentCulture);
        }

        public static T1 CopyFrom<T1, T2>(this T1 obj, T2 otherObject)
            where T1 : class
            where T2 : class
        {
            var srcFields = otherObject.GetType().GetProperties(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty);

            var destFields = obj.GetType().GetProperties(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty);

            foreach (var property in srcFields)
            {
                var dest = destFields.FirstOrDefault(x => x.Name == property.Name);
                if (dest != null && dest.CanWrite)
                    dest.SetValue(obj, property.GetValue(otherObject, null), null);
            }

            return obj;
        }

        public static Type GetProxyInterface(this Type type)
        {
            return type.GetInterfaces()
                .SingleOrDefault(i => i.GetCustomAttribute(typeof(TypeModuleAttribute), true) != null);
        }
    }
}