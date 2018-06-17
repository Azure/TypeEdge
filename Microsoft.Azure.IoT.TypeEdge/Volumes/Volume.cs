using Microsoft.Azure.IoT.TypeEdge.Modules;
using Microsoft.Azure.IoT.TypeEdge.Modules.Messages;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Microsoft.Azure.IoT.TypeEdge.Volumes
{
    public class Volume<T> : TypeProperty
       where T : class, IEdgeMessage, new()
    {
        public Volume(string name, EdgeModule module)
             : base(name, module)
        {
            Module.RegisterVolume(Name);
        }

        public bool TryWrite(T data, out string fileName)
        {
            var fn = $@"{DateTime.Now.Ticks}";
            if (Module.SetFileData(Name, fn, data))
            {
                fileName = fn;
                return true;
            }
            fileName = null;
            return false;
        }

        public T Read(string fileName)
        {
            return Module.GetFileData<T>(Name, fileName);
        }

        public bool Delete(string fileName)
        {
            return Module.DeleteFile(Name, fileName);
        }
    }
}
