using System;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoT.EdgeCompose.Modules
{
    public class Input<T>
        where T : IEdgeMessage
    {
        public void Subscribe(Output<T> output, Func<T, Task<MessageResult>> handler)
        {
        }
        public void Subscribe<O>(Output<O> output, Func<O, Task<T>> convert)
            where O : IEdgeMessage
        {
        }
    }
}