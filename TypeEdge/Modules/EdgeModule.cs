using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Client.Transport.Mqtt;
using Microsoft.Azure.Devices.Shared;
using TypeEdge.Enums;
using TypeEdge.Modules.Endpoints;
using TypeEdge.Modules.Enums;
using TypeEdge.Modules.Messages;
using TypeEdge.Twins;
using TypeEdge.Volumes;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Autofac;
using Castle.DynamicProxy;
using TypeEdge.Proxy;

[assembly: InternalsVisibleTo("TypeEdge.Host")]
[assembly: InternalsVisibleTo("TypeEdge.Proxy")]

namespace TypeEdge.Modules
{
    public abstract class EdgeModule : IDisposable
    {
        #region members
        private readonly Dictionary<string, MethodCallback> _methodSubscriptions;
        private readonly Dictionary<string, SubscriptionCallback> _routeSubscriptions;
        private readonly Dictionary<string, SubscriptionCallback> _twinSubscriptions;
        private string _connectionString;
        private ModuleClient _ioTHubModuleClient;
        private ITransportSettings[] _transportSettings;
        #endregion

        protected EdgeModule()
        {
            _routeSubscriptions = new Dictionary<string, SubscriptionCallback>();
            _twinSubscriptions = new Dictionary<string, SubscriptionCallback>();
            _methodSubscriptions = new Dictionary<string, MethodCallback>();
            Volumes = new Dictionary<string, string>();

            Routes = new List<string>();

            InstantiateProperties();
        }

        #region properties
        internal virtual string Name
        {
            get
            {
                var proxyInterface = GetType().GetProxyInterface();

                if (proxyInterface == null)
                    throw new ArgumentException($"{GetType().Name} has needs to implement an single interface annotated with the TypeModule Attribute");

                return proxyInterface.Name.Substring(1).ToLower(CultureInfo.CurrentCulture);
            }
        }
        public Dictionary<string, string> Volumes { get; }
        internal List<string> Routes { get; set; }
        #endregion

        #region virtual methods

        internal virtual async Task<T> PublishTwinAsync<T>(string name, T twin)
            where T : TypeTwin, new()
        {
            await _ioTHubModuleClient.UpdateReportedPropertiesAsync(twin.GetReportedProperties());
            return twin;
        }

        internal virtual async Task<T> GetTwinAsync<T>(string name)
            where T : TypeTwin, new()
        {
            var twin = await _ioTHubModuleClient.GetTwinAsync();
            return TypeTwin.CreateTwin<T>(name, twin);
        }

        public virtual Task<ExecutionResult> RunAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(ExecutionResult.Ok);
        }

        public virtual InitializationResult Init()
        {
            return InitializationResult.Ok;
        }

        protected virtual void Dispose(bool disposing)
        {
        }
        #endregion


        protected T GetProxy<T>()
            where T : class
        {
            var cb = new ContainerBuilder();
            cb.RegisterInstance(new ProxyGenerator()
                .CreateInterfaceProxyWithoutTarget<T>(new ModuleProxy<T>()));
            return cb.Build().Resolve<T>();
        }

        internal async Task<ExecutionResult> _RunAsync(CancellationToken cancellationToken)
        {
            RegisterMethods();

            // Open a connection to the Edge runtime
            _ioTHubModuleClient = ModuleClient.CreateFromConnectionString(_connectionString, _transportSettings);

            await _ioTHubModuleClient.OpenAsync();
            //Console.WriteLine($"{Name}:IoT Hub module client initialized.");

            // Register callback to be called when a message is received by the module
            foreach (var subscription in _routeSubscriptions)
            {
                await _ioTHubModuleClient.SetInputMessageHandlerAsync(subscription.Key, MessageHandler,
                    subscription.Value);

                //Console.WriteLine($"{Name}:MessageHandler set for {subscription.Key}");
            }

            // Register callback to be called when a twin update is received by the module
            await _ioTHubModuleClient.SetDesiredPropertyUpdateCallbackAsync(PropertyHandler, _twinSubscriptions);

            foreach (var subscription in _methodSubscriptions)
            {
                await _ioTHubModuleClient.SetMethodHandlerAsync(subscription.Key, MethodCallback, subscription.Value);
                //Console.WriteLine($"{Name}:MethodCallback set for{subscription.Key}");
            }

            //Console.WriteLine($"{Name}:Running RunAsync..");
            return await RunAsync(cancellationToken);
        }

        internal InitializationResult _Init(IConfigurationRoot configuration, IContainer container)
        {
            //Console.WriteLine($"{Name}:InternalConfigure called");

            _connectionString = configuration.GetValue<string>($"{Constants.EdgeHubConnectionStringKey}");
            if (string.IsNullOrEmpty(_connectionString))
                throw new ArgumentException($"Missing {Constants.EdgeHubConnectionStringKey} in configuration for {Name}");

            // Cert verification is not yet fully functional when using Windows OS for the container
            var bypassCertVerification = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            if (!bypassCertVerification)
                InstallCert();

            var settings = new AmqpTransportSettings(TransportType.Amqp_Tcp_Only);
            if (true) //bypassCertVerification)
                settings.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) =>
                true;
            _transportSettings = new ITransportSettings[] { settings };

            return Init();
        }

        internal async Task<PublishResult> PublishMessageAsync<T>(string outputName, T message)
            where T : IEdgeMessage
        {
            //Console.WriteLine($"{Name}:PublishMessageAsync called");
            var edgeMessage = new Message(message.GetBytes());


            if (message.Properties != null)
                foreach (var prop in edgeMessage.Properties)
                    edgeMessage.Properties.Add(prop.Key, prop.Value);

            await _ioTHubModuleClient.SendEventAsync(outputName, edgeMessage);

            var messageString = Encoding.UTF8.GetString(message.GetBytes());
            //Console.WriteLine($">>>>>>{Name}: message: Body: [{messageString}]");

            return PublishResult.Ok;
        }

        internal void SubscribeRoute<T>(string outName, string outRoute, string inName, string inRoute,
            Func<T, Task<MessageResult>> handler)
            where T : IEdgeMessage
        {
            //Console.WriteLine($"{Name}:SubscribeRoute called");

            if (outRoute != "$downstream")
                Routes.Add($"FROM {outRoute} INTO {inRoute}");

            if (_routeSubscriptions.Keys.Contains(inName))
                throw new Exception($"Only one subscription allowed with name {inName} in Module {Name}");

            _routeSubscriptions[inName] = new SubscriptionCallback(inName, handler, typeof(T));
        }

        internal void SubscribeRoute(string outName, string outRoute, string inName, string inRoute)
        {
            //Console.WriteLine($"{Name}:SubscribeRoute called");

            if (outRoute != "$downstream")
                Routes.Add($"FROM {outRoute} INTO {inRoute}");
        }

        internal void SubscribeTwin<T>(string name, Func<T, Task<TwinResult>> handler) where T : TypeTwin
        {
            //Console.WriteLine($"{Name}:SubscribeTwin called");

            _twinSubscriptions[name] = new SubscriptionCallback(name, handler, typeof(T));
        }

        internal async Task ReportTwinAsync<T>(string name, T twin)
            where T : TypeTwin
        {
            //Console.WriteLine($"{Name}:ReportTwinAsync called");
            await _ioTHubModuleClient.UpdateReportedPropertiesAsync(twin.GetReportedProperties());
        }

        internal void RegisterVolume(string volumeName)
        {
            if (Volumes.Keys.Contains(volumeName))
                return;

            var volumePath = volumeName.ToLower();

            if (!Directory.Exists(volumePath))
            {
                var di = Directory.CreateDirectory(volumePath);
            }

            Volumes[volumeName] = volumePath;
        }

        internal T GetReferenceData<T>(string name, string index)
            where T : class, IEdgeMessage, new()
        {
            try
            {
                var path = Path.Combine(Volumes[name], index);
                if (File.Exists(path))
                {
                    var result = new T();
                    var bytes = File.ReadAllBytes(path);
                    result.SetBytes(bytes);
                    return result;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{Name}:ERROR:{ex}");
            }
            return null;
        }

        internal bool SetReferenceData<T>(string name, string index, T value)
            where T : class, IEdgeMessage, new()
        {
            try
            {
                var path = Path.Combine(Volumes[name], index);
                var bytes = value.GetBytes();

                File.WriteAllBytes(path, bytes);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{Name}:ERROR:{ex}");
            }
            return false;
        }

        internal bool DeleteReferenceData(string name, string index)
        {
            try
            {
                var path = Path.Combine(Volumes[name], index);
                if (File.Exists(path))
                    File.Delete(path);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{Name}:ERROR:{ex}");
            }
            return false;
        }

        public void Dispose()
        {
            Dispose(true);

            if (Volumes != null)
                foreach (var item in Volumes)
                {
                    try
                    {
                        //todo:empty or not?
                        //if (Directory.Exists(item.Value))
                        //    Directory.Delete(item.Value);
                    }
                    catch { }
                }

            GC.SuppressFinalize(this);
        }
        #region private methods

        private Task<MethodResponse> MethodCallback(MethodRequest methodRequest, object userContext)
        {
            //Console.WriteLine($"{Name}:MethodCallback called");
            if (!(userContext is MethodCallback callback))
                throw new InvalidOperationException($"{Name}:UserContext doesn't contain a valid SubscriptionCallback");

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
                Console.WriteLine($"{Name}:ERROR:{ex}");

                // Acknowlege the direct method call with a 400 error message
                var result = "{\"result\":\"" + ex.Message + "\"}";
                return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(result), 400));
            }
        }

        private async Task<MessageResponse> MessageHandler(Message message, object userContext)
        {
            //Console.WriteLine($"{Name}:MessageHandler called");

            if (!(userContext is SubscriptionCallback callback))
                throw new InvalidOperationException("UserContext doesn't contain a valid SubscriptionCallback");

            var messageBytes = message.GetBytes();
            var messageString = Encoding.UTF8.GetString(messageBytes);
            //Console.WriteLine($"<<<<<<{Name}:message: Body: [{messageString}]");

            if (!(Activator.CreateInstance(callback.Type) is IEdgeMessage input))
            {
                Console.WriteLine($"{Name}:Abandoned Message");
                return MessageResponse.Abandoned;
            }

            input.SetBytes(messageBytes);

            var invocationResult = callback.Handler.DynamicInvoke(input);
            var result = await (Task<MessageResult>)invocationResult;

            return result == MessageResult.Ok ? MessageResponse.Completed : MessageResponse.Abandoned;
        }

        private async Task PropertyHandler(TwinCollection desiredProperties, object userContext)
        {
            //Console.WriteLine($"{Name}:PropertyHandler called");

            if (!(userContext is Dictionary<string, SubscriptionCallback> callbacks))
                throw new InvalidOperationException("UserContext doesn't contain a valid SubscriptionCallback");

            //Console.WriteLine($"{Name}:Desired property change:");
            //Console.WriteLine(JsonConvert.SerializeObject(desiredProperties));

            foreach (var callback in callbacks)
                if (desiredProperties.Contains($"___{callback.Key}"))
                {
                    var input = TypeTwin.CreateTwin(callback.Value.Type, callback.Key, desiredProperties);

                    var invocationResult = callback.Value.Handler.DynamicInvoke(input);
                    await (Task<TwinResult>)invocationResult;
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
            //Console.WriteLine($"{Name}:Added Cert: " + certPath);
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
                    || genericDef == typeof(ModuleTwin<>)
                    || genericDef == typeof(Volume<>))
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
            //Console.WriteLine($"{Name}:RegisterMethods called");
            var interfaceType = GetType().GetProxyInterface();

            if (interfaceType == null)
                return;

            var moduleMethods = GetType().GetInterfaceMap(interfaceType).TargetMethods.Where(e => !e.IsSpecialName);
            foreach (var method in moduleMethods)
                _methodSubscriptions[method.Name] = new MethodCallback(method.Name, method);
        }

        #endregion
    }
}