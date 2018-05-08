using Autofac;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Client.Transport.Mqtt;
using Microsoft.Azure.IoT.EdgeCompose.Attributes;
using Microsoft.Azure.IoT.EdgeCompose.Hubs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
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
    public abstract class EdgeModule : IEdgeModule
    {
        public EdgeModule()
        {
            var props = GetType().GetProperties();

            foreach (var prop in props)
            {
                var type = prop.PropertyType;

                if (type.IsGenericType)
                    if (type.GetGenericTypeDefinition() == typeof(Input<>) || type.GetGenericTypeDefinition() == typeof(Output<>))
                    {
                        if (!prop.CanWrite)
                            throw new Exception($"{prop.Name} needs to be set dynamically, please define a setter.");
                        prop.SetValue(this, Activator.CreateInstance(type.GetGenericTypeDefinition().MakeGenericType(type.GenericTypeArguments)));
                    }
            }

        }
        public virtual string Name { get { return this.GetType().Name; } }

        public virtual CreationResult Create(IConfigurationRoot configuration)
        {
            return CreationResult.OK;
        }
        public virtual Task<InitializationResult> InitAsync()
        {
            return Task.FromResult(InitializationResult.OK);
        }
        public virtual Task<ExecutionResult> RunAsync()
        {
            return Task.FromResult(ExecutionResult.OK);
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

        public void DependsOn(IEdgeModule module)
        {
        }

        /*
    public void Configure(IConfigurationRoot configuration)
    {
        var edgeDeviceConnectionString = configuration.GetValue<string>(Constants.DeviceConnectionStringName);

        Options = new TOptions();
        Options.DeviceConnectionString = edgeDeviceConnectionString;

        PopulateOptions(configuration);
    }

    public virtual void PopulateOptions(IConfigurationRoot configuration)
    {
        //stadardize the custom options loading here
    }

    public void Subscribe(Upstream<TInputMessage> output)
    {

    }

    public void Subscribe<T>(Upstream<T> output, Func<T, TInputMessage> endpointTypeConverter)
        where T : IModuleMessage
    {
    }



    private static void InstallCert()
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

    public async Task<CreationResult> CreateAsync()
    {
        return await CreateHandler(Options);
    }

    public Task WhenCancelled(CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource<bool>();
        cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).SetResult(true), tcs);
        return tcs.Task;
    }

    public async Task StartAsync()
    {
        // The Edge runtime gives us the connection string we need -- it is injected as an environment variable
        string connectionString = Environment.GetEnvironmentVariable("EdgeHubConnectionString");
        Console.WriteLine("Connection String {0}", connectionString);

        // Cert verification is not yet fully functional when using Windows OS for the container
        bool bypassCertVerification = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        if (!bypassCertVerification)
            InstallCert();

        MqttTransportSettings mqttSetting = new MqttTransportSettings(Devices.Client.TransportType.Mqtt_Tcp_Only);

        // During dev you might want to bypass the cert verification. It is highly recommended to verify certs systematically in production
        if (bypassCertVerification)
            mqttSetting.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

        ITransportSettings[] settings = { mqttSetting };

        // Open a connection to the Edge runtime
        IoTHubModuleClient = DeviceClient.CreateFromConnectionString(connectionString, settings);

        await IoTHubModuleClient.OpenAsync();
        Console.WriteLine($"{Name} module DeviceClient initialized.");

        // Register callback to be called when a message is received by the module
        await IoTHubModuleClient.SetInputMessageHandlerAsync("Input", (e, u) => this.InputMessageHandlerAsync(e, u), this);

        // Wait until the app unloads or is cancelled
        var cts = new CancellationTokenSource();
        AssemblyLoadContext.Default.Unloading += (ctx) => cts.Cancel();

        await ExecuteHandler(Output);

        WhenCancelled(cts.Token).Wait();
    }


    public async Task<MessageResponse> InputMessageHandlerAsync(Devices.Client.Message message, object userContext)
    {
        var deviceClient = userContext as DeviceClient;
        if (deviceClient == null)
        {
            throw new InvalidOperationException("UserContext doesn't contain " + "expected values");
        }

        byte[] messageBytes = message.GetBytes();
        string messageString = Encoding.UTF8.GetString(messageBytes);
        Console.WriteLine($"Body: [{messageString}]");

        await IncomingMessageHandler(messageString, deviceClient);
        return MessageResponse.Completed;
    }

    internal string ModuleConnectionString { get { return $"{Options.DeviceConnectionString};{Agent.Constants.ModuleIdKey}={Name}"; } }

    public DeviceClient IoTHubModuleClient { get; private set; }
    */
    }
}
