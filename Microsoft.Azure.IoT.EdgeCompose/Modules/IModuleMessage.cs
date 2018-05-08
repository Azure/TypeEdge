namespace Microsoft.Azure.IoT.EdgeCompose.Modules
{
    public interface IModuleMessage
    {
        IModuleMessage FromMessage(Devices.Client.Message message);
        string ToJson();
    }
}