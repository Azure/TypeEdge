using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Client.Transport.Mqtt;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Azure.IoT.TypeEdge.Hubs;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoT.TypeEdge.Modules
{
    public abstract class EdgeModule
    {
        private string connectionString;
        private DeviceClient ioTHubModuleClient;
        private ITransportSettings[] transportSettings;

        public virtual Task<T> PublishTwinAsync<T>(string name, T twin)
            where T : IModuleTwin, new()
        {
            throw new NotImplementedException();
        }

        public virtual Task<T> GetTwinAsync<T>(string name)
            where T : IModuleTwin, new()

        {
            throw new NotImplementedException();
        }

        private SubscriptionCallback twinSubscription;
        private readonly Dictionary<string, SubscriptionCallback> routeSubscriptions;

        public virtual string Name { get { return this.GetType().Name; } }
        public Upstream<JsonMessage> Upstream { get; set; }
        public List<string> Routes { get; set; }

        public EdgeModule()
        {
            routeSubscriptions = new Dictionary<string, SubscriptionCallback>();

            Routes = new List<string>();
            Upstream = new Upstream<JsonMessage>(this);

            CreateProperties();

        }
        public virtual void BuildSubscriptions()
        {
        }
        public virtual CreationResult Configure(IConfigurationRoot configuration)
        {
            return CreationResult.OK;
        }
        public virtual Task<ExecutionResult> RunAsync()
        {
            return Task.FromResult(ExecutionResult.OK);
        }
        public async Task<ExecutionResult> InternalRunAsync()
        {
            // Open a connection to the Edge runtime
            ioTHubModuleClient = DeviceClient.CreateFromConnectionString(connectionString, transportSettings);

            await ioTHubModuleClient.OpenAsync();
            Console.WriteLine("IoT Hub module client initialized.");

            // Register callback to be called when a message is received by the module
            foreach (var subscription in routeSubscriptions)
            {
                await ioTHubModuleClient.SetInputMessageHandlerAsync(subscription.Key, MessageHandler, subscription.Value);
            }

            if (twinSubscription != null)
                await ioTHubModuleClient.SetDesiredPropertyUpdateCallbackAsync(PropertyHandler, twinSubscription);

            return await RunAsync();
        }
        public CreationResult InternalConfigure(IConfigurationRoot configuration)
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
            twinSubscription = new SubscriptionCallback(name, handler, typeof(T));
        }

        private async Task<MessageResponse> MessageHandler(Devices.Client.Message message, object userContext)
        {
            if (!(userContext is SubscriptionCallback callback))
                throw new InvalidOperationException("UserContext doesn't contain a valid SubscriptionCallback");

            byte[] messageBytes = message.GetBytes();
            string messageString = Encoding.UTF8.GetString(messageBytes);
            Console.WriteLine($"Received message: Body: [{messageString}]");

            var input = Activator.CreateInstance(callback.MessageType) as IEdgeMessage;
            input.SetBytes(messageBytes);

            var invocationResult = callback.Handler.DynamicInvoke(input);
            var result = await ((Task<MessageResult>)invocationResult);

            if (result == MessageResult.OK)
                return MessageResponse.Completed;

            return MessageResponse.Abandoned;
        }
        private async Task PropertyHandler(TwinCollection desiredProperties, object userContext)
        {
            if (!(userContext is SubscriptionCallback callback))
                throw new InvalidOperationException("UserContext doesn't contain a valid SubscriptionCallback");

            Console.WriteLine("Desired property change:");
            Console.WriteLine(JsonConvert.SerializeObject(desiredProperties));

            var input = Activator.CreateInstance(callback.MessageType) as IModuleTwin;
            input.SetTwin(new Twin(new TwinProperties() { Desired = desiredProperties }));

            var invocationResult = callback.Handler.DynamicInvoke(input);
            var result = await ((Task<MessageResult>)invocationResult);
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
            Console.WriteLine("Added Cert: " + certPath);
            store.Close();
        }
        private void CreateProperties()
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

        internal async Task ReportTwinAsync<T>(T twin)
            where T : IModuleTwin
        {
            await ioTHubModuleClient.UpdateReportedPropertiesAsync(twin.GetTwin().Properties.Reported);
        }
    }
}
