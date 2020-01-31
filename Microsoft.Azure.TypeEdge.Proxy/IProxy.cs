using Castle.DynamicProxy;
using Microsoft.Azure.TypeEdge.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Azure.TypeEdge.Proxy
{
    [TypeModule]
    public interface IProxy : IInterceptor
    {
    }
}
