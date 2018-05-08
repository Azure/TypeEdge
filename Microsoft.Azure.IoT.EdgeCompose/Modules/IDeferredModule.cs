namespace Microsoft.Azure.IoT.EdgeCompose.Modules
{
    public interface IDeferredModule
    {
        void DependsOn(IEdgeModule module);
    }
}