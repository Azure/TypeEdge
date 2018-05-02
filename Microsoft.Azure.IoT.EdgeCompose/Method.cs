using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Azure.IoT.EdgeCompose
{
    public class Method<TMethodArgument, TMethodResponse> : IMethod
        where TMethodArgument : MethodArgument
        where TMethodResponse : MethodResponse
    {
        public string Name { get; set; }
        public Func<TMethodArgument, TMethodResponse> Callback { get; set; }

        public Method(string name, Func<TMethodArgument, TMethodResponse> callback)
        {
            Name = name;
            Callback = callback;
        }
    }
}
