using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoT.TypeEdge
{
    public class SubscriptionCallback
    {
        public string Name { get; private set; }
        public Delegate Handler { get; private set; }

        public Type Type { get; private set; }

        public SubscriptionCallback(string name, Delegate handler, Type type)
        {
            this.Name = name;
            this.Handler = handler;
            Type = type;
        }

    }
}