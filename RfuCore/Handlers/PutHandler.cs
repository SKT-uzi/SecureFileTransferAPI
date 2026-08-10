using ResumableFileTransfer.RfuCore.Adapters;
using ResumableFileTransfer.RfuCore.Models;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ResumableFileTransfer.RfuCore.Handlers
{
    public class PutHandler : BaseHandler
    {
        public PutHandler(RfuContextAdapter context) : base(context)
        {

        }

        public override async Task<RfuResult> InvokeAsync()
        {
            #region Parse request
            var parsedResult = ParseRequest();
            if (parsedResult.Status != RfuResult.VALID_REQUEST) return parsedResult.Status;

            var store = Context.RfuConfiguration.DataStore;

            var contentRange = (RfuContentRange)parsedResult.Data;
            #endregion

            // Force caller to initilize object Context.File
            if (Context.File == null || string.IsNullOrEmpty(Context.File.FileID))
            {
                throw new Exception("Object Context.File has not been initialized.");
            }

            #region Build file info
            Context.File.UploadBasePath = Context.RfuConfiguration.UploadBasePath;
            Context.File.TargetBasePath = Context.RfuConfiguration.TargetBasePath;
            Context.File.ContentRange = contentRange;
            #endregion

            #region Check Resume
            RfuFileStatus status;
            if (Context.File.IsEmpty == false && contentRange.Start == 0 && contentRange.End == 0)
            {
                // 1. Upload was completed
                // 2. The file id is invalid or the file was expired
                // 3. Continue to upload
                status = await store.GetStatusAsync(Context.File);

                if (status.Response == RfuResult.CONTINUE_TO_DOWNLOAD)
                {
                    // read metadata
                    Context.File.Metadata = await store.ReadMetadataAsync(Context.File);
                    Context.Response.Headers["Range"] = $"bytes=0-{(status.Offset == 0 ? 0 : status.Offset  - 1)}";
                }

                return status.Response;
            }

            status = await store.GetStatusAsync(Context.File);

            if (status.Response == RfuResult.FILE_NOT_FOUND)
                return status.Response;
            #endregion

            #region Check if offset of file are matching between client and server
            var offset = status.Offset;
            if (contentRange.Start != offset)
            {
                return RfuResult.OFFSET_NOTMATCH;
            }
            #endregion

            #region Upload
            Context.File.UploadContent = await ReadUploadContentAsync();

            offset = await store.WriteAsync(Context.File);

            Context.Response.Headers["Range"] = $"bytes=0-{offset - 1}";
            #endregion

            #region All data uploaded completed, move file to target folder
            if (offset == contentRange.Total)
            {
                // Read metadata for logging in controller
                Context.File.Metadata = await store.ReadMetadataAsync(Context.File);

                //try
                //{
                //    await store.MoveAsync(Context.File);
                //}
                //catch (Exception ex)
                //{
                //    throw new Exception($"Error occured when moving file to target folder.\nFileID: {Context.File.FileID}\nError Message: {ex.Message}");
                //}

                return RfuResult.UPLOAD_COMPLETED;
            }
            #endregion

            #region Get meta data when data firstly come in for caller's usage, for example, logging
            if (Context.File.ContentRange.Start == 0)
            {
                // read metadata
                Context.File.Metadata = await store.ReadMetadataAsync(Context.File);
            }
            #endregion

            return RfuResult.UPLOAD_SUCCESSFULLY;
        }

        internal override RequestParsedResult ParseRequest()
        {
            // Check content-range
            var contentRangeString = Context.Request.Headers["Content-Range"];
            if (contentRangeString.Count == 0)
            {
                return new RequestParsedResult() { Status = RfuResult.CONTENT_RANGE_REQUIRED };
            }

            // Content-Range: */13
            Regex regOffset = new Regex(@"^\*/\d+$");
            if (regOffset.IsMatch(contentRangeString.ToString()))
            {
                var contentRange = GetContentRangeFromRequest();

                return new RequestParsedResult() { Status = RfuResult.VALID_REQUEST, Data = contentRange };
            }

            // Content-Range: bytes 0-12/13
            Regex regUpdate = new Regex(@"^bytes \d+-\d+/\d+$");
            if (regUpdate.IsMatch(contentRangeString.ToString()))
            {
                var contentRange = GetContentRangeFromRequest();

                if (contentRange.Total == 0 && contentRange.Start == 0 && contentRange.End == 0)
                {
                    // empty blob
                    return new RequestParsedResult() { Status = RfuResult.VALID_REQUEST, Data = contentRange };
                }

                if (contentRange.End < contentRange.Start || contentRange.Total <= contentRange.End)
                {
                    return new RequestParsedResult() { Status = RfuResult.CONTENT_RANGE_FORMAT_INVALID };
                }

                if (contentRange.Total > Context.RfuConfiguration.MaxFileSize)
                {
                    return new RequestParsedResult() { Status = RfuResult.FILE_SIZE_TOO_LARGE };
                }

                return new RequestParsedResult() { Status = RfuResult.VALID_REQUEST, Data = contentRange };
            }
            else
            {
                return new RequestParsedResult() { Status = RfuResult.CONTENT_RANGE_FORMAT_INVALID };
            }
        }
    }
}
