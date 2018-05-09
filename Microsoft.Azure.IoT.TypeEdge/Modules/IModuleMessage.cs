namespace Microsoft.Azure.IoT.TypeEdge.Modules
{
    public interface IModuleMessage
    {
        IModuleMessage FromMessage(Devices.Client.Message message);
        string ToJson();
    }
}