using System;
using System.Reflection;
using Castle.DynamicProxy;
using Microsoft.Azure.IoT.TypeEdge.Attributes;
using Microsoft.Azure.IoT.TypeEdge.Modules;
using Microsoft.Azure.IoT.TypeEdge.Modules.Endpoints;
using Microsoft.Azure.IoT.TypeEdge.Twins;
using Microsoft.Azure.IoT.TypeEdge.Volumes;

namespace Microsoft.Azure.IoT.TypeEdge.Proxy
{
    internal class ModuleProxyBase : EdgeModule, IInterceptor
    {
        private readonly Type _type;

        public ModuleProxyBase(Type type)
        {
            _type = type;
        }

        internal override string Name
        {
            get
            {
                if (!(_type.GetCustomAttribute(typeof(TypeModuleAttribute), true) is TypeModuleAttribute))
                    throw new ArgumentException($"{_type.Name} has no TypeModule annotation");
                if (!_type.IsInterface)
                    throw new ArgumentException($"{_type.Name} needs to be an interface");
                return _type.Name.Substring(1).ToLower();
            }
        }

        public void Intercept(IInvocation invocation)
        {
            if (!invocation.Method.ReturnType.IsGenericType)
                return;
            var genericDef = invocation.Method.ReturnType.GetGenericTypeDefinition();
            if (!genericDef.IsAssignableFrom(typeof(Input<>)) &&
                !genericDef.IsAssignableFrom(typeof(Output<>)) &&
                !genericDef.IsAssignableFrom(typeof(ModuleTwin<>)) &&
                !genericDef.IsAssignableFrom(typeof(Volume<>)))
                return;
            var value = Activator.CreateInstance(
                genericDef.MakeGenericType(invocation.Method.ReturnType.GenericTypeArguments),
                invocation.Method.Name.Replace("get_", ""), this);
            invocation.ReturnValue = value;
        }
    }
}