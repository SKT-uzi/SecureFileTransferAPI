using ResumableFileTransfer.API.Interfaces;
using ResumableFileTransfer.API.Models;
using ResumableFileTransfer.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using Microsoft.Extensions.Configuration;

namespace ResumableFileTransfer.API.Providers
{
    public class APIConfiguration : IAPIConfiguration
    {
        private static readonly string[] _configurationKeys =
        {
            "CIMSConnectionString",
            "DBConnectionString",
            "EncryptionKey",
            "RuntimefilesStorageConnectionString",
            "StorageType",
            "AzureBlobUploadBasePath",
            "AzureFileShareTargetBasePath",
            "AzureBlobTargetBasePath",
            "JWTSecretKey",
            "JWTExpireMinutes",
            "MaxFileSize",
            "ForceExpirationDay",
            "ForceCompletionMinute",
            "NeedScanning",
            "MovingFileQueueName",
            "EmailSubjectSoftwareTokenInvalid",
            "InvalidCharactersInName",
            "MaxFileNameLength",
            "LargeFileSize",
            "FileCompletionQueueName",
            "LargeFileCompletionQueueName"
        };

        private string _appName = "ResumableFileTransfer";

        private readonly IConfiguration _configuration;
        private string _dbConnectionString;
        private string _runtimefilesStorageConnectionString;
        private Dictionary<string, string> _items;
        private string _encryptionKey;
        private StorageType _storageType;
        private string[] _invalidCharactersInName;
        private string _jwtSecretKey;
        private int _jwtExpireMinutes;
        private long _maxFileSize;
        private long _largeFileSize;
        private int _maxFileNameLength;
        private string _fileCompletionQueueName;
        private string _largeFileCompletionQueueName;
        private string _azureBlobUploadBasePath;
        private string _azureFileShareTargetBasePath;
        private string _azureBlobTargetBasePath;

        public APIConfiguration(IConfiguration configuration)
        {
            _configuration = configuration;
            _items = _configurationKeys
                .Select(key => new KeyValuePair<string, string>(key, _configuration[key]))
                .Where(item => string.IsNullOrWhiteSpace(item.Value) == false)
                .ToDictionary(item => item.Key, item => item.Value, StringComparer.OrdinalIgnoreCase);

            _encryptionKey = GetRequiredString("EncryptionKey");

            var storageType = GetRequiredString("StorageType");
            _storageType = (StorageType)Enum.Parse(typeof(StorageType), storageType);

            var s = GetRequiredString("InvalidCharactersInName");
            s = HttpUtility.HtmlDecode(s);
            var list = new List<string>();
            foreach (var c in s)
            {
                list.Add(c.ToString());
            }
            
            _invalidCharactersInName = list.ToArray();

            _dbConnectionString = GetRequiredString("ResumableFileTransferDBConnectionString");
            _runtimefilesStorageConnectionString = GetRequiredString("RuntimefilesStorageConnectionString");
            _maxFileNameLength = GetRequiredValue<int>("MaxFileNameLength");
            _jwtSecretKey = GetRequiredString("JWTSecretKey");
            _jwtExpireMinutes = GetRequiredValue<int>("JWTExpireMinutes");
            _maxFileSize = GetRequiredValue<long>("MaxFileSize");
            _largeFileSize = GetRequiredValue<long>("LargeFileSize");
            _fileCompletionQueueName = GetRequiredString("FileCompletionQueueName");
            _largeFileCompletionQueueName = GetRequiredString("LargeFileCompletionQueueName");
            _azureBlobUploadBasePath = GetRequiredString("AzureBlobUploadBasePath");
            _azureFileShareTargetBasePath = GetRequiredString("AzureFileShareTargetBasePath");
            _azureBlobTargetBasePath = GetRequiredString("AzureBlobTargetBasePath");
        }

        public string ResumableFileTransferDBConnectionString => _dbConnectionString;
        public string RuntimefilesStorageConnectionString => _runtimefilesStorageConnectionString;
        public Dictionary<string, string> Items => _items;
        public string AppName => _appName;
        public string EncryptionKey => _encryptionKey;
        public StorageType StorageType => _storageType;
        public string[] InvalidCharactersInName => _invalidCharactersInName;
        public int MaxFileNameLength => _maxFileNameLength;
        public string JWTSecretKey => _jwtSecretKey;
        public int JWTExpireMinutes => _jwtExpireMinutes;
        public long MaxFileSize => _maxFileSize;
        public long LargeFileSize => _largeFileSize;
        public string FileCompletionQueueName => _fileCompletionQueueName;
        public string LargeFileCompletionQueueName => _largeFileCompletionQueueName;
        public string AzureBlobUploadBasePath => _azureBlobUploadBasePath;
        public string AzureFileShareTargetBasePath => _azureFileShareTargetBasePath;
        public string AzureBlobTargetBasePath => _azureBlobTargetBasePath;

        private string GetRequiredString(string key)
        {
            var value = _configuration[key];
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException($"Configuration value '{key}' is required.");
            }

            return value;
        }

        private T GetRequiredValue<T>(string key)
        {
            var converter = TypeDescriptor.GetConverter(typeof(T));
            return (T)converter.ConvertFromInvariantString(GetRequiredString(key));
        }
    }
}
