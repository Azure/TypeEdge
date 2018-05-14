using Castle.DynamicProxy;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Azure.IoT.TypeEdge.Attributes;
using Microsoft.Azure.IoT.TypeEdge.Modules;
using Newtonsoft.Json;
using System;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoT.TypeEdge.Proxy
{
    internal class ModuleProxy<T> : EdgeModule, IInterceptor
        where T : class
    {
        private string connectionString;
        private string deviceId;
        private RegistryManager registryManager;

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

        public ModuleProxy(string connectionString, string deviceId)
        {
            this.deviceId = deviceId;
            this.connectionString = connectionString;
            registryManager = RegistryManager.CreateFromConnectionString(connectionString);
        }
        internal override async Task<_T> GetTwinAsync<_T>(string name)
        {
            var twin = await registryManager.GetTwinAsync(deviceId, Name);
            var typeTwin = Activator.CreateInstance<_T>();
            typeTwin.SetTwin(name, twin);
            return typeTwin;
        }
        internal override async Task<_T> PublishTwinAsync<_T>(string name, _T typeTwin)
        {
            var twin = typeTwin.GetDesiredTwin(name);
            var res = await registryManager.UpdateTwinAsync(deviceId, Name, twin, twin.ETag);
            typeTwin.SetTwin(name, res);
            return typeTwin;
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
                    var value = Activator.CreateInstance(
                        genericDef.MakeGenericType(invocation.Method.ReturnType.GenericTypeArguments),
                        invocation.Method.Name.Replace("get_", ""), this);
                    invocation.ReturnValue = value;
                }
            }
        }
    }

}
