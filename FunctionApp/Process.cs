using Azure.Storage.Queues;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ResumableFileTransfer.Common;
using ResumableFileTransfer.Entity;
using ResumableFileTransfer.FunctionApp.Models;
using ResumableFileTransfer.RfuCore.Models;
using ResumableFileTransfer.RfuCore.Stores;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ResumableFileTransfer.FunctionApp
{
    public static class Process
    {
        private static readonly string _className = typeof(Process).FullName!;
        private static readonly SemaphoreSlim _semaphore = new(initialCount: 1, maxCount: 1);

        [Function(nameof(CompleteFile))]
        public static async Task CompleteFile([QueueTrigger("%FileCompletionQueueName%", Connection = "RuntimefilesStorageConnectionString")] string queueItem)
        {
            await ProcessFileAsync(queueItem);
        }

        [Function(nameof(CompleteLargeFile))]
        public static async Task CompleteLargeFile([QueueTrigger("%LargeFileCompletionQueueName%", Connection = "RuntimefilesStorageConnectionString")] string queueItem)
        {
            await _semaphore.WaitAsync();
            try
            {
                await ProcessFileAsync(queueItem);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        [Function("HandleExpiredFile")]
        public static async Task HandleExpiredFile([TimerTrigger("%HandleExpiredFileTrigger%")] TimerInfo myTimer)
        {
            var rfuConfig = CreateConfiguration();

            var helper = new AzureStorageHelper(Config.RuntimefilesStorageConnectionString);
            var files = await helper.GetBlobsAsync(rfuConfig.UploadBasePath);
            foreach (var file in files)
            {
                var name = file.Name;

                // if metadata file has existed for a long time, and the data file does not exist, delete the metadata file
                if (name.EndsWith(".metadata")
                    && (DateTime.Now - file.CreateDate).Value.TotalDays > Config.ForceExpirationDay
                    && await helper.ExistsBlobAsync(name[..name.LastIndexOf('.')]) == false)
                {
                    await helper.DeleteBlobAsync(name);

                    await ApplicationLogHelper.WriteInformationAsync(Config.AppName, _className, $"HandleExpiredFile: File {name} was expired and has been deleted");
                }
            }
        }

        [Function("HandleMissingFile")]
        public static async Task HandleMissingFile([TimerTrigger("%HandleMissingFileTrigger%")] TimerInfo myTimer)
        {
            var rfuConfig = CreateConfiguration();
            var store = rfuConfig.DataStore;
            
            var helper = new AzureStorageHelper(Config.RuntimefilesStorageConnectionString);
            var files = await helper.GetBlobsAsync(rfuConfig.UploadBasePath);

            foreach (var file in files)
            {
                var name = file.Name;
                if (name.EndsWith(".metadata") == false && (DateTime.Now - file.ModifyDate).Value.TotalMinutes > Config.ForceCompletionMinute)
                {
                    var fileId = file.Name[(file.Name.LastIndexOf('/') + 1)..];

                    RfuFile rfuFile = new()
                    {
                        FileID = fileId,
                        UploadBasePath = rfuConfig.UploadBasePath,
                        TargetBasePath = rfuConfig.TargetBasePath,
                        MalwareBasePath = rfuConfig.MalwareBasePath
                    };

                    rfuFile.Metadata = await store.ReadMetadataAsync(rfuFile);
                    var fileSize = await store.GetOffsetAsync(rfuFile);

                    var tokenDetail = await GetTokenDetailByLocationAsync(rfuFile.Metadata.RootFolder);
                    var message = JsonConvert.SerializeObject(new CompletionQueue { FileID = fileId, SoftwareToken = tokenDetail.Token });
                    if (fileSize > Config.LargeFileSize)
                    {
                        await SendMessageToQueueAsync(Config.LargeFileCompletionQueueName, message);
                    }
                    else
                    {
                        await SendMessageToQueueAsync(Config.FileCompletionQueueName, message);
                    }

                    await ApplicationLogHelper.WriteInformationAsync(Config.AppName, _className, $"HandleMissingFile: File (FileID: {fileId}, FileName: {name}) missed to be moved to target folder, send message to queue to trigger the completion.");
                }
            }
        }

        private static async Task ProcessFileAsync(string queueItem)
        {
            var queue = JsonConvert.DeserializeObject<CompletionQueue>(queueItem)
                ?? throw new InvalidOperationException("Queue payload is required.");
            var fileId = queue.FileID;
            var softwareToken = queue.SoftwareToken;

            await ApplicationLogHelper.WriteInformationAsync(Config.AppName, _className, $"File: {fileId} is triggered to process");

            var rfuConfig = CreateConfiguration();
            var store = rfuConfig.DataStore;
            RfuFile file = new()
            {
                FileID = fileId,
                UploadBasePath = rfuConfig.UploadBasePath,
                TargetBasePath = rfuConfig.TargetBasePath,
                MalwareBasePath = rfuConfig.MalwareBasePath
            };

            // if the file has been processed by other process, return
            if (await store.ExistsAsync(file.UploadFullPath) == false)
            {
                await ApplicationLogHelper.WriteInformationAsync(Config.AppName, _className, $"CompleteFile: File {file.UploadFullPath} does not exist, it may has been processed, you can check log to find the reason.");
                return;
            }

            file.Metadata = await store.ReadMetadataAsync(file);

            bool isValidFile = true;
            if (Config.NeedScanning)
            {
                var result = await ScanFileAsync(file);
                if (result == ScanResult.Bad) isValidFile = false;
                else if (result == ScanResult.NA)
                {
                    ApplicationLogHelper.WriteError(Config.AppName, _className, new Exception($"CompleteFile: File {file.UploadFullPath} validation not applicable, the scanning api may be offline"));
                    return;
                }
            }

            if (isValidFile)
            {
                // move
                try
                {
                    await store.CompleteAsync(file);
                }
                catch (Exception ex)
                {
                    ApplicationLogHelper.WriteError(Config.AppName, _className, new Exception($"CompleteFile: Error occured when moving file to target folder.\nFileID: {file.FileID}, FileName: {file.Metadata.FileName}, \nError Message: {ex.Message}", ex));
                    return;
                }

                await ApplicationLogHelper.WriteInformationAsync(Config.AppName, _className, $"CompleteFile: File {fileId} has been moved to target folder: {file.Metadata.RootFolder}");
            }
            else
            {
                await store.MoveToMalwareAsync(file);

                await ApplicationLogHelper.WriteInformationAsync(Config.AppName, _className, $"CompleteFile: File {fileId} scanned failed and has been moved to malware folder");

                _ = await GetTokenDetailByTokenAsync(softwareToken);

                // disable software token
            }
        }

        private static RfuConfiguration CreateConfiguration()
        {
            if (Config.StorageType == StorageType.AzureFileShare)
            {
                return new RfuConfiguration { UploadBasePath = Config.AzureBlobUploadBasePath, TargetBasePath = Config.AzureFileShareTargetBasePath, MalwareBasePath = Config.AzureBlobMalwareBasePath, DataStore = new AzureFileShareStore(Config.RuntimefilesStorageConnectionString) };
            }
            else if (Config.StorageType == StorageType.AzureBlob)
            {
                return new RfuConfiguration { UploadBasePath = Config.AzureBlobUploadBasePath, TargetBasePath = Config.AzureBlobTargetBasePath, MalwareBasePath = Config.AzureBlobMalwareBasePath, DataStore = new AzureBlobStore(Config.RuntimefilesStorageConnectionString) };
            }

            throw new InvalidOperationException($"Unsupported storage type: {Config.StorageType}");
        }

        private static async Task SendMessageToQueueAsync(string queueName, string message)
        {
            QueueClient queueClient = new(Config.RuntimefilesStorageConnectionString, queueName, new QueueClientOptions
            {
                MessageEncoding = QueueMessageEncoding.Base64
            });
            await queueClient.CreateIfNotExistsAsync();

            if (await queueClient.ExistsAsync())
            {
                // Send a message to the queue
                await queueClient.SendMessageAsync(message);
            }
        }

        private static async Task<AccountSoftwareToken> GetTokenDetailByLocationAsync(string location)
        {
            if (string.IsNullOrEmpty(location))
            {
                return new AccountSoftwareToken();
            }

            await using var dbContext = ResumableFileTransferDbContextFactory.Create(Config.ResumableFileTransferDBConnectionString);
            return await dbContext.AccountSoftwareTokens
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.TargetFileLocation == location) ?? new AccountSoftwareToken();
        }

        private static async Task<AccountSoftwareToken> GetTokenDetailByTokenAsync(string softwareToken)
        {
            if (string.IsNullOrEmpty(softwareToken))
            {
                return new AccountSoftwareToken();
            }

            await using var dbContext = ResumableFileTransferDbContextFactory.Create(Config.ResumableFileTransferDBConnectionString);
            return await dbContext.AccountSoftwareTokens
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Token == softwareToken) ?? new AccountSoftwareToken();
        }

        private static async Task<ScanResult> ScanFileAsync(RfuFile file)
        {
            try
            {
                using HttpClient client = new();
                client.Timeout = TimeSpan.FromMinutes(20);
                var requestUri = Config.ScanningUrl;

                var helper = new AzureStorageHelper(Config.RuntimefilesStorageConnectionString);
                var url = await helper.GetBlobSasUri(file.UploadFullPath);

                var content = JsonConvert.SerializeObject(new ScanningFileRequest { Name = file.Metadata.FileName, FileUrl = url.ToString() });
                using var response = await client.PostAsync(requestUri, new StringContent(content));
                var result = await response.Content.ReadAsStringAsync();
                var ok = bool.Parse(result);
                return ok ? ScanResult.OK : ScanResult.Bad;
            }
            catch (Exception ex)
            {
                ApplicationLogHelper.WriteInformation(Config.AppName, _className, $"Exception in ScanFileAsync: {GetMemory()}");
                ApplicationLogHelper.WriteError(Config.AppName, _className, new Exception($"ScanFileAsync failed.\nFileID: {file.FileID}\nError Message: {ex.Message}", ex));

                return ScanResult.NA;
            }
        }

        private static string GetMemory()
        {
            System.Diagnostics.Process proc = System.Diagnostics.Process.GetCurrentProcess();
            long b = proc.PrivateMemorySize64;
            for (int i = 0; i < 2; i++)
            {
                b /= 1024;
            }
            return b + "MB";
        }
    }
}
