using ResumableFileTransfer.Common;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using Azure.Storage.Queues;

namespace ResumableFileTransfer.API
{
    public static class CommonHelper
    {
        private static string _encryptionKey;
        private static string _runtimefilesStorageConnectionString;

        public static void Initialize(string encryptionKey, string runtimefilesStorageConnectionString)
        {
            if (string.IsNullOrWhiteSpace(encryptionKey))
            {
                throw new ArgumentException("Encryption key is required.", nameof(encryptionKey));
            }

            if (string.IsNullOrWhiteSpace(runtimefilesStorageConnectionString))
            {
                throw new ArgumentException("Runtime files storage connection string is required.", nameof(runtimefilesStorageConnectionString));
            }

            _encryptionKey = encryptionKey;
            _runtimefilesStorageConnectionString = runtimefilesStorageConnectionString;
        }

        public static string Encrypt(string content)
        {
            return BaseHelper.Encrypt(content, GetEncryptionKey());
        }

        public static string Decrypt(string content)
        {
            return BaseHelper.Decrypt(content, GetEncryptionKey());
        }

        public static string GetDecryptedValueFromPayload(HttpContext context, string key)
        {
            string value = GetValueFromPayload(context, key);
            if (value != null)
            {
                try
                {
                    value = Decrypt(value);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Invalid Payload, Key: {key}, Value: {(value ?? "null")}\nInner Exception: {ex.Message}");
                }
            }

            return value;
        }

        public static string GetValueFromPayload(HttpContext context, string key)
        {
            string value = null;
            if (context.Items.ContainsKey(key))
            {
                value = context.Items[key].ToString();
            }

            return value;
        }

        public static async Task SendMessageToQueueAsync(string queueName, string message)
        {
            QueueClient queueClient = new QueueClient(GetRuntimefilesStorageConnectionString(), queueName, new QueueClientOptions
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

        private static string GetEncryptionKey()
        {
            if (string.IsNullOrWhiteSpace(_encryptionKey))
            {
                throw new InvalidOperationException("CommonHelper has not been initialized with an encryption key.");
            }

            return _encryptionKey;
        }

        private static string GetRuntimefilesStorageConnectionString()
        {
            if (string.IsNullOrWhiteSpace(_runtimefilesStorageConnectionString))
            {
                throw new InvalidOperationException("CommonHelper has not been initialized with a runtime files storage connection string.");
            }

            return _runtimefilesStorageConnectionString;
        }
    }
}
