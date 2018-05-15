using Castle.DynamicProxy;
using Microsoft.Azure.IoT.TypeEdge.Attributes;
using Microsoft.Azure.IoT.TypeEdge.Modules;
using System;
using System.Reflection;

namespace Microsoft.Azure.IoT.TypeEdge.Host
{
    internal class ModuleProxyBase : EdgeModule, IInterceptor
    {
        readonly Type type;
        public ModuleProxyBase(Type type)
        {
            this.type = type;
        }
        internal override string Name
        {
            get
            {
                var typeModule = type.GetCustomAttribute(typeof(TypeModuleAttribute), true) as TypeModuleAttribute;
                if (typeModule != null && typeModule.Name != null)
                    return typeModule.Name;

                if (type.IsInterface)
                    return type.Name.TrimStart('I');
                return type.Name;
            }
        }
        public void Intercept(IInvocation invocation)
        {
            if (invocation.Method.ReturnType.IsGenericType)
            {
                var genericDef = invocation.Method.ReturnType.GetGenericTypeDefinition();
                if (genericDef.IsAssignableFrom(typeof(Input<>))
                    || genericDef.IsAssignableFrom(typeof(Output<>))
                    || genericDef.IsAssignableFrom(typeof(ModuleTwin<>)))
                {
                    var value = Activator.CreateInstance(genericDef.MakeGenericType(invocation.Method.ReturnType.GenericTypeArguments), invocation.Method.Name.Replace("get_", ""), this);
                    invocation.ReturnValue = value;
                }
            }
        }
    }
}