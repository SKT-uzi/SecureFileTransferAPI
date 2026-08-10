using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ResumableFileTransfer.Common
{
    public static class ApplicationLogHelper
    {
        public static void WriteInformation(string appName, string source, string message)
        {
            TrackingLogHelper.WriteTextLog(FormatMessage("Information", appName, source, message));
        }

        public static Task WriteInformationAsync(string appName, string source, string message)
        {
            return TrackingLogHelper.WriteTextLogAsync(FormatMessage("Information", appName, source, message));
        }

        public static void WriteError(string appName, string source, Exception exception, IDictionary<string, object> properties = null)
        {
            TrackingLogHelper.WriteErrorLog(FormatError(appName, source, exception, properties));
        }

        public static Task WriteErrorAsync(string appName, string source, Exception exception, IDictionary<string, object> properties = null)
        {
            return TrackingLogHelper.WriteErrorLogAsync(FormatError(appName, source, exception, properties));
        }

        public static void WriteError(string appName, string source, string message, Exception exception = null, IDictionary<string, object> properties = null)
        {
            TrackingLogHelper.WriteErrorLog(FormatError(appName, source, exception, properties, message));
        }

        public static Task WriteErrorAsync(string appName, string source, string message, Exception exception = null, IDictionary<string, object> properties = null)
        {
            return TrackingLogHelper.WriteErrorLogAsync(FormatError(appName, source, exception, properties, message));
        }

        private static string FormatMessage(string level, string appName, string source, string message)
        {
            return $"[{level}] [{appName}] [{source}] {message}";
        }

        private static string FormatError(string appName, string source, Exception exception, IDictionary<string, object> properties = null, string message = null)
        {
            var builder = new StringBuilder();
            builder.AppendLine(FormatMessage("Error", appName, source, message ?? exception?.Message ?? "Unknown error"));

            if (properties != null && properties.Count > 0)
            {
                builder.AppendLine($"Properties: {JsonConvert.SerializeObject(properties)}");
            }

            if (exception != null)
            {
                builder.Append(exception);
            }

            return builder.ToString().TrimEnd();
        }
    }
}
