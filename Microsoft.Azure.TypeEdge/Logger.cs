using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Microsoft.Azure.TypeEdge
{
    //this code has been copied from the IoT Edge Runtime

    public static class Logger
    {
        public const string RuntimeLogLevelEnvKey = "RuntimeLogLevel";

        private static readonly Dictionary<string, LogEventLevel> LogLevelDictionary =
            new Dictionary<string, LogEventLevel>(StringComparer.OrdinalIgnoreCase)
            {
                {"verbose", LogEventLevel.Verbose},
                {"debug", LogEventLevel.Debug},
                {"info", LogEventLevel.Information},
                {"information", LogEventLevel.Information},
                {"warning", LogEventLevel.Warning},
                {"error", LogEventLevel.Error},
                {"fatal", LogEventLevel.Fatal}
            };

        private static LogEventLevel _logLevel = LogEventLevel.Information;

        private static readonly Lazy<ILoggerFactory> LoggerLazy =
            new Lazy<ILoggerFactory>(GetLoggerFactory, true);

        public static ILoggerFactory Factory => LoggerLazy.Value;

        public static void SetLogLevel(string level)
        {
            _logLevel = LogLevelDictionary.TryGetValue(level.ToLower(), out var value)
                ? value
                : LogEventLevel.Information;
        }

        public static LogEventLevel GetLogLevel()
        {
            return _logLevel;
        }

        private static ILoggerFactory GetLoggerFactory()
        {
            var levelSwitch = new LoggingLevelSwitch {MinimumLevel = _logLevel};
            var loggerConfig = new LoggerConfiguration()
                .MinimumLevel.ControlledBy(levelSwitch)
                .Enrich.FromLogContext()
                .WriteTo.Console(
                    outputTemplate:
                    "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext:1}] - {Message}{NewLine}{Exception}"
                )
                .CreateLogger();
            if (levelSwitch.MinimumLevel <= LogEventLevel.Debug)
                loggerConfig = new LoggerConfiguration()
                    .MinimumLevel.ControlledBy(levelSwitch)
                    .Enrich.FromLogContext()
                    .WriteTo.Console(
                        outputTemplate:
                        "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext:1}] - {Message}{NewLine}{Exception}"
                    )
                    .CreateLogger();
            var factory = new LoggerFactory()
                .AddSerilog(loggerConfig);

            return factory;
        }
    }
}