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
        private string iotHubConnectionString;
        private string deviceId;
        private RegistryManager registryManager;
        private static ServiceClient serviceClient;

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

        public ModuleProxy(string iotHubConnectionString, string deviceId)
        {
            this.deviceId = deviceId;
            this.iotHubConnectionString = iotHubConnectionString;
            registryManager = RegistryManager.CreateFromConnectionString(iotHubConnectionString);
            serviceClient = ServiceClient.CreateFromConnectionString(iotHubConnectionString);
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
            if (invocation.Method.IsSpecialName && invocation.Method.ReturnType.IsGenericType)
            {
                //known properties
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
            else if (!invocation.Method.IsSpecialName)
            {
                //direct methods
                var methodInvocation = new CloudToDeviceMethod(invocation.Method.Name) { ResponseTimeout = TimeSpan.FromSeconds(30) };
                var paramData = JsonConvert.SerializeObject(invocation.Arguments);
                methodInvocation.SetPayloadJson(paramData);

                // Invoke the direct method asynchronously and get the response from the simulated device.
                var response = serviceClient.InvokeDeviceMethodAsync(deviceId, Name, methodInvocation).Result;

                if (response.Status == 200)
                    invocation.ReturnValue = Convert.ChangeType(JsonConvert.DeserializeObject(response.GetPayloadAsJson()), invocation.Method.ReturnType);
                else
                    throw new Exception($"Direct method result Status:{response.Status}, {response.GetPayloadAsJson()}");
            }
        }
    }

}
