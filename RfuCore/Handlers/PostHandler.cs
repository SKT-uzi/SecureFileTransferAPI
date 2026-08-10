using ResumableFileTransfer.RfuCore.Adapters;
using ResumableFileTransfer.RfuCore.Models;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResumableFileTransfer.RfuCore.Handlers
{
    public class PostHandler : BaseHandler
    {
        public PostHandler(RfuContextAdapter context) : base(context)
        {

        }

        public override async Task<RfuResult> InvokeAsync()
        {
            #region Parse request
            var parsedResult = ParseRequest();
            if (parsedResult.Status != RfuResult.VALID_REQUEST) return parsedResult.Status;
            #endregion

            #region Build file info
            var metadata = (RfuFileMetadata)parsedResult.Data;

            metadata.RootFolder = Context.RfuConfiguration.RootFolder;

            RfuFile file = new RfuFile()
            {
                //FileID = fileId,
                Metadata = metadata,
                UploadBasePath = Context.RfuConfiguration.UploadBasePath,
                TargetBasePath = Context.RfuConfiguration.TargetBasePath
            };
            #endregion

            #region Check file extension
            var fileName = metadata.FileName;
            var ext = fileName.Substring(fileName.LastIndexOf(".") + 1);

            if (Context.RfuConfiguration.AllowedExtensions.Any(x => x.ToLower() == ext.ToLower()) == false)
            {
                Context.File = file;
                return RfuResult.FILE_EXT_INVALID;
            }
            #endregion

            #region Generate file id, build response header "Location" with file url
            // attach file id
            var fileId = Uid.Generate();
            file.FileID = fileId;
            file.Location = $"{GetAbsoluteUri()}/{fileId}";

            await Context.RfuConfiguration.DataStore.CreateAsync(file);
            Context.File = file;
            #endregion

            return RfuResult.CREATED;
        }

        internal override RequestParsedResult ParseRequest()
        {
            // metadata deserialize failed
            RfuFileMetadata metadata;

            var bytes = Task.Run(async () => { return await ReadUploadContentAsync(); }).Result;
            var body = Encoding.UTF8.GetString(bytes);
            try
            {
                metadata = JsonConvert.DeserializeObject<RfuFileMetadata>(body);
            }
            catch (Exception)
            {
                return new RequestParsedResult() { Status = RfuResult.METADATA_FORMAT_INVALID };
            }

            // file name is empty
            if (metadata == null || string.IsNullOrEmpty(metadata.FileName))
            {
                return new RequestParsedResult() { Status = RfuResult.METADATA_FILENAME_REQUIRED };
            }

            // path is invalid
            if (metadata.Path.Contains("../"))
            {
                return new RequestParsedResult() { Status = RfuResult.METADATA_FORMAT_INVALID };
            }

            // check file name
            if (metadata.FileName.Contains("/") || Context.RfuConfiguration.InvalidCharactersInName.Where(x => metadata.FileName.IndexOf(x) > -1).Count() > 0)
            {
                var status = RfuResult.FILE_NAME_INVALID;
                status.Body = string.Format(status.Body, string.Join("", Context.RfuConfiguration.InvalidCharactersInName));
                return new RequestParsedResult() { Status = status };
            }

            if (metadata.FileName.Length > Context.RfuConfiguration.MaxFileNameLength)
            {
                var status = RfuResult.FILE_NAME_TOO_LONG;
                status.Body = string.Format(status.Body, Context.RfuConfiguration.MaxFileNameLength);
                return new RequestParsedResult() { Status = status };
            }

            return new RequestParsedResult() { Status = RfuResult.VALID_REQUEST, Data = metadata };
        }
    }
}
