using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoT.TypeEdge
{
    public class SubscriptionCallback
    {
        public string Name { get; set; }
        public Delegate Handler { get; set; }

        public Type MessageType { get; set; }

        public SubscriptionCallback(string name, Delegate handler, Type messageType)
        {
            this.Name = name;
            this.Handler = handler;
            MessageType = messageType;
        }
    }
}