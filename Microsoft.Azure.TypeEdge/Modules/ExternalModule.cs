using System.Collections.Generic;
using Microsoft.Azure.Devices.Shared;

namespace Microsoft.Azure.TypeEdge.Modules
{
    public abstract class ExternalModule : TypeModule
    {
        private readonly TwinCollection _defaultTwin;
        private readonly HostingSettings _settings;

        protected ExternalModule(string name,
            HostingSettings settings,
            TwinCollection defaultTwin,
            List<string> routes)
        {
            Name = name;
            _settings = settings;
            _defaultTwin = defaultTwin;
            Routes = routes;
            _settings.IsExternalModule = true;
        }

        public override string Name { get; }

        internal override TwinCollection DefaultTwin => _defaultTwin;
        internal override List<string> Routes { get; }

        internal override HostingSettings HostingSettings => _settings;
    }
}