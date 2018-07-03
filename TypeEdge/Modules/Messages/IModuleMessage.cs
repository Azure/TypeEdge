using Microsoft.Azure.Devices.Client;

namespace TypeEdge.Modules.Messages
{
    public interface IModuleMessage
    {
        IModuleMessage FromMessage(Message message);
        string ToJson();
    }
}