using Castle.DynamicProxy;
using System.Dynamic;

namespace Microsoft.Azure.IoT.TypeEdge.Modules
{
    internal class ModuleProxy: IInterceptor
    {
        public void Intercept(IInvocation invocation)
        {
        }
    }

}
