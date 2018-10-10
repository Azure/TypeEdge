namespace Microsoft.Azure.TypeEdge.Host.Service
{
    public class CodeGeneratorSettings
    {
        public static CodeGeneratorSettings Default { get; } = new CodeGeneratorSettings
            {Namespace = "TypeEdge.Proxy", OutputPath = "./TypeEdge.Proxy"};

        public string Namespace { get; set; }
        public string OutputPath { get; set; }
        public Language Language { get; set; }
    }
}