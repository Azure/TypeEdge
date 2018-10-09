using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.TypeEdge.Description
{
    public class TwinDescription
    {
        public string TypeDescription { get; }

        public TwinDescription(IEnumerable<Type> types, Func<Type, string> schemaGenerator)
        {
            if (types == null || !types.Any())
                return;
            
            var mergeSettings = new JsonMergeSettings
            {
                MergeArrayHandling = MergeArrayHandling.Union
            };

            var typeJObjects = types.Select(e => JObject.Parse(schemaGenerator(e))).ToArray();
            var union = typeJObjects[0];
            for (var i = 0; i < typeJObjects.Length-1; i++)
                union.Merge(typeJObjects[i + 1], mergeSettings);

            TypeDescription = union.ToString(Formatting.None);

        }
    }
}