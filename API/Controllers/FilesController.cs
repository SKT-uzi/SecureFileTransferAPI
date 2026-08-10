using ResumableFileTransfer.API.Authentication;
using ResumableFileTransfer.API.Interfaces;
using ResumableFileTransfer.API.Models;
using ResumableFileTransfer.Entity;
using ResumableFileTransfer.RfuCore.Adapters;
using ResumableFileTransfer.RfuCore.Handlers;
using ResumableFileTransfer.RfuCore.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ResumableFileTransfer.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class FilesController : Base.ApiControllerBase
    {
        private readonly IRfuConfiguration _rfuConfig;
        private readonly IDBContext _dbContext;
        private readonly IAPIConfiguration _apiConfiguration;
        private readonly ILogger<FilesController> _logger;

        public FilesController(IRfuConfiguration rfuConfig, IDBContext dbContext, IAPIConfiguration apiConfiguration, ILogger<FilesController> logger)
        {
            _rfuConfig = rfuConfig;
            _dbContext = dbContext;
            _apiConfiguration = apiConfiguration;
            _logger = logger;
        }


        /// <summary>
        /// Create a file with metadata.
        /// </summary>
        /// <returns>If file is created successfully, an url will be add to header in response, client can update the file with it.</returns>
        [HttpPost]
        [ApiAuthorize]
        public async Task<IActionResult> Post()
        {
            var softwareToken = await ValidateSoftwareToken(DataTransferAction.Create);
            if (string.IsNullOrEmpty(softwareToken))
            {
                // a 401 code will be returned only if jwt token validation failed
                // in other case, a 403 code will be returned
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return new JsonResult(new BaseResponseResult { Message = "Software token is invalid" });
            }

            var rfuContext = CreateRfuContext();

            var tokenInfo = _dbContext.GetTokenInfo(softwareToken);
            rfuContext.RfuConfiguration.RootFolder = tokenInfo.TargetFileLocation;
            rfuContext.RfuConfiguration.AllowedExtensions = tokenInfo.FileFormat.Split(",");

            var result = await new PostHandler(rfuContext).InvokeAsync();

            if (result == RfuResult.FILE_EXT_INVALID)
            {
                var errorMessage = string.Format(result.Body, string.Join(", ", rfuContext.RfuConfiguration.AllowedExtensions));
                SaveDataTransferLog(DataTransferAction.Create, DataTransferStatus.Failed, errorMessage, rfuContext);

                Response.StatusCode = (int)result.StatusCode;
                return new JsonResult(new PreconditionFailedResponseResult { ErrorType = result.ErrorType, Message = errorMessage });
            }
            else if (result == RfuResult.CREATED)
            {
                if (rfuContext.File == null || string.IsNullOrEmpty(rfuContext.File.Location))
                {
                    throw new Exception("File.Location has not been generated");
                }
                Response.Headers["Location"] = rfuContext.File.Location;
                SaveDataTransferLog(DataTransferAction.Create, DataTransferStatus.Successfully, string.Empty, rfuContext);
            }

            Response.StatusCode = (int)result.StatusCode;

            if (result.StatusCode == HttpStatusCode.PreconditionFailed)
            {
                return new JsonResult(new PreconditionFailedResponseResult { ErrorType = result.ErrorType, Message = result.Body });
            }
            else
            {
                return new JsonResult(new BaseResponseResult() { Message = result.Body });
            }
        }

        /// <summary>
        /// Update a file with content or get offset of file.
        /// </summary>
        /// <param name="id">File ID.</param>
        /// <returns>Offset of file</returns>
        [HttpPut("{id}")]
        [ApiAuthorize]
        public async Task<IActionResult> Put(string id)
        {
            var rfuContext = CreateRfuContext();

            rfuContext.File = new RfuFile() { FileID = id };
            rfuContext.Logger = _logger;

            var result = await new PutHandler(rfuContext).InvokeAsync();

            if (result == RfuResult.UPLOAD_SUCCESSFULLY)
            {
                // logging when the data firstly come in
                if (rfuContext.File.ContentRange.Start == 0)
                {
                    SaveDataTransferLog(DataTransferAction.Start, DataTransferStatus.Successfully, string.Empty, rfuContext);
                }
            }

            // logging when client request to resume
            if (result == RfuResult.CONTINUE_TO_DOWNLOAD)
            {
                SaveDataTransferLog(DataTransferAction.Resume, DataTransferStatus.Successfully, $"Range: {rfuContext.Response.Headers["Range"]}", rfuContext);
            }
            // logging when a file was uploaded completed
            else if (result == RfuResult.UPLOAD_COMPLETED)
            {
                var softwareToken = CommonHelper.GetDecryptedValueFromPayload(HttpContext, Consts.ENCRYPTED_SOFTWARE_TOKEN);
                var message = JsonConvert.SerializeObject(new CompletionQueue { FileID = id, SoftwareToken = softwareToken });
                if (rfuContext.File.ContentRange.Total > _apiConfiguration.LargeFileSize)
                {
                    await CommonHelper.SendMessageToQueueAsync(_apiConfiguration.LargeFileCompletionQueueName, message);
                }
                else
                {
                    await CommonHelper.SendMessageToQueueAsync(_apiConfiguration.FileCompletionQueueName, message);
                }
                SaveDataTransferLog(DataTransferAction.Complete, DataTransferStatus.Successfully, string.Empty, rfuContext);
            }

            Response.StatusCode = (int)result.StatusCode;
            if (result.StatusCode == HttpStatusCode.PreconditionFailed)
            {
                return new JsonResult(new PreconditionFailedResponseResult { ErrorType = result.ErrorType, Message = result.Body });
            }
            else
            {
                return new JsonResult(new BaseResponseResult() { Message = result.Body });
            }
        }

        private async Task<string> ValidateSoftwareToken(DataTransferAction dataTransferAction)
        {
            string softwareToken;
            try
            {
                softwareToken = CommonHelper.GetDecryptedValueFromPayload(HttpContext, Consts.ENCRYPTED_SOFTWARE_TOKEN);
            }
            catch (Exception ex)
            {
                var param = new Dictionary<string, string>();
                param.Add("EncryptedSoftwareToken", CommonHelper.GetValueFromPayload(HttpContext, Consts.ENCRYPTED_SOFTWARE_TOKEN));
                param.Add("ClientIP", GetClientIP());
                param.Add("Exception", ex.Message);

                var errorMessage = $"Payload software token decryption failed, the jwt token may have been tampered with, data: {JsonConvert.SerializeObject(param)}";

                SaveDataTransferLog(dataTransferAction, DataTransferStatus.Failed, errorMessage);

                // the logging will also send email to tech
                _logger.LogError(errorMessage);

                return null;
            }

            var tokenStatus = _dbContext.CheckSoftwareTokenStatus(softwareToken);

            if (tokenStatus != SoftwareTokenValidateStatus.Valid)
            {
                var param = new Dictionary<string, string>();
                param.Add("SoftwareToken", softwareToken);
                param.Add("SoftwareTokenStatus", tokenStatus.ToString());
                param.Add("ClientIP", GetClientIP());

                var errorMessage = $"Software token is invalid, data: {JsonConvert.SerializeObject(param)}";

                SaveDataTransferLog(dataTransferAction, DataTransferStatus.Failed, errorMessage);

                // send email to tech
                //await CommonHelper.SendEmailAsync(_apiConfiguration.EmailFrom, $"{_apiConfiguration.TechnicalEmail}", _apiConfiguration.EmailSubjectSoftwareTokenInvalid, errorMessage);

                return null;
            }

            return await Task.FromResult(softwareToken);
        }

        private RfuContextAdapter CreateRfuContext()
        {
            RfuContextAdapter context = new RfuContextAdapter();
            context.Request = HttpContext.Request;
            context.Response = HttpContext.Response;
            context.RfuConfiguration = _rfuConfig;

            return context;
        }

        private void SaveDataTransferLog(DataTransferAction action, DataTransferStatus status, string note, RfuContextAdapter rfuContext = null)
        {
            var log = new DataTransferLog()
            {
                Action = action,
                Status = status,
                Note = note,
                ClientIP = GetClientIP()
            };

            if (rfuContext != null)
            {
                log.FileID = rfuContext.File.FileID;
                log.Path = rfuContext.File.Metadata != null ? FormatPath($"{FormatPath(rfuContext.File.Metadata.RootFolder)}/{FormatPath(rfuContext.File.Metadata.Path)}") : string.Empty;
                log.FileName = rfuContext.File.Metadata != null ? rfuContext.File.Metadata.FileName : string.Empty;
                log.FileSize = rfuContext.File.ContentRange != null ? rfuContext.File.ContentRange.Total : (long?)null;
            }

            _logger.LogInformation(JsonConvert.SerializeObject(log));

            // add COMPLETE log to database
            if (action == DataTransferAction.Complete)
            {
                _dbContext.SaveDataTransferLog(log);
            }
        }
    }
}
