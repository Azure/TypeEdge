using System;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoT.EdgeCompose.Modules
{
    public class Output<T>
       where T : IEdgeMessage
    {
        public void Publish(Func<T, Task<MessageResult>> handler)
        {
        }
    }
}