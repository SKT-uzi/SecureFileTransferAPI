using ResumableFileTransfer.Common;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace ResumableFileTransfer.API.Providers
{
    public class AzureLogger : ILogger
    {
        private readonly string _name;
        private readonly AzureLoggerConfiguration _config;

        public AzureLogger(string name, AzureLoggerConfiguration config)
        {
            _name = name;
            _config = config;
        }

        public IDisposable BeginScope<TState>(TState state) where TState : notnull
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= _config.LogLevel;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var message = formatter(state, exception);
            Debug.WriteLine($"{_name} - {message}");

            if (!IsEnabled(logLevel))
            {
                return;
            }

            if (logLevel == LogLevel.Information)
            {
                ApplicationLogHelper.WriteInformation(_config.AppName, _name, message);
            }
            else if (logLevel >= LogLevel.Error)
            {
                ApplicationLogHelper.WriteError(_config.AppName, _name, message, exception);
            }
        }
    }
}
