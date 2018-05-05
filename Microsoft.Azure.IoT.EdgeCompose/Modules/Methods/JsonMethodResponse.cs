using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Azure.IoT.EdgeCompose.Modules.Methods
{
    public class JsonMethodResponse : MethodResponse
    {

        public JsonMethodResponse(MethodArgument arg, string data)
        {
            Payload = data;
            Argument = arg;
        }
    }
}
