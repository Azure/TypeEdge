using System;

namespace Microsoft.Azure.TypeEdge.Description
{
    public class TwinDescription
    {
        public TwinDescription(string name, Type type, Func<Type, string> schemaGenerator)
        {
            Name = name;
            TypeDescription = new TypeDescription(type, schemaGenerator);
        }

        public string Name { get; }

        public TypeDescription TypeDescription { get; }

        //public TwinDescription(IEnumerable<Type> types, Func<Type, string> schemaGenerator)
        //{
        //    if (types == null || !types.Any())
        //        return;

        //    var mergeSettings = new JsonMergeSettings
        //    {
        //        MergeArrayHandling = MergeArrayHandling.Union
        //    };

        //    var typeJObjects = types.Select(e => JObject.Parse(schemaGenerator(e))).ToArray();
        //    var union = typeJObjects[0];
        //    for (var i = 0; i < typeJObjects.Length-1; i++)
        //        union.Merge(typeJObjects[i + 1], mergeSettings);

        //    TypeDescription = union.ToString(Formatting.None);

        //}
    }
}