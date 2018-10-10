using System;
using System.IO;
using Microsoft.Azure.TypeEdge.Description;

namespace Microsoft.Azure.TypeEdge.Host.Service
{
    public class ServiceGenerator
    {
        public static bool CreateFiles(ServiceDescription service)
        {
            return CreateFiles(service, new CodeGenerator().Generate);
        }

        public static bool CreateFiles(ServiceDescription service, Func<TypeDescription, string> codeGenerator)
        {
            var settings = CodeGeneratorSettings.Default;
            settings.Namespace = service.Name + ".Proxy";
            settings.OutputPath = "./" + settings.Namespace;

            return CreateFiles(service, codeGenerator, settings);
        }

        public static bool CreateFiles(ServiceDescription serviceDescription,
            Func<TypeDescription, string> codeGenerator,
            CodeGeneratorSettings settings)
        {
            var service = new Service(serviceDescription, settings);
            var serviceCode = service.TransformText();


            if (!Directory.Exists(settings.OutputPath))
                Directory.CreateDirectory(settings.OutputPath);


            foreach (var endpoint in serviceDescription.InputDescriptions)
            {
                var code = codeGenerator(endpoint.TypeDescription);
                if (!string.IsNullOrEmpty(code))
                    File.WriteAllText(Path.Combine(settings.OutputPath, endpoint.TypeDescription.Name + ".cs"), code);
            }

            foreach (var endpoint in serviceDescription.OutputDescriptions)
            {
                var code = codeGenerator(endpoint.TypeDescription);
                if (!string.IsNullOrEmpty(code))
                    File.WriteAllText(Path.Combine(settings.OutputPath, endpoint.TypeDescription.Name + ".cs"), code);
            }

            foreach (var twin in serviceDescription.TwinDescriptions)
            {
                var code = codeGenerator(twin.TypeDescription);
                if (!string.IsNullOrEmpty(code))
                    File.WriteAllText(Path.Combine(settings.OutputPath, twin.TypeDescription.Name + ".cs"), code);
            }

            foreach (var method in serviceDescription.DirectMethodDescriptions)
            {
                if (method.ReturnTypeDescription != null)
                {
                    var code = codeGenerator(method.ReturnTypeDescription);
                    if (!string.IsNullOrEmpty(code))
                        File.WriteAllText(Path.Combine(settings.OutputPath, method.ReturnTypeDescription.Name + ".cs"),
                            code);
                }

                if (method.ArgumentsTypeDescription != null)
                    foreach (var arg in method.ArgumentsTypeDescription)
                    {
                        var code = codeGenerator(arg.TypeDescription);
                        if (!string.IsNullOrEmpty(code))
                            File.WriteAllText(Path.Combine(settings.OutputPath, arg.TypeDescription.Name + ".cs"),
                                code);
                    }
            }

            File.WriteAllText(Path.Combine(settings.OutputPath, $"I{serviceDescription.Name}.cs"), serviceCode);

            return true;
        }
    }
}