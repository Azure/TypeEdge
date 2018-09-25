


using System.IO;
using Microsoft.Extensions.Configuration;

namespace TypeEdge.DovEnv
{
    public class DotenvConfigurationProvider : FileConfigurationProvider
    {
        public DotenvConfigurationProvider(DotenvConfigurationSource source) : base(source)
        {
        }

        public override void Load(Stream stream)
        {
            Data = Dotenv.Read(stream).GetData();
        }
    }
}