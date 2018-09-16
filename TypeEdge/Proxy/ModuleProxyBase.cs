using System;
using System.Reflection;
using Castle.DynamicProxy;
using TypeEdge.Attributes;
using TypeEdge.Modules;
using TypeEdge.Modules.Endpoints;
using TypeEdge.Twins;
using TypeEdge.Volumes;

namespace TypeEdge.Proxy
{
    internal class ModuleProxyBase : TypeModule, IInterceptor
    {
        private readonly Type _type;

        public ModuleProxyBase(Type type)
        {
            _type = type;
        }

        public override string Name
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