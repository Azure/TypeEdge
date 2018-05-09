using Autofac;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Client.Transport.Mqtt;
using Microsoft.Azure.IoT.EdgeCompose.Attributes;
using Microsoft.Azure.IoT.EdgeCompose.Hubs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Agent = Microsoft.Azure.Devices.Edge.Agent.Core;

namespace Microsoft.Azure.IoT.EdgeCompose.Modules
{
    public abstract class EdgeModule
    {
        internal List<string> Routes { get; set; }

        private string ConnectionString { get; set; }
        private DeviceClient IoTHubModuleClient { get; set; }
        private ITransportSettings[] TransportSettings { get; set; }

        private Dictionary<string, MessageCallback> Subscriptions { get; set; }

        public EdgeModule()
        {
            Subscriptions = new Dictionary<string, MessageCallback>();
            Routes = new List<string>();

            var props = GetType().GetProperties();

            foreach (var prop in props)
            {
                var type = prop.PropertyType;

                if (type.IsGenericType)
                    if (type.GetGenericTypeDefinition() == typeof(Input<>) || type.GetGenericTypeDefinition() == typeof(Output<>))
                    {
                        if (!prop.CanWrite)
                            throw new Exception($"{prop.Name} needs to be set dynamically, please define a setter.");

                        var name = $"{prop.Name}";
                        var value = Activator.CreateInstance(type.GetGenericTypeDefinition().MakeGenericType(type.GenericTypeArguments), name, this);
                        prop.SetValue(this, value);
                    }
            }

        }
        public virtual string Name { get { return this.GetType().Name; } }

        public virtual CreationResult Configure(IConfigurationRoot configuration)
        {
            return CreationResult.OK;
        }
        internal CreationResult InternalConfigure(IConfigurationRoot configuration)
        {
            ConnectionString = configuration.GetValue<string>(Agent.Constants.EdgeHubConnectionStringKey);

            // Cert verification is not yet fully functional when using Windows OS for the container
            bool bypassCertVerification = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            if (!bypassCertVerification)
                InstallCert();

            Console.WriteLine("Connection String {0}", ConnectionString);

            MqttTransportSettings mqttSetting = new MqttTransportSettings(Devices.Client.TransportType.Mqtt_Tcp_Only);
            // During dev you might want to bypass the cert verification. It is highly recommended to verify certs systematically in production
            if (bypassCertVerification)
            {
                mqttSetting.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
            }
            TransportSettings = new ITransportSettings[] { mqttSetting };

            return Configure(configuration);
        }

        void InstallCert()
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

        internal async Task<ExecutionResult> InternalRunAsync()
        {
            // Open a connection to the Edge runtime
            IoTHubModuleClient = DeviceClient.CreateFromConnectionString(ConnectionString, TransportSettings);
            await IoTHubModuleClient.OpenAsync();
            Console.WriteLine("IoT Hub module client initialized.");

            // Register callback to be called when a message is received by the module
            foreach (var subscription in Subscriptions)
            {
                await IoTHubModuleClient.SetInputMessageHandlerAsync(subscription.Key, SubscribedMessageHandler, subscription.Value);
            }
            return await RunAsync();
        }
        public virtual Task<ExecutionResult> RunAsync()
        {
            return Task.FromResult(ExecutionResult.OK);
        }

        private async Task<MessageResponse> SubscribedMessageHandler(Devices.Client.Message message, object userContext)
        {
            var inputRoute = userContext as string;
            if (string.IsNullOrEmpty(inputRoute))
                throw new InvalidOperationException("UserContext doesn't contain a valid input route");

            byte[] messageBytes = message.GetBytes();
            string messageString = Encoding.UTF8.GetString(messageBytes);
            Console.WriteLine($"Received message: Body: [{messageString}]");

            var subscription = Subscriptions[inputRoute];

            if (subscription == null)
                throw new InvalidOperationException("No subscription found for this input route");

            var input = Activator.CreateInstance(subscription.MessageType);
            var result = await subscription.Handler(input);

            if (result is MessageResult && ((MessageResult)result) == MessageResult.OK)
                return MessageResponse.Completed;

            return MessageResponse.Abandoned;
        }

        public virtual Task<TwinResult> TwinHandler(ModuleTwin newTwin)
        {
            return Task.FromResult(TwinResult.OK);
        }
        public virtual Task<PropertiesResult> PropertiesHandler(ModuleProperties newProps)
        {
            return Task.FromResult(PropertiesResult.OK);
        }
        public Input<JsonMessage> DefaultInput { get; set; }
        public Output<JsonMessage> DefaultOutput { get; set; }

        public void DependsOn(EdgeModule module)
        {
        }

        public async Task<PublishResult> PublishAsync<T>(string outputName, T message)
            where T : IEdgeMessage
        {
            var edgeMessage = new Devices.Client.Message(message.GetBytes());
            if (message.Properties != null)
                foreach (var prop in edgeMessage.Properties)
                {
                    edgeMessage.Properties.Add(prop.Key, prop.Value);
                }

            await IoTHubModuleClient.SendEventAsync(outputName, edgeMessage);

            Console.WriteLine("Received message sent");
            return PublishResult.OK;
        }

        public void Subscribe<T>(string outName, string outRoute, string inName, string inRoute, Func<T, Task<MessageResult>> handler)
            where T : IEdgeMessage
        {
            if(outRoute != "$downstream")
                Routes.Add($"FROM {outRoute} INTO {inRoute}");
            Subscriptions[inName] = new MessageCallback(inName, handler.GetMethodInfo(), handler, typeof(T));
        }
        
    }
}
