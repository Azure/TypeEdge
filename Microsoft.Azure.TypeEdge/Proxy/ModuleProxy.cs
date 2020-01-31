namespace Microsoft.Azure.TypeEdge.Proxy
{
    internal class ModuleProxy<T> : ModuleProxyBase, IModuleProxy
        where T : class
    {
        public ModuleProxy()
            : base(typeof(T))
        {
        }
    }
}