using Autofac;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Edge.Agent.Core;
using Microsoft.Azure.Devices.Edge.Agent.Core.Commands;
using Microsoft.Azure.Devices.Edge.Agent.Docker;
using Microsoft.Azure.Devices.Edge.Agent.Docker.Commands;
using Microsoft.Azure.Devices.Edge.Util;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TypeEdge.Enums;
using TypeEdge.Modules;
using TypeEdge.Modules.Enums;

namespace TypeEdge.Host.Docker
{
    public class DockerModule : ExternalModule
    {
        DockerClient _dockerClient;
        //ICommand _createCommand;
        //ICommand _startCommand;
        ICommandFactory _dockerFactory;
        IModuleWithIdentity _moduleWithIdentity;
        public DockerModule(string name, HostingSettings settings, TwinCollection defaultTwin, List<string> routes) : base(name, settings, defaultTwin, routes)
        {
        }
        public DockerHostingSettings DockerHostingSettings { get; set; }

        internal override InitializationResult _Init(IConfigurationRoot configuration, IContainer container)
        {
            _dockerClient = new DockerClientConfiguration(new Uri(configuration.GetValue<string>("DockerUri")))
                .CreateClient();

            var configSource = new EmulatorConfigSource(configuration);

            var dockerLoggingOptions = new Dictionary<string, string>
                {
                    {"max-size", "1m"},
                    {"max-file", "1" }
                };
            var loggingConfig = new DockerLoggingConfig("json-file", dockerLoggingOptions);

            var dockerAuthConfig = configuration.GetSection("DockerRegistryAuth").Get<List<AuthConfig>>() ?? new List<AuthConfig>();
            var combinedDockerConfigProvider = new CombinedDockerConfigProvider(dockerAuthConfig);
            //var runtimeInfoProvider = RuntimeInfoProvider.CreateAsync(_dockerClient);

            var dockerModule = new Microsoft.Azure.Devices.Edge.Agent.Docker.DockerModule(
                   Name,
                   DockerHostingSettings.Version,
                   DockerHostingSettings.DesiredStatus,
                   DockerHostingSettings.RestartPolicy,
                   DockerHostingSettings.Config,
                   null,
                   null
               );

            var connectionString = configuration.GetValue<string>(Microsoft.Azure.Devices.Edge.Agent.Core.Constants.EdgeHubConnectionStringKey);
            var connectionStringBuilder = IotHubConnectionStringBuilder.Create(connectionString);

            var moduleIdentity = new ModuleIdentity(connectionStringBuilder.IotHubName,
                Environment.MachineName,
                connectionStringBuilder.DeviceId,
                connectionStringBuilder.ModuleId,
                new ConnectionStringCredentials(connectionString));

            _moduleWithIdentity = new ModuleWithIdentity(dockerModule, moduleIdentity);
            //var combinedDockerConfig = combinedDockerConfigProvider.GetCombinedConfig(dockerModule, runtimeInfo);

            _dockerFactory = new LoggingCommandFactory(new DockerCommandFactory(_dockerClient,
                loggingConfig,
                configSource,
                combinedDockerConfigProvider),
                Logger.Factory) as ICommandFactory;

            //var updateCommand = new GroupCommand(
            //      new RemoveCommand(_dockerClient, dockerModule),
            //        new GroupCommand(
            //            new PullCommand(_dockerClient, combinedDockerConfig),
            //            CreateCommand.BuildAsync(_dockerClient,
            //            dockerModule,
            //            dockerModuleWithIdentity.ModuleIdentity,
            //            loggingConfig,
            //            configSource,
            //            false).Result));

            //_createCommand = new GroupCommand(
            //   new PullCommand(_dockerClient, combinedDockerConfig),
            //   CreateCommand.BuildAsync(_dockerClient,
            //   dockerModule,
            //   dockerModuleWithIdentity.ModuleIdentity,
            //   loggingConfig,
            //   configSource,
            //   false).Result);

            //_startCommand = dockerFactory.StartAsync(dockerModule).Result;
            return base._Init(configuration, container);
        }


        internal override async Task<ExecutionResult> _RunAsync(CancellationToken cancellationToken)
        {
            try
            {
                var containers = await _dockerClient.Containers.ListContainersAsync(new ContainersListParameters() { All = true });
                if (containers.Where(e => e.Image == (_moduleWithIdentity.Module as Microsoft.Azure.Devices.Edge.Agent.Docker.DockerModule).Config.Image).SingleOrDefault() != null)
                {
                    Console.WriteLine($"Removing {_moduleWithIdentity.Module.Name}...");
                    await (await _dockerFactory.RemoveAsync(_moduleWithIdentity.Module)).ExecuteAsync(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{_moduleWithIdentity.Module.Name} not found");
            }

            try
            {
                var runtimeConfig = new DockerRuntimeConfig("1.24.0", "{}");
                var runtimeInfo = new DockerRuntimeInfo("docker", runtimeConfig);
                Console.WriteLine($"Creating {_moduleWithIdentity.Module.Name}...");
                //_dockerClient.Volumes.CreateAsync(new VolumesCreateParameters() { Name = "vol1", Driver = "local",  })
                await (await _dockerFactory.CreateAsync(_moduleWithIdentity, runtimeInfo)).ExecuteAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            try
            {
                Console.WriteLine($"Starting {_moduleWithIdentity.Module.Name}...");

                await (await _dockerFactory.StartAsync(_moduleWithIdentity.Module)).ExecuteAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return await base._RunAsync(cancellationToken);
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                Console.WriteLine($"Removing {_moduleWithIdentity.Module.Name}...");
                _dockerFactory.RemoveAsync(_moduleWithIdentity.Module).Result.ExecuteAsync(new CancellationToken());
            }
            catch (Exception ex)
            {
            }

            base.Dispose(disposing);
        }
    }
}
