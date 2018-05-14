using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoT.TypeEdge
{
    public class MethodCallback
    {
        public string Name { get; private set; }

        public MethodInfo MethodInfo { get; private set; }

        public MethodCallback(string name, MethodInfo methodInfo)
        {
            this.Name = name;
            MethodInfo = methodInfo;
        }

    }
}