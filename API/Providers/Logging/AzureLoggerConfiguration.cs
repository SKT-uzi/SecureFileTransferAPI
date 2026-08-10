using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace ResumableFileTransfer.API.Providers
{
    public class AzureLoggerConfiguration
    {
        public string AppName { get; set; }
        public LogLevel LogLevel { get; set; } = LogLevel.Information;
    }
}
