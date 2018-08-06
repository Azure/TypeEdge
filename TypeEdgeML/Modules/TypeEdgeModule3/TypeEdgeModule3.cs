using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using TypeEdge.Modules;
using TypeEdge.Modules.Endpoints;
using TypeEdge.Modules.Enums;
using TypeEdge.Modules.Messages;
using TypeEdge.Twins;
using Microsoft.Extensions.Configuration;
using Python.Runtime;
using TypeEdgeML.Shared;
using TypeEdgeML.Shared.Messages;
using TypeEdgeML.Shared.Twins;
using TypeEdgeModule3;

namespace Modules
{
    public class TypeEdgeModule3 : EdgeModule, ITypeEdgeModule3, IDisposable
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
                    Console.WriteLine(ex.ToString());
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
                Console.WriteLine("Processing new message in TypeEdgeModule3");

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
                        Console.WriteLine(ex.ToString());
                    }
                });


                await Output.PublishAsync(new TypeEdgeModule3Output
                {
                    Data = msg.Data,
                    Metadata = DateTime.UtcNow.ToShortTimeString()
                });
                Console.WriteLine("TypeEdgeModule3: Generated Message");

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