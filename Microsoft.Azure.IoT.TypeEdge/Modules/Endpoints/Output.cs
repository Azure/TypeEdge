using System;
using System.Threading.Tasks;
using Microsoft.Azure.IoT.TypeEdge.Enums;
using Microsoft.Azure.IoT.TypeEdge.Modules.Messages;

namespace Microsoft.Azure.IoT.TypeEdge.Modules.Endpoints
{
    public class Output<T> : Endpoint
        where T : class, IEdgeMessage, new()
    {
        private Volumes.Volume<T> _volume;

        public Output(string name, EdgeModule module) :
            base(name, module)
        {
            if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(Reference<>))
            {
                _volume = new Volumes.Volume<T>(Name, Module);
                Module.RegisterVolume(name);
            }
        }

        public override string RouteName => $"/messages/modules/{Module.Name}/outputs/{Name}";

        public async Task<PublishResult> PublishAsync(T message)
        {
            if (_volume != null)
            {
                string fileName;
                if (_volume.TryWrite(message, out fileName))
                {
                    typeof(T).GetProperty("FileName").SetValue(message, fileName);
                    typeof(T).GetProperty("Message").SetValue(message, null);
                    var referenceCount = (int)typeof(T).GetProperty("ReferenceCount").GetValue(message);
                    typeof(T).GetProperty("ReferenceCount").SetValue(message, ++referenceCount);
                }
            }
            return await Module.PublishMessageAsync(Name, message);
        }

        public virtual void Subscribe(EdgeModule input, Func<T, Task<MessageResult>> handler)
        {
            var dereference = new Func<T, Task<MessageResult>>((t) =>
            {
                if (_volume != null)
                {
                    //todo: find a typed way to do this
                    var fileName = typeof(T).GetProperty("FileName").GetValue(t) as string;
                    var referenceCount = (int)typeof(T).GetProperty("ReferenceCount").GetValue(t);
                    var message = _volume.Read(fileName);

                    var res = handler(message);

                    if (--referenceCount <= 0)
                        _volume.Delete(fileName);

                    return res;
                }
                else
                    return handler(t);
            });

            var inRouteName = $"BrokeredEndpoint(\"/modules/{input.Name}/inputs/{Name}\")";

            if (_volume != null)
                input.RegisterVolume(Name);

            input.SubscribeRoute(Name,
                RouteName,
                Name,
                inRouteName,
                dereference);
        }

    }
}