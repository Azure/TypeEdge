using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Azure.IoT.TypeEdge.Proxy
{
    public class TypeEdgeProxy
    {
        string connectionString;
        public TypeEdgeProxy(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public T GetModuleProxy<T>()
        {
            throw new NotImplementedException();
        }
    }
}
