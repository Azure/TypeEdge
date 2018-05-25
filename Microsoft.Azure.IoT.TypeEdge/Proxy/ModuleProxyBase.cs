using System;
using System.Reflection;
using Castle.DynamicProxy;
using Microsoft.Azure.IoT.TypeEdge.Attributes;
using Microsoft.Azure.IoT.TypeEdge.Modules;
using Microsoft.Azure.IoT.TypeEdge.Modules.Endpoints;
using Microsoft.Azure.IoT.TypeEdge.Twins;

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
                var typeModule = _type.GetCustomAttribute(typeof(TypeModuleAttribute), true) as TypeModuleAttribute;
                if (typeModule?.Name != null)
                    return typeModule.Name;

                return _type.IsInterface ? _type.Name.TrimStart('I') : _type.Name;
            }
        }

        public void Intercept(IInvocation invocation)
        {
            if (!invocation.Method.ReturnType.IsGenericType)
                return;
            var genericDef = invocation.Method.ReturnType.GetGenericTypeDefinition();
            if (!genericDef.IsAssignableFrom(typeof(Input<>)) && !genericDef.IsAssignableFrom(typeof(Output<>)) &&
                !genericDef.IsAssignableFrom(typeof(ModuleTwin<>)))
                return;
            var value = Activator.CreateInstance(
                genericDef.MakeGenericType(invocation.Method.ReturnType.GenericTypeArguments),
                invocation.Method.Name.Replace("get_", ""), this);
            invocation.ReturnValue = value;
        }
    }
}