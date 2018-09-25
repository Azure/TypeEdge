using System.Reflection;

namespace Microsoft.Azure.TypeEdge.Modules
{
    public class MethodCallback
    {
        public MethodCallback(string name, MethodInfo methodInfo)
        {
            Name = name;
            MethodInfo = methodInfo;
        }

        public string Name { get; }

        public MethodInfo MethodInfo { get; }
    }
}