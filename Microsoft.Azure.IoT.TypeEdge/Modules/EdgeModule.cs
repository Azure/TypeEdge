using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Client.Transport.Mqtt;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Azure.IoT.TypeEdge.Attributes;
using Microsoft.Azure.IoT.TypeEdge.Enums;
using Microsoft.Azure.IoT.TypeEdge.Modules.Endpoints;
using Microsoft.Azure.IoT.TypeEdge.Modules.Enums;
using Microsoft.Azure.IoT.TypeEdge.Modules.Messages;
using Microsoft.Azure.IoT.TypeEdge.Twins;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

[assembly: InternalsVisibleTo("Microsoft.Azure.IoT.TypeEdge.Host")]
[assembly: InternalsVisibleTo("Microsoft.Azure.IoT.TypeEdge.Proxy")]

namespace Microsoft.Azure.IoT.TypeEdge.Modules
{
    public abstract class EdgeModule
    {
        private readonly Dictionary<string, MethodCallback> _methodSubscriptions;
        private readonly Dictionary<string, SubscriptionCallback> _routeSubscriptions;

        private readonly Dictionary<string, SubscriptionCallback> _twinSubscriptions;
        private string _connectionString;
        private DeviceClient _ioTHubModuleClient;
        private ITransportSettings[] _transportSettings;

        protected EdgeModule()
        {
            _routeSubscriptions = new Dictionary<string, SubscriptionCallback>();
            _twinSubscriptions = new Dictionary<string, SubscriptionCallback>();
            _methodSubscriptions = new Dictionary<string, MethodCallback>();

            Routes = new List<string>();


            InstantiateProperties();

            RegisterMethods();
        }

        internal virtual string Name
        {
            get
            {
                var proxyInterface = GetType().GetProxyInterface();
                var typeModule =
                    proxyInterface.GetCustomAttribute(typeof(TypeModuleAttribute), true) as TypeModuleAttribute;
                if (typeModule != null)
                    return typeModule.Name;
                return GetType().Name;
            }
        }

        internal List<string> Routes { get; set; }


        internal virtual Task<T> PublishTwinAsync<T>(string name, T twin)
            where T : IModuleTwin, new()
        {
            _ioTHubModuleClient.UpdateReportedPropertiesAsync(twin.GetReportedTwin(name).Properties.Reported);
            throw new NotImplementedException();
        }

        internal virtual async Task<T> GetTwinAsync<T>(string name)
            where T : IModuleTwin, new()

        {
            var typeTwin = Activator.CreateInstance<T>();
            typeTwin.SetTwin(name, await _ioTHubModuleClient.GetTwinAsync());
            return typeTwin;
        }


        public virtual Task<ExecutionResult> RunAsync()
        {
            return Task.FromResult(ExecutionResult.Ok);
        }

        public virtual CreationResult Configure(IConfigurationRoot configuration)
        {
            return CreationResult.Ok;
        }

        public virtual void BuildSubscriptions()
        {
        }

        internal async Task<ExecutionResult> InternalRunAsync()
        {
            // Open a connection to the Edge runtime
            _ioTHubModuleClient = DeviceClient.CreateFromConnectionString(_connectionString, _transportSettings);

            await _ioTHubModuleClient.OpenAsync();
            Console.WriteLine($"IoT Hub module {Name} client initialized.");

            // Register callback to be called when a message is received by the module
            foreach (var subscription in _routeSubscriptions)
                await _ioTHubModuleClient.SetInputMessageHandlerAsync(subscription.Key, MessageHandler,
                    subscription.Value);

            // Register callback to be called when a twin update is received by the module
            await _ioTHubModuleClient.SetDesiredPropertyUpdateCallbackAsync(PropertyHandler, _twinSubscriptions);

            foreach (var subscription in _methodSubscriptions)
                await _ioTHubModuleClient.SetMethodHandlerAsync(subscription.Key, MethodCallback, subscription.Value);
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

                for (var i = 0; i < paramTypes.Length; i++)
                    paramValues[i] = Convert.ChangeType(paramValues[i], paramTypes[i].ParameterType);

                var res = callback.MethodInfo.Invoke(this, paramValues);
                return Task.FromResult(
                    new MethodResponse(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(res)), 200));
            }
            catch (Exception ex)
            {
                // Acknowlege the direct method call with a 400 error message
                var result = "{\"result\":\"" + ex.Message + "\"}";
                return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(result), 400));
            }
        }

        internal CreationResult InternalConfigure(IConfigurationRoot configuration)
        {
            _connectionString = configuration.GetValue<string>(Constants.EdgeHubConnectionStringKey);

            // Cert verification is not yet fully functional when using Windows OS for the container
            var bypassCertVerification = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            if (!bypassCertVerification)
                InstallCert();

            var mqttSetting = new MqttTransportSettings(TransportType.Mqtt_Tcp_Only);
            if (true) //bypassCertVerification)
                mqttSetting.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
            _transportSettings = new ITransportSettings[] {mqttSetting};

            return Configure(configuration);
        }

        internal async Task<PublishResult> PublishMessageAsync<T>(string outputName, T message)
            where T : IEdgeMessage
        {
            var edgeMessage = new Message(message.GetBytes());
            if (message.Properties != null)
                foreach (var prop in edgeMessage.Properties)
                    edgeMessage.Properties.Add(prop.Key, prop.Value);

            await _ioTHubModuleClient.SendEventAsync(outputName, edgeMessage);

            var messageString = Encoding.UTF8.GetString(message.GetBytes());
            Console.WriteLine($">>>>>>{Name}: message: Body: [{messageString}]");

            return PublishResult.Ok;
        }

        internal void SubscribeRoute<T>(string outName, string outRoute, string inName, string inRoute,
            Func<T, Task<MessageResult>> handler)
            where T : IEdgeMessage
        {
            if (outRoute != "$downstream")
                Routes.Add($"FROM {outRoute} INTO {inRoute}");

            _routeSubscriptions[inName] = new SubscriptionCallback(inName, handler, typeof(T));
        }

        internal void SubscribeRoute(string outName, string outRoute, string inName, string inRoute)
        {
            if (outRoute != "$downstream")
                Routes.Add($"FROM {outRoute} INTO {inRoute}");
        }

        internal void SubscribeTwin<T>(string name, Func<T, Task<TwinResult>> handler) where T : IModuleTwin
        {
            _twinSubscriptions[name] = new SubscriptionCallback(name, handler, typeof(T));
        }

        private async Task<MessageResponse> MessageHandler(Message message, object userContext)
        {
            if (!(userContext is SubscriptionCallback callback))
                throw new InvalidOperationException("UserContext doesn't contain a valid SubscriptionCallback");

            var messageBytes = message.GetBytes();
            var messageString = Encoding.UTF8.GetString(messageBytes);
            Console.WriteLine($"<<<<<<{Name}:message: Body: [{messageString}]");

            if (!(Activator.CreateInstance(callback.Type) is IEdgeMessage input))
                return MessageResponse.Abandoned;
            input.SetBytes(messageBytes);

            var invocationResult = callback.Handler.DynamicInvoke(input);
            var result = await (Task<MessageResult>) invocationResult;

            return result == MessageResult.Ok ? MessageResponse.Completed : MessageResponse.Abandoned;
        }

        private async Task PropertyHandler(TwinCollection desiredProperties, object userContext)
        {
            if (!(userContext is Dictionary<string, SubscriptionCallback> callbacks))
                throw new InvalidOperationException("UserContext doesn't contain a valid SubscriptionCallback");

            Console.WriteLine($"{Name}:Desired property change:");
            Console.WriteLine(JsonConvert.SerializeObject(desiredProperties));

            foreach (var callback in callbacks)
                if (desiredProperties.Contains($"___{callback.Key}"))
                {
                    var input = Activator.CreateInstance(callback.Value.Type) as IModuleTwin;
                    input.SetTwin(callback.Key, new Twin(new TwinProperties {Desired = desiredProperties}));

                    var invocationResult = callback.Value.Handler.DynamicInvoke(input);
                    var result = await (Task<TwinResult>) invocationResult;
                }
        }

        private void InstallCert()
        {
            var certPath = Environment.GetEnvironmentVariable("EdgeModuleCACertificateFile");
            if (string.IsNullOrWhiteSpace(certPath))
            {
                // We cannot proceed further without a proper cert file
                Console.WriteLine($"Missing path to certificate collection file: {certPath}");
                throw new InvalidOperationException("Missing path to certificate file.");
            }

            if (!File.Exists(certPath))
            {
                // We cannot proceed further without a proper cert file
                Console.WriteLine($"Missing path to certificate collection file: {certPath}");
                throw new InvalidOperationException("Missing certificate file.");
            }

            var store = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadWrite);
            store.Add(new X509Certificate2(X509Certificate.CreateFromCertFile(certPath)));
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
                    var value = Activator.CreateInstance(
                        type.GetGenericTypeDefinition().MakeGenericType(type.GenericTypeArguments), name, this);
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
                    _methodSubscriptions[method.Name] = new MethodCallback(method.Name, method);
            }
        }

        internal async Task ReportTwinAsync<T>(string name, T twin)
            where T : IModuleTwin
        {
            await _ioTHubModuleClient.UpdateReportedPropertiesAsync(twin.GetReportedTwin(name).Properties.Reported);
        }
    }
}