using Castle.DynamicProxy;
using Microsoft.Azure.IoT.TypeEdge.Attributes;
using Microsoft.Azure.IoT.TypeEdge.Modules;
using System;
using System.Reflection;

namespace Microsoft.Azure.IoT.TypeEdge.Host
{
    internal class ModuleProxy<T> : ModuleProxyBase
        where T : class
    {
        public ModuleProxy()
            :base(typeof(T))
        {

        }
    }
}