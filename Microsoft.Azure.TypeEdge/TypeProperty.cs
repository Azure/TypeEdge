using Microsoft.Azure.TypeEdge.Modules;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Azure.TypeEdge
{
    public abstract class TypeProperty
    {
        protected TypeProperty(string name, TypeModule module)
        {
            Name = name;
            Module = module;
        }

        public string Name { get; set; }
        internal TypeModule Module { get; set; }
    }
}
