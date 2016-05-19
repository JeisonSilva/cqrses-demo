using NLog;
using NLog.Config;
using NLog.Targets;

namespace Payroll.Infrastructure
{
    public class DefaultLogger : ILogger
    {
        private readonly Logger _logger;

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
            _logger = LogManager.GetLogger("log");
        }

        public void Trace(string message)
        {
            _logger.Trace(message);
        }

        public void Debug(string message)
        {
            _logger.Debug(message);
        }

        public void Info(string message)
        {
            _logger.Info(message);
        }

        public void Warn(string message)
        {
            _logger.Warn(message);
        }

        public void Error(string message)
        {
            _logger.Error(message);
        }

        public void Fatal(string message)
        {
            _logger.Fatal(message);
        }
    }
}
