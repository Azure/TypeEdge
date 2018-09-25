using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.TypeEdge.DovEnv;

namespace Microsoft.Azure.TypeEdge
{
    public static class Extensions
    {
        public static IConfigurationBuilder AddDotenv(this IConfigurationBuilder builder)
        {
            return AddDotenv(builder, null, string.Empty, false, false);
        }

        public static IConfigurationBuilder AddDotenv(this IConfigurationBuilder builder, string path)
        {
            return AddDotenv(builder, null, path, false, false);
        }

        public static IConfigurationBuilder AddDotenvFile(this IConfigurationBuilder builder, string path, bool optional)
        {
            return AddDotenv(builder, null, path, optional, false);
        }

        public static IConfigurationBuilder AddDotenvFile(this IConfigurationBuilder builder, string path, bool optional, bool reloadOnChange)
        {
            return AddDotenv(builder, null, path, optional, reloadOnChange);
        }

        public static IConfigurationBuilder AddDotenv(this IConfigurationBuilder configurationBuilder, IFileProvider provider, string filePath, bool optional, bool reloadOnChange)
        {
            if (configurationBuilder == null)
                throw new ArgumentNullException(nameof(configurationBuilder));

            if (string.IsNullOrEmpty(filePath))
                filePath = Dotenv.DefaultPath;

            var lookUpPaths = new List<string>() { AppContext.BaseDirectory, "" };

            if (provider == null)
            {
                bool exists = false;
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
            else
                if (!provider.GetFileInfo(filePath).Exists)
                throw new Exception($"Could not locate {filePath}");

            if (Path.IsPathRooted(filePath))
            {                
                provider = new PhysicalFileProvider(Path.GetDirectoryName(filePath), Microsoft.Extensions.FileProviders.Physical.ExclusionFilters.None);
                filePath = Path.GetFileName(filePath);
            }

            var source = new DotenvConfigurationSource
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
            Dictionary<TKey, TValue> ret = new Dictionary<TKey, TValue>(original.Count,
                                                                    original.Comparer);
            foreach (KeyValuePair<TKey, TValue> entry in original)
            {
                ret.Add(entry.Key, (TValue)entry.Value.Clone());
            }
            return ret;
        }

        public static Task WhenCanceled(this CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).SetResult(true), tcs);
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
            PropertyInfo[] srcFields = otherObject.GetType().GetProperties(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty);

            PropertyInfo[] destFields = obj.GetType().GetProperties(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty);

            foreach (var property in srcFields)
            {
                var dest = destFields.FirstOrDefault(x => x.Name == property.Name);
                if (dest != null && dest.CanWrite)
                    dest.SetValue(obj, property.GetValue(otherObject, null), null);
            }

            return obj;
        }
    }
}