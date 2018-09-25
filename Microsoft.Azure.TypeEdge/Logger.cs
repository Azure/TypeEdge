using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Core;

namespace Microsoft.Azure.TypeEdge
{
    //this code has been copied from the IoT Edge Runtime

    public static class Logger
    {
        public const string RuntimeLogLevelEnvKey = "RuntimeLogLevel";

        static readonly Dictionary<string, LogEventLevel> LogLevelDictionary = new Dictionary<string, LogEventLevel>(StringComparer.OrdinalIgnoreCase)
        {
            {"verbose", LogEventLevel.Verbose},
            {"debug", LogEventLevel.Debug},
            {"info", LogEventLevel.Information},
            {"information", LogEventLevel.Information},
            {"warning", LogEventLevel.Warning},
            {"error", LogEventLevel.Error},
            {"fatal", LogEventLevel.Fatal}
        };

        static LogEventLevel logLevel = LogEventLevel.Information;

        public static void SetLogLevel(string level)
        {
            logLevel = LogLevelDictionary.TryGetValue(level.ToLower(), out LogEventLevel value) ? value : LogEventLevel.Information;
        }

        public static LogEventLevel GetLogLevel() => logLevel;

        static readonly Lazy<ILoggerFactory> LoggerLazy = new Lazy<ILoggerFactory>(() => GetLoggerFactory(), true);

        public static ILoggerFactory Factory => LoggerLazy.Value;

        static ILoggerFactory GetLoggerFactory()
        {
            var levelSwitch = new LoggingLevelSwitch();
            levelSwitch.MinimumLevel = logLevel;
            Serilog.Core.Logger loggerConfig = new LoggerConfiguration()
                    .MinimumLevel.ControlledBy(levelSwitch)
                    .Enrich.FromLogContext()
                    .WriteTo.Console(
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext:1}] - {Message}{NewLine}{Exception}"
                    )
                    .CreateLogger();
            if (levelSwitch.MinimumLevel <= LogEventLevel.Debug)
            {
                // Overwrite with richer content if less then debug
                loggerConfig = new LoggerConfiguration()
                    .MinimumLevel.ControlledBy(levelSwitch)
                    .Enrich.FromLogContext()
                    .WriteTo.Console(
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext:1}] - {Message}{NewLine}{Exception}"
                    )
                    .CreateLogger();
            }
            ILoggerFactory factory = new LoggerFactory()
                .AddSerilog(loggerConfig);

            return factory;
        }
    }
}
