namespace Microsoft.Azure.TypeEdge.Proxy
{
    internal class ModuleProxy<T> : ModuleProxyBase
        where T : class
    {
        public ModuleProxy()
            : base(typeof(T))
        {
        }
    }
}