using Microsoft.Azure.Devices.Shared;
using System;
using System.Reflection;

namespace Microsoft.Azure.IoT.TypeEdge.Modules
{
    public abstract class TypeModuleTwin : IModuleTwin
    {
        public Twin LastKnownTwin { get; set; }
        public void SetTwin(Twin twin)
        {
            LastKnownTwin = twin;
            foreach (var prop in GetType().GetProperties(BindingFlags.Public| BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                if (twin.Properties.Desired.Contains(prop.Name))
                    prop.SetValue(this, Convert.ChangeType(twin.Properties.Desired[prop.Name], prop.PropertyType));
            }
        }

        public Twin GetTwin()
        {
            Twin result = LastKnownTwin;

            if (result == null)
                result = new Twin() { Properties = new TwinProperties() { Reported = new TwinCollection() } };

            foreach (var prop in GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                if (prop.GetValue(this) != null)
                    LastKnownTwin.Properties.Reported[prop.Name] = Convert.ChangeType(prop.GetValue(this), typeof(string));
            }
            return result;
        }
    }
}