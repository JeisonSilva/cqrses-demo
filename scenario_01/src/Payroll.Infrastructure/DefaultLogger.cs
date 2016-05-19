using NLog;
using NLog.Config;
using NLog.Targets;

namespace Payroll.Infrastructure
{
    public class DefaultLogger : ILogger
    {
        
        public DefaultLogger()
        {
            var config = new LoggingConfiguration();

            var target = new ColoredConsoleTarget()
            {
                Name = "Console",
                Layout = @"${date:format=HH\:mm\:ss} ${logger} ${message}"
            };

            config.AddTarget(target);

            var rule = new LoggingRule("*", LogLevel.Trace, target);
            config.LoggingRules.Add(rule);

            LogManager.Configuration = config;
        }

        public void Trace(string logger, string message)
        {
            LogManager.GetLogger(logger).Trace(message);
        }

        public void Debug(string logger, string message)
        {
            LogManager.GetLogger(logger).Debug(message);
        }

        public void Info(string logger, string message)
        {
            LogManager.GetLogger(logger).Info(message);
        }

        public void Warn(string logger, string message)
        {
            LogManager.GetLogger(logger).Warn(message);
        }

        public void Error(string logger, string message)
        {
            LogManager.GetLogger(logger).Error(message);
        }

        public void Fatal(string logger, string message)
        {
            LogManager.GetLogger(logger).Fatal(message);
        }
    }
}
