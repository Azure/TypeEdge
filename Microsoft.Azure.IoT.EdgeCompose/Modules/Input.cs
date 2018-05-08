using System;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoT.EdgeCompose.Modules
{
    public class Input<T> : Endpoint
        where T : IEdgeMessage
    {
        public Input(string name, IEdgeModule module) :
          base(name, module)
        {
        }

        public void Subscribe(Output<T> output, Func<T, Task<MessageResult>> handler)
        {
        }
        public void Subscribe<O>(Output<O> output, Func<O, Task<T>> convert)
            where O : IEdgeMessage
        {
        }
    }
}