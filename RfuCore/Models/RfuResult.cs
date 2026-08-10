using System;
using System.Net;

namespace ResumableFileTransfer.RfuCore.Models
{
    public class RfuResult : IEquatable<RfuResult>
    {
        public HttpStatusCode StatusCode { get; set; }
        public string Body { get; set; }
        public string ErrorType { get; set; }

        public RfuResult(HttpStatusCode httpStatusCode, string body, string errorType = "")
        {
            StatusCode = httpStatusCode;
            Body = body;
            ErrorType= errorType;
        }

        // result of POST
        public static RfuResult CREATED = new RfuResult(HttpStatusCode.OK, "File is created");

        // result of PUT
        public static RfuResult UPLOAD_STARTED = new RfuResult(HttpStatusCode.OK, "Upload started");
        public static RfuResult UPLOAD_SUCCESSFULLY = new RfuResult(HttpStatusCode.OK, "Upload successfully");
        public static RfuResult UPLOAD_COMPLETED = new RfuResult(HttpStatusCode.OK, "Upload completed");
        public static RfuResult FILE_NOT_FOUND = new RfuResult(HttpStatusCode.NotFound, "File not found");
        public static RfuResult CONTINUE_TO_DOWNLOAD = new RfuResult(HttpStatusCode.PermanentRedirect, string.Empty);

        // result of request checking
        public static RfuResult VALID_REQUEST = new RfuResult(HttpStatusCode.OK, string.Empty);
        public static RfuResult FILE_EXT_INVALID = new RfuResult(HttpStatusCode.PreconditionFailed, "File extension is invalid, allowed extensions are {0}", "FILE_EXT_INVALID");
        public static RfuResult FILE_NAME_INVALID = new RfuResult(HttpStatusCode.PreconditionFailed, "File name can not contain any of the following characters: {0}", "FILE_NAME_INVALID");
        public static RfuResult FILE_NAME_TOO_LONG = new RfuResult(HttpStatusCode.PreconditionFailed, "The length of file name can not exceeds {0} characters", "FILE_NAME_TOO_LONG");
        public static RfuResult METADATA_FILENAME_REQUIRED = new RfuResult(HttpStatusCode.PreconditionFailed, "File name is required", "METADATA_FILENAME_REQUIRED");
        public static RfuResult METADATA_FORMAT_INVALID = new RfuResult(HttpStatusCode.PreconditionFailed, "Metadata format is invalid", "METADATA_FORMAT_INVALID");
        public static RfuResult CONTENT_RANGE_REQUIRED = new RfuResult(HttpStatusCode.PreconditionFailed, "Content-Range is required", "CONTENT_RANGE_REQUIRED");
        public static RfuResult CONTENT_RANGE_FORMAT_INVALID = new RfuResult(HttpStatusCode.PreconditionFailed, "Content-Range format is invalid", "CONTENT_RANGE_FORMAT_INVALID");
        public static RfuResult FILE_SIZE_TOO_LARGE = new RfuResult(HttpStatusCode.PreconditionFailed, "File size is too large", "FILE_SIZE_TOO_LARGE");
        public static RfuResult OFFSET_NOTMATCH = new RfuResult(HttpStatusCode.PreconditionFailed, "Upload offset not match", "OFFSET_NOTMATCH");


        #region Equals
        public override int GetHashCode()
        {
            return this.StatusCode.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is RfuResult))
                return false;

            return Equals((RfuResult)obj);
        }

        public bool Equals(RfuResult other)
        {
            return this.StatusCode == other.StatusCode && this.Body == other.Body;
        }

        public static bool operator ==(RfuResult result1, RfuResult result2)
        {
            return result1.Equals(result2);
        }

        public static bool operator !=(RfuResult result1, RfuResult result2)
        {
            return !result1.Equals(result2);
        }
        #endregion
    }
}
