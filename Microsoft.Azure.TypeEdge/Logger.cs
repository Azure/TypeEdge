// Copyright (c) Microsoft. All rights reserved.
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;

namespace Microsoft.Azure.TypeEdge
{
    //this code has been copied from the IoT Edge Runtime
    public static class Logger
    {
        public const string RuntimeLogLevelEnvKey = "RuntimeLogLevel";


        static readonly Lazy<ILoggerFactory> LoggerLazy = new Lazy<ILoggerFactory>(GetLoggerFactory, true);
        static LogEventLevel logLevel = LogEventLevel.Information;

        public static ILoggerFactory Factory => LoggerLazy.Value;

        public static LogEventLevel GetLogLevel() => logLevel;

        static ILoggerFactory GetLoggerFactory()
        {
            string outputTemplate = logLevel > LogEventLevel.Debug
                ? "<{Severity}> {Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] - {Message}{NewLine}{Exception}"
                : "<{Severity}> {Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext:1}] - {Message}{NewLine}{Exception}";

            LoggerConfiguration ConsoleSinkMap(LoggerSinkConfiguration loggerSinkConfiguration)
                => loggerSinkConfiguration.Console(outputTemplate: outputTemplate);

            return GetLoggerFactory(logLevel, ConsoleSinkMap);
        }

        internal static ILoggerFactory GetLoggerFactory(LogEventLevel logEventLevel, Func<LoggerSinkConfiguration, LoggerConfiguration> loggerSink)
        {
            var levelSwitch = new LoggingLevelSwitch
            {
                MinimumLevel = logEventLevel
            };

            LoggerSinkConfiguration loggerSinkConfiguration = new LoggerConfiguration()
                .MinimumLevel.ControlledBy(levelSwitch)
                .Enrich.FromLogContext()
                .Enrich.With(SeverityEnricher.Instance)
                .WriteTo;

            Serilog.Core.Logger loggerConfig = loggerSink(loggerSinkConfiguration).CreateLogger();

            ILoggerFactory factory = new LoggerFactory()
                .AddSerilog(loggerConfig);

            return factory;
        }

        // This maps the Edge log level to the severity level based on Syslog severity levels.
        // https://en.wikipedia.org/wiki/Syslog#Severity_level
        // This allows tools to parse the severity level from the log text and use it to enhance the log
        // For example errors can show up as red
        class SeverityEnricher : ILogEventEnricher
        {
            static readonly IDictionary<LogEventLevel, int> LogLevelSeverityMap = new Dictionary<LogEventLevel, int>
            {
                [LogEventLevel.Fatal] = 0,
                [LogEventLevel.Error] = 3,
                [LogEventLevel.Warning] = 4,
                [LogEventLevel.Information] = 6,
                [LogEventLevel.Debug] = 7,
                [LogEventLevel.Verbose] = 7
            };

            SeverityEnricher()
            {
            }

            public static SeverityEnricher Instance => new SeverityEnricher();

            public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory) =>
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                    "Severity", LogLevelSeverityMap[logEvent.Level]));
        }
    }
}