using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoT.TypeEdge.Modules
{
    public class MessageCallback
    {
        public string Name { get; set; }
        public MethodInfo MethodInfo { get; set; }
        public Delegate Handler { get; set; }

        public Type MessageType { get; set; }

        public MessageCallback(string name, MethodInfo methodInfo, Delegate handler, Type messageType)
        {
            this.Name = name;
            this.MethodInfo = methodInfo;
            this.Handler = handler;
            MessageType = messageType;
        }
    }
}