using Microsoft.Azure.TypeEdge.Description;

namespace Microsoft.Azure.TypeEdge.Host.Service
{
    partial class Service
    {
        private readonly CodeGeneratorSettings _codeGeneratorSettings;
        private readonly ServiceDescription _serviceDescription;

        public Service(ServiceDescription serviceDescription, CodeGeneratorSettings codeGeneratorSettings)
        {
            _serviceDescription = serviceDescription;
            _codeGeneratorSettings = codeGeneratorSettings;
        }
    }
}