using ResumableFileTransfer.RfuCore.Adapters;
using ResumableFileTransfer.RfuCore.Models;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ResumableFileTransfer.RfuCore.Handlers
{
    public abstract class BaseHandler
    {
        internal RfuContextAdapter Context;

        internal BaseHandler(RfuContextAdapter context)
        {
            Context = context;
        }

        public abstract Task<RfuResult> InvokeAsync();
        internal abstract RequestParsedResult ParseRequest();

        internal RfuContentRange GetContentRangeFromRequest()
        {
            var rangeValue = Context.Request.Headers["Content-Range"];
            if (rangeValue.Count > 0)
            {
                // bytes 0-4/13
                if (rangeValue.ToString().Contains("bytes"))
                {
                    var range = rangeValue.ToString().Trim().Replace("bytes ", "");
                    var start = range.Substring(0, range.IndexOf("-"));
                    var left = range.Substring(range.IndexOf("-") + 1);
                    var end = left.Substring(0, left.IndexOf("/"));
                    var total = left.Substring(left.IndexOf("/") + 1);

                    return new RfuContentRange()
                    {
                        Start = long.Parse(start),
                        End = long.Parse(end),
                        Total = long.Parse(total)
                    };
                }
                // */13
                else
                {
                    var range = rangeValue.ToString().Trim();
                    var total = range.Substring(range.IndexOf("/") + 1);

                    return new RfuContentRange()
                    {
                        Total = long.Parse(total)
                    };
                }
            }

            return null;
        }



        internal string GetAbsoluteUri()
        {
            var request = Context.Request;

            string scheme = request.Scheme;
            string host = request.Host.ToString();

            string orginalHost = request.Headers["X-Original-Host"];

            string forwardedProto = request.Headers["X-Forwarded-Proto"];
            if (!string.IsNullOrEmpty(orginalHost))
            {
                scheme = forwardedProto ?? request.Scheme;
                host = orginalHost;
            }

            return new StringBuilder()
                .Append(scheme)
                .Append("://")
                .Append(host)
                .Append(request.PathBase)
                .Append(request.Path)
                .Append(request.QueryString)
                .ToString();
        }

        internal async Task<byte[]> ReadUploadContentAsync()
        {
            if (Context.Request.HasFormContentType)
            {
                var form = await Context.Request.ReadFormAsync();
                if (form.Files.Count > 0)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        await form.Files[0].CopyToAsync(ms);
                        return ms.ToArray();
                    }
                }

                return null;
            }
            else
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    await Context.Request.Body.CopyToAsync(ms);
                    return ms.ToArray();
                }
            }
        }
    }
}
