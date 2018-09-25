using Microsoft.Azure.Devices.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Azure.TypeEdge.Modules
{
    public abstract class ExternalModule : TypeModule 
    {
        private readonly string _name;
        HostingSettings _settings;
        TwinCollection _defaultTwin;
        List<string> _routes;

        public ExternalModule(string name,
            HostingSettings settings,
            TwinCollection defaultTwin,
            List<string> routes)
        {
            _name = name;
            _settings = settings;
            _defaultTwin = defaultTwin;
            _routes = routes;
            _settings.IsExternalModule = true;
        }

        public override string Name => _name;
        internal override TwinCollection DefaultTwin => _defaultTwin;
        internal override List<string> Routes => _routes;
        internal override HostingSettings HostingSettings => _settings;
    }
}
