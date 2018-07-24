using TypeEdge.Modules;
using TypeEdge.Modules.Messages;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace TypeEdge.Volumes
{
    public class Volume<T> : TypeProperty
       where T : class, IEdgeMessage, new()
    {
        public Volume(string name, EdgeModule module)
             : base(name, module)
        {
            Module.RegisterVolume(Name);
        }

        public bool TryWrite(T data, string fileName)
        {            
            if (Module.SetReferenceData(Name, fileName, data))            
                return true;                        
            return false;
        }

        public T Read(string fileName)
        {
            return Module.GetReferenceData<T>(Name, fileName);
        }

        public bool Delete(string fileName)
        {
            return Module.DeleteReference(Name, fileName);
        }
    }
}
