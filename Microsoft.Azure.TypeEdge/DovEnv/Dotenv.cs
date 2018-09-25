using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;


//copied from https://github.com/frozzare/dotnet-dotenv

namespace Microsoft.Azure.TypeEdge.DovEnv
{
    public class Dotenv
    {
        public static readonly string DefaultPath = ".env";

        private readonly Dictionary<string, string> _variables = new Dictionary<string, string>();

        public static Dotenv Read(Stream input)
        {
            using (var reader = new StreamReader(input))
            {
                return new Dotenv(reader.ReadToEnd());
            }
        }

        public Dictionary<string, string> GetData()
        {
            return _variables.DeepClone();
        }


        protected Dotenv(string content)
        {
            var parsedVars = ParseContent(content);

            foreach (var variable in parsedVars)
            {
                var key = variable.Key;
                var value = variable.Value;

                foreach (var var in ParseValue(value))
                {
                    // When variable is not defined the result should be "{}".
                    var replace = String.IsNullOrEmpty(parsedVars[var]) ? "{}" : parsedVars[var];
                    value = value.Replace("${" + var + "}", replace, StringComparison.OrdinalIgnoreCase);
                }

                Environment.SetEnvironmentVariable(key, value);
                _variables[key] = value;
            }
        }

        protected IList<string> ParseValue(string value)
        {
            var vars = new List<string>();
            var regex = new Regex(@"\$\{(.*?)\}");

            foreach (Match match in regex.Matches(value))
            {
                if (!match.Success)
                {
                    continue;
                }

                vars.Add(match.Groups[1].Value);
            }

            return vars;
        }

        protected Dictionary<string, string> ParseContent(string content)
        {
            var lines = content.Split('\n');
            var vars = new Dictionary<string, string>();
            var regex = new Regex(@"^(?:export|)\s*([^\d+][:\w_]+)\s?=\s?(.+)");
            foreach (var t in lines)
            {
                var matches = regex.Match(t);
                var key = matches.Groups[1].Value;
                var value = String.Empty;

                // Bail if empty key.
                if (string.IsNullOrEmpty(key))
                {
                    continue;
                }

                // Replace empty value with real value if any.
                if (!string.IsNullOrEmpty(matches.Groups[2].Value))
                {
                    value = matches.Groups[2].Value;
                }

                // Split string that don't starts with a quote.
                if (!value.StartsWith("\"", StringComparison.OrdinalIgnoreCase) && !value.StartsWith("\'", StringComparison.OrdinalIgnoreCase))
                {
                    value = value.Split(' ')[0];
                }

                // Remove quotes in the beging and the end of a string.
                value = Regex.Replace(value, "^(?:\"|\')|(?:\"|\')$", string.Empty);

                vars.Add(key, value);
            }

            return vars;
        }
    }
}