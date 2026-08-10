using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace ResumableFileTransfer.API.Providers
{
    public class AzureLoggerProvider : ILoggerProvider
    {
        private readonly AzureLoggerConfiguration _config;
        private readonly ConcurrentDictionary<string, AzureLogger> _loggers = new ConcurrentDictionary<string, AzureLogger>();

        public AzureLoggerProvider(AzureLoggerConfiguration config)
        {
            _config = config;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, name => new AzureLogger(name, _config));
        }

        public void Dispose()
        {
            _loggers.Clear();
        }
    }
}
