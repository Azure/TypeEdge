using System;
using System.Threading.Tasks;
using Microsoft.Azure.TypeEdge.Modules.Messages;

namespace Microsoft.Azure.TypeEdge.Modules.Endpoints
{
    public class Input<T> : Endpoint
        where T : class, IEdgeMessage, new()
    {
        private Volumes.Volume<T> _volume;

        public Input(string name, TypeModule module) :
            base(name, module)
        {
            if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(Reference<>))
            {
                _volume = new Volumes.Volume<T>(Name, Module);
                Module.RegisterVolume(name);
            }
        }

        public override string RouteName => $"BrokeredEndpoint(\"/modules/{Module.Name}/inputs/{Name}\")";

        public virtual void Subscribe(Endpoint output, Func<T, Task<MessageResult>> handler)
        {
            var dereference = new Func<T, Task<MessageResult>>((t) =>
            {
                if (_volume != null)
                {
                    //todo: find a typed way to do this
                    var fileName = typeof(T).GetProperty("FileName").GetValue(t) as string;
                    var referenceCount = (int)typeof(T).GetProperty("ReferenceCount").GetValue(t);
                    var message = _volume.Read(fileName);

                    var res =  handler(message);

                    if (--referenceCount <= 0)
                        _volume.Delete(fileName);

                    return res;
                }
                else
                    return handler(t);
            });

            Module.SubscribeRoute(output.Name,
                output.RouteName,
                Name,
                RouteName,
                dereference);
        }
    }
}