using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.TypeEdge.Enums;
using Microsoft.Azure.TypeEdge.Modules;
using Microsoft.Azure.TypeEdge.Modules.Endpoints;
using Microsoft.Azure.TypeEdge.Modules.Enums;
using Microsoft.Azure.TypeEdge.Twins;
using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.TypeEdge.Test
{
    public class TestModule : TypeModule, ITestModule
    {
        //default values
        private static readonly TestTwin _defaultTwin = new TestTwin
        {
            Offset = 60
        };

        public TestModule(TestTwin defaultTwin = null)
        {
            Twin.SetDefault(defaultTwin ?? _defaultTwin);

            Twin.Subscribe(async twin =>
            {
                Logger.LogInformation("Twin update");

                await Twin.ReportAsync(twin);
                return TwinResult.Ok;
            });
        }

        public Output<TestMessage> Output { get; set; }
        public ModuleTwin<TestTwin> Twin { get; set; }

        public int TestDirectMethod(int value)
        {
            Logger.LogInformation($"GenerateAnomaly called with value:{value}");
            return value;
        }

        public override async Task<ExecutionResult> RunAsync(CancellationToken cancellationToken)
        {
            var twin = await Twin.GetAsync();
            var message = new TestMessage()
            {
                Value = 1.0
            };

            await Output.PublishAsync(message);
            return ExecutionResult.Ok;
        }

    }
}
