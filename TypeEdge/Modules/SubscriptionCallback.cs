using System;

namespace TypeEdge.Modules
{
    public class SubscriptionCallback
    {
        public SubscriptionCallback(string name, Delegate handler, Type type)
        {
            Name = name;
            Handler = handler;
            Type = type;
        }

        public string Name { get; }
        public Delegate Handler { get; }

        public Type Type { get; }
    }
}