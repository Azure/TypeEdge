using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Azure.IoT.TypeEdge.Attributes
{
    public class TypeModuleAttribute : Attribute
    {
        public string Name { get; set; }
    }
}
