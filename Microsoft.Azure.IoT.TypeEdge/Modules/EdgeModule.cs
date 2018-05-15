using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Client.Transport.Mqtt;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Azure.IoT.TypeEdge.Attributes;
using Microsoft.Azure.IoT.TypeEdge.Hubs;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Castle.DynamicProxy;
using Microsoft.Azure.IoT.TypeEdge.Host;

[assembly: InternalsVisibleTo("Microsoft.Azure.IoT.TypeEdge.Host")]
[assembly: InternalsVisibleTo("Microsoft.Azure.IoT.TypeEdge.Proxy")]

namespace Microsoft.Azure.IoT.TypeEdge.Modules
{
    public abstract class EdgeModule
    {
        private string connectionString;
        private DeviceClient ioTHubModuleClient;
        private ITransportSettings[] transportSettings;

        private readonly Dictionary<string, SubscriptionCallback> twinSubscriptions;
        private readonly Dictionary<string, SubscriptionCallback> routeSubscriptions;
        private readonly Dictionary<string, MethodCallback> methodSubscriptions;

        protected Upstream<JsonMessage> Upstream { get; set; }
        internal virtual Task<T> PublishTwinAsync<T>(string name, T twin)
            where T : IModuleTwin, new()
        {
            ioTHubModuleClient.UpdateReportedPropertiesAsync(twin.GetReportedTwin(name).Properties.Reported);
            throw new NotImplementedException();
        }
        internal virtual async Task<T> GetTwinAsync<T>(string name)
            where T : IModuleTwin, new()

        {
            var typeTwin = Activator.CreateInstance<T>();
            typeTwin.SetTwin(name, await ioTHubModuleClient.GetTwinAsync());
            return typeTwin;
        }
        internal virtual string Name
        {
            get
            {
                var proxyInterface = GetType().GetProxyInterface();
                var typeModule = proxyInterface.GetCustomAttribute(typeof(TypeModuleAttribute), true) as TypeModuleAttribute;
                if (typeModule != null)
                    return typeModule.Name;
                return GetType().Name;
            }
        }
        internal List<string> Routes { get; set; }

        public EdgeModule()
        {
            routeSubscriptions = new Dictionary<string, SubscriptionCallback>();
            twinSubscriptions = new Dictionary<string, SubscriptionCallback>();
            methodSubscriptions = new Dictionary<string, MethodCallback>();

            Routes = new List<string>();
            Upstream = new Upstream<JsonMessage>(this);

            InstantiateProperties();

            RegisterMethods();
        }


       
        public virtual Task<ExecutionResult> RunAsync()
        {
            return Task.FromResult(ExecutionResult.OK);
        }
        public virtual CreationResult Configure(IConfigurationRoot configuration)
        {
            return CreationResult.OK;
        }
        public virtual void BuildSubscriptions()
        {
        }
        internal async Task<ExecutionResult> InternalRunAsync()
        {
            // Open a connection to the Edge runtime
            ioTHubModuleClient = DeviceClient.CreateFromConnectionString(connectionString, transportSettings);

            await ioTHubModuleClient.OpenAsync();
            Console.WriteLine($"IoT Hub module {Name} client initialized.");

            // Register callback to be called when a message is received by the module
            foreach (var subscription in routeSubscriptions)
            {
                await ioTHubModuleClient.SetInputMessageHandlerAsync(subscription.Key, MessageHandler, subscription.Value);
            }

            // Register callback to be called when a twin update is received by the module
            await ioTHubModuleClient.SetDesiredPropertyUpdateCallbackAsync(PropertyHandler, twinSubscriptions);

            foreach (var subscription in methodSubscriptions)
            {
                await ioTHubModuleClient.SetMethodHandlerAsync(subscription.Key, MethodCallback, subscription.Value);
            }
            return await RunAsync();
        }

        private Task<MethodResponse> MethodCallback(MethodRequest methodRequest, object userContext)
        {
            if (!(userContext is MethodCallback callback))
                throw new InvalidOperationException("UserContext doesn't contain a valid SubscriptionCallback");

            var paramValues = JsonConvert.DeserializeObject<object[]>(methodRequest.DataAsJson);

            try
            {
                var paramTypes = callback.MethodInfo.GetParameters();

                for (int i = 0; i < paramTypes.Length; i++)
                    paramValues[i] = Convert.ChangeType(paramValues[i], paramTypes[i].ParameterType);

                var res = callback.MethodInfo.Invoke(this, paramValues);
                return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(res)), 200));
            }
            catch (Exception ex)
            {
                // Acknowlege the direct method call with a 400 error message
                string result = "{\"result\":\"" + ex.Message + "\"}";
                return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(result), 400));
            }
        }
        internal CreationResult InternalConfigure(IConfigurationRoot configuration)
        {
            connectionString = configuration.GetValue<string>(Constants.EdgeHubConnectionStringKey);

            // Cert verification is not yet fully functional when using Windows OS for the container
            bool bypassCertVerification = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            if (!bypassCertVerification)
                InstallCert();

            MqttTransportSettings mqttSetting = new MqttTransportSettings(Devices.Client.TransportType.Mqtt_Tcp_Only);
            if (true)//bypassCertVerification)
            {
                mqttSetting.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
            }
            transportSettings = new ITransportSettings[] { mqttSetting };

            return Configure(configuration);
        }

        internal async Task<PublishResult> PublishMessageAsync<T>(string outputName, T message)
            where T : IEdgeMessage
        {
            var edgeMessage = new Devices.Client.Message(message.GetBytes());
            if (message.Properties != null)
                foreach (var prop in edgeMessage.Properties)
                {
                    edgeMessage.Properties.Add(prop.Key, prop.Value);
                }

            await ioTHubModuleClient.SendEventAsync(outputName, edgeMessage);

            string messageString = Encoding.UTF8.GetString(message.GetBytes());
            Console.WriteLine($"{Name}:Sent message: Body: [{messageString}]");

            return PublishResult.OK;
        }
        internal void SubscribeRoute<T>(string outName, string outRoute, string inName, string inRoute, Func<T, Task<MessageResult>> handler)
            where T : IEdgeMessage
        {
            if (outRoute != "$downstream")
                Routes.Add($"FROM {outRoute} INTO {inRoute}");

            routeSubscriptions[inName] = new SubscriptionCallback(inName, handler, typeof(T));
        }
        internal void SubscribeRoute(string outName, string outRoute, string inName, string inRoute)
        {
            if (outRoute != "$downstream")
                Routes.Add($"FROM {outRoute} INTO {inRoute}");
        }
        internal void SubscribeTwin<T>(string name, Func<T, Task<TwinResult>> handler) where T : IModuleTwin
        {
            twinSubscriptions[name] = new SubscriptionCallback(name, handler, typeof(T));
        }

        private async Task<MessageResponse> MessageHandler(Devices.Client.Message message, object userContext)
        {
            if (!(userContext is SubscriptionCallback callback))
                throw new InvalidOperationException("UserContext doesn't contain a valid SubscriptionCallback");

            byte[] messageBytes = message.GetBytes();
            string messageString = Encoding.UTF8.GetString(messageBytes);
            Console.WriteLine($"{Name}:Received message: Body: [{messageString}]");

            var input = Activator.CreateInstance(callback.Type) as IEdgeMessage;
            input.SetBytes(messageBytes);

            var invocationResult = callback.Handler.DynamicInvoke(input);
            var result = await ((Task<MessageResult>)invocationResult);

            if (result == MessageResult.OK)
                return MessageResponse.Completed;

            return MessageResponse.Abandoned;
        }
        private async Task PropertyHandler(TwinCollection desiredProperties, object userContext)
        {
            if (!(userContext is Dictionary<string, SubscriptionCallback> callbacks))
                throw new InvalidOperationException("UserContext doesn't contain a valid SubscriptionCallback");

            Console.WriteLine($"{Name}:Desired property change:");
            Console.WriteLine(JsonConvert.SerializeObject(desiredProperties));

            foreach (var callback in callbacks)
            {
                if (desiredProperties.Contains($"___{callback.Key}"))
                {
                    var input = Activator.CreateInstance(callback.Value.Type) as IModuleTwin;
                    input.SetTwin(callback.Key, new Twin(new TwinProperties() { Desired = desiredProperties }));

                    var invocationResult = callback.Value.Handler.DynamicInvoke(input);
                    var result = await ((Task<TwinResult>)invocationResult);
                }
            }
        }
        private void InstallCert()
        {
            string certPath = Environment.GetEnvironmentVariable("EdgeModuleCACertificateFile");
            if (string.IsNullOrWhiteSpace(certPath))
            {
                // We cannot proceed further without a proper cert file
                Console.WriteLine($"Missing path to certificate collection file: {certPath}");
                throw new InvalidOperationException("Missing path to certificate file.");
            }
            else if (!File.Exists(certPath))
            {
                // We cannot proceed further without a proper cert file
                Console.WriteLine($"Missing path to certificate collection file: {certPath}");
                throw new InvalidOperationException("Missing certificate file.");
            }
            X509Store store = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadWrite);
            store.Add(new X509Certificate2(X509Certificate2.CreateFromCertFile(certPath)));
            Console.WriteLine($"{Name}:Added Cert: " + certPath);
            store.Close();
        }
        private void InstantiateProperties()
        {
            var props = GetType().GetProperties();

            foreach (var prop in props)
            {
                var type = prop.PropertyType;

                if (!type.IsGenericType)
                    continue;

                var genericDef = type.GetGenericTypeDefinition();
                if (genericDef == typeof(Input<>)
                    || genericDef == typeof(Output<>)
                    || genericDef == typeof(ModuleTwin<>))
                {
                    if (!prop.CanWrite)
                        throw new Exception($"{prop.Name} needs to be set dynamically, please define a setter.");

                    var name = $"{prop.Name}";
                    var value = Activator.CreateInstance(type.GetGenericTypeDefinition().MakeGenericType(type.GenericTypeArguments), name, this);
                    prop.SetValue(this, value);
                }
            }
        }

        private void RegisterMethods()
        {
            var interfaceType = GetType().GetProxyInterface();
            if (interfaceType != null)
            {
                var moduleMethods = GetType().GetInterfaceMap(interfaceType).TargetMethods.Where(e => !e.IsSpecialName);
                foreach (var method in moduleMethods)
                {
                    methodSubscriptions[method.Name] = new MethodCallback(method.Name, method);
                }
            }
        }

        internal async Task ReportTwinAsync<T>(string name, T twin)
            where T : IModuleTwin
        {
            await ioTHubModuleClient.UpdateReportedPropertiesAsync(twin.GetReportedTwin(name).Properties.Reported);
        }
    }
}
