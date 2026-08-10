using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace ResumableFileTransfer.Common
{
    public struct TrackingLogHelper
    {
        private static readonly string TrackingLogLocalPath = Environment.GetEnvironmentVariable("TrackingLogLocalPath")?.Trim();
        private static readonly string TrackingLogBlobPath = Environment.GetEnvironmentVariable("TrackingLogBlobPath")?.Trim();
        private static readonly string TrackingLogBlobConnectionString = Environment.GetEnvironmentVariable("ConfigStorageConnectionString")?.Trim();
       
        /// <summary>
        /// Writes the text log.
        /// </summary>
        /// <param name="content">The content.</param>
        public static void WriteTextLog(string content)
        {
            try
            {
                AppendLog("Information", content);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        /// <summary>
        /// Writes the text log asynchronous.
        /// </summary>
        /// <param name="content">The content.</param>
        public static async Task WriteTextLogAsync(string content)
        {
            try
            {
                await AppendLogAsync("Information", content);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        /// <summary>
        /// Writes the error log.
        /// </summary>
        /// <param name="content">The content.</param>
        public static void WriteErrorLog(string content)
        {
            try
            {
                AppendLog("Error", content);
                AppendLog("Information", content);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        /// <summary>
        /// Writes the error log asynchronous.
        /// </summary>
        /// <param name="content">The content.</param>
        public static async Task WriteErrorLogAsync(string content)
        {
            try
            {
                await AppendLogAsync("Error", content);
                await AppendLogAsync("Information", content);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        /// <summary>
        /// Backups to azure BLOB.
        /// </summary>
        public static void BackupToAzureBlob(string appName)
        {
            try
            {
                if (string.IsNullOrEmpty(TrackingLogLocalPath) || string.IsNullOrEmpty(TrackingLogBlobPath) || string.IsNullOrEmpty(TrackingLogBlobConnectionString))
                    return;

                if (Directory.Exists(TrackingLogHelper.TrackingLogLocalPath) == false)
                    return;

                var dateNow = DateTime.Now;
                var dateNowStart = new DateTime(dateNow.Year, dateNow.Month, dateNow.Day);
                var blobFolderPath = TrackingLogBlobPath.Replace("{appname}", appName);
                var localFileList = Directory.GetFiles(TrackingLogHelper.TrackingLogLocalPath);
                var azureStorageHelper = new AzureStorageHelper(TrackingLogBlobConnectionString);
                Task.Run(async () =>
                {
                    foreach (var localFile in localFileList)
                    {
                        var localFileInfo = new FileInfo(localFile);
                        var blobPath = BaseHelper.CombineStoragePath(blobFolderPath, localFileInfo.Name);
                        using (var stream = new MemoryStream(File.ReadAllBytes(localFile)))
                        {
                            await azureStorageHelper.UploadBlobAsync(blobPath, stream);
                        }
                        if (localFileInfo.CreationTime < dateNowStart)
                        {
                            File.Delete(localFile);
                        }
                    }
                }).Wait();
                
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        public static async Task BackupToAzureBlobAsync(string appName)
        {
            try
            {
                if (string.IsNullOrEmpty(TrackingLogLocalPath) || string.IsNullOrEmpty(TrackingLogBlobPath) || string.IsNullOrEmpty(TrackingLogBlobConnectionString))
                    return;

                if (Directory.Exists(TrackingLogHelper.TrackingLogLocalPath) == false)
                    return;

                var dateNow = DateTime.Now;
                var dateNowStart = new DateTime(dateNow.Year, dateNow.Month, dateNow.Day);
                var blobFolderPath = TrackingLogBlobPath.Replace("{appname}", appName);
                var localFileList = Directory.GetFiles(TrackingLogHelper.TrackingLogLocalPath);
                var azureStorageHelper = new AzureStorageHelper(TrackingLogBlobConnectionString);
                foreach (var localFile in localFileList)
                {
                    var localFileInfo = new FileInfo(localFile);
                    var blobPath = BaseHelper.CombineStoragePath(blobFolderPath, localFileInfo.Name);
                    using (var stream = new MemoryStream(File.ReadAllBytes(localFile)))
                    {
                        await azureStorageHelper.UploadBlobAsync(blobPath, stream);
                    }
                    if (localFileInfo.CreationTime < dateNowStart)
                    {
                        File.Delete(localFile);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private static void AppendLog(string level, string content)
        {
            if (string.IsNullOrEmpty(TrackingLogLocalPath))
                return;

            EnsureLocalDirectory();

            using (StreamWriter sw = new StreamWriter(GetFilePath(level), true))
            {
                sw.Write(GetLogText(content));
            }
        }

        private static async Task AppendLogAsync(string level, string content)
        {
            if (string.IsNullOrEmpty(TrackingLogLocalPath))
                return;

            EnsureLocalDirectory();

            using (StreamWriter sw = new StreamWriter(GetFilePath(level), true))
            {
                await sw.WriteAsync(GetLogText(content));
            }
        }

        private static void EnsureLocalDirectory()
        {
            if (Directory.Exists(TrackingLogLocalPath) == false)
            {
                Directory.CreateDirectory(TrackingLogLocalPath);
            }
        }

        private static string GetFilePath(string level)
        {
            var strDay = DateTime.Now.ToString("yyyy-MM-dd");
            return Path.Combine(TrackingLogLocalPath, $"{level}_{strDay}.txt");
        }

        private static string GetLogText(string content)
        {
            var strTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            return $"{strTime}: {content}\r\n";
        }
    }
}
