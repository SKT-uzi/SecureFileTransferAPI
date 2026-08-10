using ResumableFileTransfer.RfuCore.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ResumableFileTransfer.RfuCore.Adapters
{
    public class RfuContextAdapter
    {
        public RfuFile File { get; set; }
        public HttpRequest Request { get; set; }
        public HttpResponse Response { get; set; }
        public IRfuConfiguration RfuConfiguration { get; set; }
        public ILogger Logger { get; set; }
    }
}
