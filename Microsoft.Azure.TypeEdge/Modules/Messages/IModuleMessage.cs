using Microsoft.Azure.Devices.Client;

namespace Microsoft.Azure.TypeEdge.Modules.Messages
{
    public interface IModuleMessage
    {
        IModuleMessage FromMessage(Message message);
        string ToJson();
    }
}