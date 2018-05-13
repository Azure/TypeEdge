using Castle.DynamicProxy;
using Microsoft.Azure.Devices;
using Microsoft.Azure.IoT.TypeEdge.Modules;
using Newtonsoft.Json;
using System;
using System.Dynamic;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoT.TypeEdge.Proxy
{
    internal class ModuleProxy : EdgeModule, IInterceptor
    {
        private string connectionString;
        private string deviceId;
        private RegistryManager registryManager;
        public ModuleProxy(string connectionString, string deviceId)
        {
            this.deviceId = deviceId;
            this.connectionString = connectionString;
            registryManager = RegistryManager.CreateFromConnectionString(connectionString);
        }
        public override async Task<T> GetTwinAsync<T>(string name)
        {
            var twin = await registryManager.GetTwinAsync(deviceId);
            var typeTwin = Activator.CreateInstance<T>();
            typeTwin.SetTwin(twin);
            return typeTwin;
        }
        public override async Task<T> PublishTwinAsync<T>(string name, T typeTwin)
        {
            var twin = typeTwin.GetTwin();
            var res = await registryManager.UpdateTwinAsync(deviceId, twin, twin.ETag);
            typeTwin.SetTwin(res);
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
                    var value = Activator.CreateInstance(genericDef.MakeGenericType(invocation.Method.ReturnType.GenericTypeArguments), invocation.Method.Name.Replace("get_", ""), this);
                    invocation.ReturnValue = value;
                }
            }
        }
    }

}
