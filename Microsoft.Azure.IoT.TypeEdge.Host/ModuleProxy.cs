using Castle.DynamicProxy;
using Microsoft.Azure.IoT.TypeEdge.Attributes;
using Microsoft.Azure.IoT.TypeEdge.Modules;
using System;
using System.Reflection;

namespace Microsoft.Azure.IoT.TypeEdge.Host
{
    internal class ModuleProxy<T> : EdgeModule, IInterceptor
        where T : class
    {
        internal override string Name
        {
            get
            {
                var typeModule = typeof(T).GetCustomAttribute(typeof(TypeModuleAttribute), true) as TypeModuleAttribute;
                if (typeModule != null && typeModule.Name != null)
                    return typeModule.Name;

                if (typeof(T).IsInterface)
                    return typeof(T).Name.TrimStart('I');
                return typeof(T).Name;
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