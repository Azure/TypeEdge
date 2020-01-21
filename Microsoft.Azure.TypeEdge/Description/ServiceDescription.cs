using System.Collections.Generic;
using System.Data;

namespace Microsoft.Azure.TypeEdge.Description
{
    public class ServiceDescription
    {
        public ServiceDescription(string name,
            List<EndpointDescription> inputDescriptions,
            List<EndpointDescription> outputDescriptions,
            List<TwinDescription> twinDescriptions,
            List<DirectMethodDescription> directMethodDescriptions)
        {
            Name = name;
            InputDescriptions = inputDescriptions;
            OutputDescriptions = outputDescriptions;
            TwinDescriptions = twinDescriptions;
            DirectMethodDescriptions = directMethodDescriptions;
        }
        public string Name { get; }
        public List<EndpointDescription> InputDescriptions { get; }
        public List<EndpointDescription> OutputDescriptions { get; }
        public List<TwinDescription> TwinDescriptions { get; }
        public List<DirectMethodDescription> DirectMethodDescriptions { get; }
    }
}