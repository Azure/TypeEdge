using TypeEdge.Modules;
using System;
using System.Collections.Generic;
using System.Text;

namespace TypeEdge
{
    public abstract class TypeProperty
    {
        protected TypeProperty(string name, EdgeModule module)
        {
            Name = name;
            Module = module;
        }

        public string Name { get; set; }
        internal EdgeModule Module { get; set; }
    }
}
