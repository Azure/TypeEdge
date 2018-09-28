using Microsoft.Extensions.Configuration;

namespace Microsoft.Azure.TypeEdge.DovEnv
{
    public class DotΕnvConfigurationSource : FileConfigurationSource
    {
        public override IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            FileProvider = FileProvider ?? builder.GetFileProvider();
            return new DotΕnvConfigurationProvider(this);
        }
    }
}