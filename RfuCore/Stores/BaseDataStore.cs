using ResumableFileTransfer.Common;
using ResumableFileTransfer.RfuCore.Interfaces;
using ResumableFileTransfer.RfuCore.Models;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ResumableFileTransfer.RfuCore.Stores
{
    public abstract class BaseDataStore: IDataStore
    {
        private readonly string _runtimefilesStorageConnectionString;
        public AsyncRetryPolicy retryPolicy;

        protected BaseDataStore()
            : this(Environment.GetEnvironmentVariable("RuntimefilesStorageConnectionString"))
        {
        }

        protected BaseDataStore(string runtimefilesStorageConnectionString)
        {
            _runtimefilesStorageConnectionString = runtimefilesStorageConnectionString;
            int MAX_RETRIES = 3;
            int SLEEP_MILISECONDS = 200;

            retryPolicy = Policy.Handle<Exception>()
            .WaitAndRetryAsync(
               retryCount: MAX_RETRIES,
               sleepDurationProvider: _ => TimeSpan.FromMilliseconds(SLEEP_MILISECONDS));
        }

        public async Task<RfuFile> CreateAsync(RfuFile file)
        {
            // create uid.metadata
            var metadataFilePath = GetMetadataFullPath(file);
            var metadata = JsonConvert.SerializeObject(file.Metadata);
            var helper = CreateStorageHelper();

            byte[] byteArray = Encoding.UTF8.GetBytes(metadata);
            using (MemoryStream stream = new MemoryStream(byteArray))
            {
                await helper.UploadBlobAsync(metadataFilePath, stream);
            }

            return file;
        }

        public async Task<BlobItemInfo> GetAsync(string fileFullPath)
        {
            var helper = CreateStorageHelper();
            return await helper.GetBlobItemAsync(fileFullPath);
        }


        public async Task<bool> ExistsAsync(string fileFullPath)
        {
            var helper = CreateStorageHelper();
            return await helper.ExistsBlobAsync(fileFullPath);
        }

        public async Task<MemoryStream> ReadAsync(string fileFullPath)
        {
            var helper = CreateStorageHelper();
            return await helper.DownloadBlobAsync(fileFullPath);
        }

        public async Task<bool> DeleteAsync(RfuFile file)
        {
            var helper = CreateStorageHelper();

            await retryPolicy.ExecuteAsync(async () =>
            {
                if (await helper.ExistsBlobAsync(file.UploadFullPath))
                {
                    await helper.DeleteBlobAsync(file.UploadFullPath);
                }

                if (await helper.ExistsBlobAsync(GetMetadataFullPath(file)))
                {
                    await helper.DeleteBlobAsync(GetMetadataFullPath(file));
                }
            });

            return true;
        }

        public async Task<bool> MoveToMalwareAsync(RfuFile file)
        {
            var helper = CreateStorageHelper();

            await retryPolicy.ExecuteAsync(async () =>
            {
                if (await helper.ExistsBlobAsync(file.UploadFullPath))
                {
                    await helper.CopyBlobAsync(file.UploadFullPath, file.MalwareFullPath, true);
                }

                if (await helper.ExistsBlobAsync(GetMetadataFullPath(file)))
                {
                    await helper.CopyBlobAsync(GetMetadataFullPath(file), GetMetadataMalwareFullPath(file), true);
                }
            });

            return true;
        }

        public async Task<long> WriteAsync(RfuFile file)
        {
            var helper = CreateStorageHelper();

            if (file.IsEmpty)
            {
                await helper.UploadBlobAsync(file.UploadFullPath, new MemoryStream(new byte[] { }));
                return 0;
            }
            else
            {
                await helper.UploadBlockBlobAsync(file.UploadFullPath, file.UploadContent, file.ContentRange.Total);
                var offset = await GetOffsetAsync(file);

                return offset;
            }
        }

        public async Task<long> GetOffsetAsync(RfuFile file)
        {
            var helper = CreateStorageHelper();
            var offset = await helper.GetBlockBlobOffset(file.UploadFullPath);
            return offset;
        }

        public async Task<RfuFileMetadata> ReadMetadataAsync(RfuFile file)
        {
            var helper = CreateStorageHelper();
            var metaStream = await helper.DownloadBlobAsync(GetMetadataFullPath(file));
            var content = string.Empty;
            using (StreamReader sr = new StreamReader(metaStream))
            {
                content = sr.ReadToEnd();
            }

            return JsonConvert.DeserializeObject<RfuFileMetadata>(content);
        }

        public abstract Task<bool> CompleteAsync(RfuFile file);

        public async Task<RfuFileStatus> GetStatusAsync(RfuFile file)
        {
            var helper = CreateStorageHelper();
            var metadataFilePath = GetMetadataFullPath(file);
            if (await helper.ExistsBlobAsync(metadataFilePath) == false)
            {
                return new RfuFileStatus() { Response = RfuResult.FILE_NOT_FOUND };
            }
            else
            {
                var offset = await GetOffsetAsync(file);

                if (file.ContentRange.Total != 0 && file.ContentRange.Total == offset)
                {
                    return new RfuFileStatus() { Response = RfuResult.UPLOAD_COMPLETED };
                }
                else
                {
                    return new RfuFileStatus() { Response = RfuResult.CONTINUE_TO_DOWNLOAD, Offset = offset };
                }
            }
        }

        public string GetMetadataFullPath(RfuFile file) => $"{file.UploadFullPath}.metadata";
        public string GetMetadataMalwareFullPath(RfuFile file) => $"{file.MalwareFullPath}.metadata";

        protected AzureStorageHelper CreateStorageHelper()
        {
            if (string.IsNullOrWhiteSpace(_runtimefilesStorageConnectionString))
            {
                throw new InvalidOperationException("RuntimefilesStorageConnectionString environment variable is required.");
            }

            return new AzureStorageHelper(_runtimefilesStorageConnectionString);
        }
    }
}
