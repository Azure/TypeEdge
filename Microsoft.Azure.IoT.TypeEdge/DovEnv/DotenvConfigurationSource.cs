using Microsoft.Extensions.Configuration;

namespace Microsoft.Azure.IoT.TypeEdge.DovEnv
{
    public class DotenvConfigurationSource : FileConfigurationSource
    {
        public override IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            FileProvider = FileProvider ?? builder.GetFileProvider();
            return new DotenvConfigurationProvider(this);
        }
    }
}
