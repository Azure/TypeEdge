using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Azure.TypeEdge.Description;
using Newtonsoft.Json.Schema;

namespace Microsoft.Azure.TypeEdge.Host.Service
{
    //copied from https://gist.github.com/balexandre/e8cd6a965cdc532cb6ae#file-jsonschematopocos-cs

    public class CodeGenerator
    {
        private const string Cyrillic = "Cyrillic";
        private const string Nullable = "?";
        private const string PocoClassPrefix = "MailChimp_";

        public string Generate(TypeDescription typeDescription)
        {
            var schema = JSchema.Parse(typeDescription.Description);
            var name = typeDescription.Name;
            if (schema.Type == null)
                throw new Exception("Schema does not specify a type.");

            var sb = new StringBuilder();

            switch (schema.Type)
            {
                case JSchemaType.Object:
                    sb.Append(ConvertJsonSchemaObjectToPoco(name, schema));
                    break;

                case JSchemaType.Array:
                    foreach (var item in schema.Items.Where(x => x.Type.HasValue && x.Type == JSchemaType.Object))
                        sb.Append(ConvertJsonSchemaObjectToPoco(null, item));
                    break;
            }

            return sb.ToString();
        }

        private StringBuilder ConvertJsonSchemaObjectToPoco(string classname, JSchema schema)
        {
            return ConvertJsonSchemaObjectToPoco(classname, schema, out var newClassName);
        }

        private StringBuilder ConvertJsonSchemaObjectToPoco(string className, JSchema schema, out string newClassName)
        {
            var sb = new StringBuilder();
            var isEnum = schema.Enum != null && schema.Enum.Any();

            sb.AppendLine(GenerateObjectSummary(schema));
            sb.AppendFormat("public {0} ", isEnum ? "enum" : "class");

            if (string.IsNullOrEmpty(className)) className = GetClassName(schema);
            newClassName = className;

            sb.Append(className);
            sb.AppendLine(" {");

            if (isEnum)
                sb.AppendLine(string.Join(",", schema.Enum));
            else
                foreach (var item in schema.Properties)
                {
                    // Property Summary
                    sb.AppendLine(GenerateObjectSummary(item.Value));

                    sb.Append("public ");
                    sb.Append(GetClrType(item.Value, sb));
                    sb.Append(" ");
                    sb.Append(item.Key.Trim());
                    sb.AppendLine(" { get; set; }");
                }

            sb.AppendLine("}");
            return sb;
        }

        private string GenerateObjectSummary(JSchema schema)
        {
            var sb = new StringBuilder();

            // summary
            sb.AppendLine("\n/// <summary>");

            if (!string.IsNullOrWhiteSpace(schema.Title))
                sb.AppendFormat("/// {0}\n", schema.Title);

            if (!string.IsNullOrWhiteSpace(schema.Description))
                sb.AppendFormat("/// {0}\n", schema.Description);

            sb.AppendLine("/// </summary>");

            // extra data
            foreach (var ed in schema.ExtensionData) sb.AppendFormat("/// <{0}>{1}</{0}>\n", ed.Key, ed.Value);

            return sb.ToString();
        }

        private string GetClassName(JSchema schema)
        {
            if (schema.Title != null)
                return string.Format("{0}{1}", PocoClassPrefix, GenerateSlug(schema.Title));
            return string.Format("{0}{1}", PocoClassPrefix, Guid.NewGuid().ToString("N"));
        }

        private string GenerateSlug(string phrase)
        {
            var str = RemoveAccent(phrase);
            str = Regex.Replace(str, @"[^a-zA-Z\s-]", ""); // invalid chars
            str = Regex.Replace(str, @"\s+", " ").Trim(); // convert multiple spaces into one space, trim
            str = Regex.Replace(str, @"\s", "_"); // convert spaces to underscores
            return str;
        }

        private string RemoveAccent(string txt)
        {
            var bytes = Encoding.GetEncoding(Cyrillic).GetBytes(txt);
            return Encoding.ASCII.GetString(bytes);
        }

        private string GetClrType(JSchema jsonSchema, StringBuilder sb)
        {
            string className = null;
            switch (jsonSchema.Type)
            {
                case JSchemaType.Array:
                    if (jsonSchema.Items.Count == 0)
                        return "IEnumerable<object>";
                    if (jsonSchema.Items.Count == 1)
                        return string.Format("IEnumerable<{0}>", GetClrType(jsonSchema.Items.First(), sb));
                    throw new Exception("Not sure what type this will be.");

                case JSchemaType.Boolean:
                    return string.Format("bool{0}",
                        jsonSchema.Required == null || !jsonSchema.Required.Any() ? string.Empty : Nullable);

                case JSchemaType.Number:
                    return string.Format("float{0}",
                        jsonSchema.Required == null || !jsonSchema.Required.Any() ? string.Empty : Nullable);

                case JSchemaType.Integer:
                    return string.Format("int{0}",
                        jsonSchema.Required == null || !jsonSchema.Required.Any() ? string.Empty : Nullable);

                case JSchemaType.String:
                    if (jsonSchema.Enum != null && jsonSchema.Enum.Any())
                    {
                        // it's an enumeration
                        sb.Insert(0, ConvertJsonSchemaObjectToPoco(null, jsonSchema, out className));
                        return className;
                    }

                    return "string";

                case JSchemaType.Object:
                    sb.Insert(0, ConvertJsonSchemaObjectToPoco(null, jsonSchema, out className));
                    return className;

                case JSchemaType.None:
                case JSchemaType.Null:
                default:
                    return "object";
            }
        }
    }

    // To be used when $ref has URL's instead Id's
    public class UrlResolver : JSchemaResolver
    {
        public override Stream GetSchemaResource(ResolveSchemaContext context, SchemaReference reference)
        {
            return GetStreamFromUrl(reference.BaseUri);
        }

        // http://stackoverflow.com/a/19051936/28004
        private Stream GetStreamFromUrl(Uri url)
        {
            byte[] refData = null;

            using (var wc = new WebClient())
            {
                refData = wc.DownloadData(url);
            }

            return new MemoryStream(refData);
        }
    }
}