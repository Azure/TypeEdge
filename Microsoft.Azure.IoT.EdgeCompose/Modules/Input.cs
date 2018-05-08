using System;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoT.EdgeCompose.Modules
{
    public class Input<T> : Endpoint
        where T : IEdgeMessage
    {
        public Input(string name, EdgeModule module) :
          base(name, module)
        {
        }

        public void Subscribe(Output<T> output, Func<T, Task<MessageResult>> handler)
        {
            Module.Subscribe(output.Name, handler);
        }
        public void Subscribe<O>(Output<O> output, Func<O, Task<T>> convert)
            where O : IEdgeMessage
        {
        }
    }
}