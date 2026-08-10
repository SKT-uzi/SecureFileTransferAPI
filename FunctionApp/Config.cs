using ResumableFileTransfer.Entity;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.ComponentModel;

namespace ResumableFileTransfer.FunctionApp
{
    public class Config
    {
        private static string _appName = "ResumableFileTransfer";
        private static IConfiguration _configuration;

        public static void Initialize(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public static string AppName
        {
            get
            {
                return _appName;
            }
        }

        public static string ResumableFileTransferDBConnectionString
        {
            get
            {
                var value = GetRequiredString("ResumableFileTransferDBConnectionString");
                if (ResumableFileTransferDbContextFactory.DetectProvider(value) == ResumableFileTransferDatabaseProvider.Sqlite)
                {
                    return value;
                }

                var appName = Environment.GetEnvironmentVariable("DBAppName") ?? _appName;
                var poolingLifeTime = Environment.GetEnvironmentVariable("DBPoolinglifeTime");

                var build = new SqlConnectionStringBuilder(value)
                {
                    ApplicationName = appName,
                    TrustServerCertificate = true
                };

                if (string.IsNullOrEmpty(poolingLifeTime) == false)
                {
                    build.LoadBalanceTimeout = Convert.ToInt32(poolingLifeTime);
                }

                return build.ConnectionString;
            }
        }

        public static string RuntimefilesStorageConnectionString => GetRequiredString("RuntimefilesStorageConnectionString");

        public static string EncryptionKey
        {
            get
            {
                return GetRequiredString("EncryptionKey");
            }
        }

        public static StorageType StorageType
        {
            get
            {
                var storageType = GetRequiredString("StorageType");
                return (StorageType)Enum.Parse(typeof(StorageType), storageType);
            }
        }

        public static int ForceExpirationDay
        {
            get
            {
                return GetRequiredValue<int>("ForceExpirationDay");
            }
        }

        public static int ForceCompletionMinute
        {
            get
            {
                return GetRequiredValue<int>("ForceCompletionMinute");
            }
        }

        public static string FileCompletionQueueName
        {
            get
            {
                return GetRequiredString("FileCompletionQueueName");
            }
        }

        public static string LargeFileCompletionQueueName
        {
            get
            {
                return GetRequiredString("LargeFileCompletionQueueName");
            }
        }

        public static bool NeedScanning
        {
            get
            {
                return GetRequiredValue<bool>("NeedScanning");
            }
        }

        public static string AzureBlobUploadBasePath
        {
            get
            {
                return GetRequiredString("AzureBlobUploadBasePath");
            }
        }

        public static string AzureFileShareTargetBasePath
        {
            get
            {
                return GetRequiredString("AzureFileShareTargetBasePath");
            }
        }

        public static string AzureBlobTargetBasePath
        {
            get
            {
                return GetRequiredString("AzureBlobTargetBasePath");
            }
        }

        public static string AzureBlobMalwareBasePath
        {
            get
            {
                return GetRequiredString("AzureBlobMalwareBasePath");
            }
        }

        public static string ScanningUrl
        {
            get
            {
                return GetRequiredString("ScanningUrl");
            }
        }

        public static long LargeFileSize
        {
            get
            {
                return GetRequiredValue<long>("LargeFileSize");
            }
        }

        private static string GetRequiredString(string key)
        {
            var value = _configuration?[key]
                ?? Environment.GetEnvironmentVariable(key)
                ?? _configuration?[$"Values:{key}"];

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException($"Configuration value '{key}' is required.");
            }

            return value;
        }

        private static T GetRequiredValue<T>(string key)
        {
            var converter = TypeDescriptor.GetConverter(typeof(T));
            return (T)converter.ConvertFromInvariantString(GetRequiredString(key));
        }
    }
}
