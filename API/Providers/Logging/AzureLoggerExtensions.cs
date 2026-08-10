using Microsoft.Extensions.Logging;
using System;

namespace ResumableFileTransfer.API.Providers
{
    public static class AzureLoggerExtensions
    {
        public static ILoggerFactory AddAzureLogger(this ILoggerFactory loggerFactory, AzureLoggerConfiguration config)
        {
            loggerFactory.AddProvider(new AzureLoggerProvider(config));
            return loggerFactory;
        }
        public static ILoggerFactory AddAzureLogger(this ILoggerFactory loggerFactory)
        {
            var config = new AzureLoggerConfiguration();
            return loggerFactory.AddAzureLogger(config);
        }
        public static ILoggerFactory AddAzureLogger(this ILoggerFactory loggerFactory, Action<AzureLoggerConfiguration> configure)
        {
            var config = new AzureLoggerConfiguration();
            configure(config);
            return loggerFactory.AddAzureLogger(config);
        }
    }
}
