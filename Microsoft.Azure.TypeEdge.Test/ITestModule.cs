using Microsoft.Azure.TypeEdge.Attributes;
using Microsoft.Azure.TypeEdge.Modules.Endpoints;
using Microsoft.Azure.TypeEdge.Twins;

namespace Microsoft.Azure.TypeEdge.Test
{
    public class TestMessage : Modules.Messages.EdgeMessage
    {
        public double Value { get; set; }
    }
    public class TestTwin : TypeTwin
    {
        public int Offset { get; set; }
    }

    [TypeModule]
    public interface ITestModule
    {
        Output<TestMessage> Output { get; set; }
        ModuleTwin<TestTwin> Twin { get; set; }
        int TestDirectMethod(int value);
    }
}
