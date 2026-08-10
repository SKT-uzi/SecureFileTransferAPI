using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace ResumableFileTransfer.API.Base
{
    public class ApiControllerBase : ControllerBase
    {
        protected string FormatPath(string path)
        {
            path = path.Trim();
            path = path.Replace("\\", "/");

            while (path.Contains("//"))
                path = path.Replace("//", "/");

            while (path.StartsWith("/"))
            {
                if (path.Length > 1)
                    path = path.Substring(1);
                else
                    path = string.Empty;
            }

            while (path.EndsWith("/"))
            {
                if (path.Length > 1)
                    path = path.Substring(0, path.Length - 1);
            }

            return path;
        }

        protected string GetClientIP()
        {
            IPAddress ip;
            string ipAddress = string.Empty;
            var headers = Request.Headers.ToList();
            if (headers.Exists((kvp) => kvp.Key == "X-Forwarded-For"))
            {
                // when running behind a load balancer you can expect this header
                var header = headers.First((kvp) => kvp.Key == "X-Forwarded-For").Value.ToString();
                // in case the IP contains a port, remove ':' and everything after
                ip = IPAddress.Parse(header.Remove(header.IndexOf(':')));
            }
            else
            {
                // this will always have a value (running locally in development won't have the header)
                ip = Request.HttpContext.Connection.RemoteIpAddress;
            }

            try
            {
                if (ip != null)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetworkV6)
                    {
                        ip = Dns.GetHostEntry(ip).AddressList
                            .First(x => x.AddressFamily == AddressFamily.InterNetwork);
                    }
                    ipAddress = ip.ToString();
                }
                return ipAddress;
            }
            catch (Exception )
            {
                return ipAddress;
            }
        }
    }
}
