using ResumableFileTransfer.Entity;
using System.Collections.Generic;

namespace ResumableFileTransfer.API.Interfaces
{
    public interface IAPIConfiguration
    {
        public Dictionary<string, string> Items { get; }
        public string AppName { get; }
        public string ResumableFileTransferDBConnectionString { get; }
        public string EncryptionKey { get; }
        public string RuntimefilesStorageConnectionString { get; }
        public StorageType StorageType { get; }
        public string[] InvalidCharactersInName { get; }
        public string JWTSecretKey { get; }
        public int JWTExpireMinutes { get; }
        public long MaxFileSize { get; }
        public long LargeFileSize { get; }
        public int MaxFileNameLength { get; }
        public string FileCompletionQueueName { get; }
        public string LargeFileCompletionQueueName { get; }
        public string AzureBlobUploadBasePath { get; }
        public string AzureFileShareTargetBasePath { get; }
        public string AzureBlobTargetBasePath { get; }
    }
}
