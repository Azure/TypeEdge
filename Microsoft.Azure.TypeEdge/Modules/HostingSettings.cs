using Newtonsoft.Json;
using System.Collections.Generic;
using Microsoft.Azure.TypeEdge.Modules.Enums;

namespace Microsoft.Azure.TypeEdge.Modules
{
    public class HostingSettings
    {
        public HostingSettings(string imageName, Dictionary<string, object> options)
        {
            ImageName = imageName;
            Options = options;
        }

        public bool IsExternalModule { get; internal set; }

        public virtual string Version => "1.0";

        public virtual string Type => "docker";

        public virtual ModuleStatus DesiredStatus => ModuleStatus.Running;

        public virtual RestartPolicy RestartPolicy => RestartPolicy.OnFailure;

        public string ImageName { get; set; }

        public Dictionary<string, object> Options { get; set; }
    }
}
