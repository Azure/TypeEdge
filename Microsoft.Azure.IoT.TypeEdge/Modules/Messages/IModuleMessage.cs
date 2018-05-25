using Microsoft.Azure.Devices.Client;

namespace Microsoft.Azure.IoT.TypeEdge.Modules.Messages
{
    public interface IModuleMessage
    {
        IModuleMessage FromMessage(Message message);
        string ToJson();
    }
}