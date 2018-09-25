using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using Microsoft.Azure.TypeEdge.Modules;
using Microsoft.Azure.TypeEdge.Modules.Endpoints;
using Microsoft.Azure.TypeEdge.Modules.Enums;
using Microsoft.Azure.TypeEdge.Modules.Messages;
using Microsoft.Azure.TypeEdge.Twins;
using Microsoft.Extensions.Configuration;
using Python.Runtime;
using TypeEdgeML.Shared;
using TypeEdgeML.Shared.Messages;
using TypeEdgeML.Shared.Twins;
using TypeEdgeModule3;
using Microsoft.Extensions.Logging;

namespace Modules
{
    public class TypeEdgeModule3 : TypeModule, ITypeEdgeModule3, IDisposable
    {
        private Py.GILState _state;
        private dynamic _sys;
        private dynamic _np;

        private string _code;
        private SingleThreadTaskScheduler _pythonTaskScheduler;
        public override InitializationResult Init()
        {
            var cts = new CancellationTokenSource();
            _pythonTaskScheduler = new SingleThreadTaskScheduler(cts.Token);


            _pythonTaskScheduler.Schedule(() =>
            {
                try
                {
                    _state = Py.GIL();
                    _sys = Py.Import("sys");
                    _np = Py.Import("numpy");
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "");
                }
            });
            _pythonTaskScheduler.Start();

            _code = File.ReadAllText(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "pythagorean.py"));

            return string.IsNullOrEmpty(_code) ? InitializationResult.Error : InitializationResult.Ok;
        }

        public TypeEdgeModule3(ITypeEdgeModule2 proxy)
        {
            proxy.Output.Subscribe(this, async msg =>
            {
                Logger.LogInformation($"Processing new message");

                await _pythonTaskScheduler.Schedule(() =>
                {
                    try
                    {
                        //TODO: run your python code here.
                        var sin = _np.sin;
                        var fortyFiveDegrees = Math.PI / 4;
                        var a = sin(fortyFiveDegrees);
                        var b = sin(fortyFiveDegrees);


                        PythonEngine.RunSimpleString(_code);

                        dynamic t = _sys.triangle;
                        var c = (float)t.Hypotenuse(a, b);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, "");
                    }
                });


                await Output.PublishAsync(new TypeEdgeModule3Output
                {
                    Data = msg.Data,
                    Metadata = DateTime.UtcNow.ToShortTimeString()
                });
                Logger.LogInformation($"Generated Message");

                return MessageResult.Ok;
            });
        }

        public Output<TypeEdgeModule3Output> Output { get; set; }
        public ModuleTwin<TypeEdgeModule3Twin> Twin { get; set; }

        public new void Dispose()
        {
            _pythonTaskScheduler?.Schedule(() => { _state?.Dispose(); });
            base.Dispose();
        }
    }
}