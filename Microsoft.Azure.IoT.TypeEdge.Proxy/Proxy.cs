using System;
using System.Reflection;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using Microsoft.Azure.Devices;
using Microsoft.Azure.IoT.TypeEdge.Attributes;
using Microsoft.Azure.IoT.TypeEdge.Modules;
using Microsoft.Azure.IoT.TypeEdge.Modules.Endpoints;
using Microsoft.Azure.IoT.TypeEdge.Twins;
using Microsoft.Azure.IoT.TypeEdge.Volumes;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoT.TypeEdge.Proxy
{
    internal class Proxy<T> : EdgeModule, IInterceptor
        where T : class
    {
        private readonly ServiceClient _serviceClient;

        private readonly string _deviceId;
        private readonly string _iotHubConnectionString;
        private readonly RegistryManager _registryManager;

        public Proxy(string iotHubConnectionString, string deviceId)
        {
            _deviceId = deviceId;
            _iotHubConnectionString = iotHubConnectionString;
            _registryManager = RegistryManager.CreateFromConnectionString(iotHubConnectionString);
            _serviceClient = ServiceClient.CreateFromConnectionString(iotHubConnectionString);
        }

        internal override string Name
        {
            get
            {
                if (!(typeof(T).GetCustomAttribute(typeof(TypeModuleAttribute), true) is TypeModuleAttribute))
                    throw new ArgumentException($"{typeof(T).Name} has no TypeModule annotation");
                if (!typeof(T).IsInterface)
                    throw new ArgumentException($"{typeof(T).Name} needs to be an interface");
                return typeof(T).Name.Substring(1).ToLower();
            }
        }

        public void Intercept(IInvocation invocation)
        {
            if (invocation.Method.IsSpecialName && invocation.Method.ReturnType.IsGenericType)
            {
                //known properties
                var genericDef = invocation.Method.ReturnType.GetGenericTypeDefinition();
                if (genericDef.IsAssignableFrom(typeof(Input<>))
                    || genericDef.IsAssignableFrom(typeof(Output<>))
                    || genericDef.IsAssignableFrom(typeof(ModuleTwin<>))
                    || genericDef.IsAssignableFrom(typeof(Volume<>)))
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
                var methodInvocation =
                    new CloudToDeviceMethod(invocation.Method.Name) { ResponseTimeout = TimeSpan.FromSeconds(30) };
                var paramData = JsonConvert.SerializeObject(invocation.Arguments);
                methodInvocation.SetPayloadJson(paramData);

                // Invoke the direct method asynchronously and get the response from the simulated device.
                var response = _serviceClient.InvokeDeviceMethodAsync(_deviceId,  Name, methodInvocation).Result;

                if (response.Status == 200)
                {
                    if (invocation.Method.ReturnType != typeof(void))
                        invocation.ReturnValue =
                            Convert.ChangeType(JsonConvert.DeserializeObject(response.GetPayloadAsJson()),
                                invocation.Method.ReturnType);
                }
                else
                    throw new Exception(
                        $"Direct method result Status:{response.Status}, {response.GetPayloadAsJson()}");
            }
        }

        internal override async Task<TT> GetTwinAsync<TT>(string name)
        {
            var twin = await _registryManager.GetTwinAsync(_deviceId, Name);
            var typeTwin = Activator.CreateInstance<TT>();
            typeTwin.SetTwin(name, twin);
            return typeTwin;
        }

        internal override async Task<TT> PublishTwinAsync<TT>(string name, TT typeTwin)
        {
            var twin = typeTwin.GetDesiredTwin(name);
            var res = await _registryManager.UpdateTwinAsync(_deviceId, Name, twin, twin.ETag);
            typeTwin.SetTwin(name, res);
            return typeTwin;
        }
    }
}