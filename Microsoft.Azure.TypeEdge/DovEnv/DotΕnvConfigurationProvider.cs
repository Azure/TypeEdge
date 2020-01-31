using Microsoft.Extensions.Configuration;
using System.IO;

namespace Microsoft.Azure.TypeEdge.DovEnv
{
    public class DotΕnvConfigurationProvider : FileConfigurationProvider
    {
        public DotΕnvConfigurationProvider(DotΕnvConfigurationSource source) : base(source)
        {
        }

        public override void Load(Stream stream)
        {
            Data = DotΕnv.Read(stream).GetData();
        }
    }
}