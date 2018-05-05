namespace Microsoft.Azure.IoT.EdgeCompose.Modules
{
    public interface IDeferred
    {
        void DependsOn(IModule module);
    }
}