using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace ResumableFileTransfer.Common
{
    public class BlobLogHelper : ILogHelper
    {
        #region Constructor
        private static readonly TelemetryClient _telemetry = new TelemetryClient(TelemetryConfiguration.CreateDefault());
        private Dictionary<string, string> DicProperties = null;
        private string DirPath { get; set; }
        private string FmtLogFileName { get; set; }
        private string ConnectionString { get; set; }
        private string ContainerName { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobLogHelper" /> class.
        /// fmtLogPath:
        /// runtime-files/Logs/{appname}/{level}_{day}.txt
        /// runtime-files/Logs/{appname}/{subfolder}/{level}_{day}.txt
        /// </summary>
        /// <param name="blobConnectionString">The BLOB connection string.</param>
        /// <param name="fmtLogPath">The FMT log path.</param>
        /// <param name="appName">Name of the application.</param>
        /// <param name="subFolder">The sub folder.</param>
        public BlobLogHelper(string blobConnectionString, string fmtLogPath, string appName, string subFolder)
        {
            var logFullPath = fmtLogPath.Replace("{appname}", appName).Replace("{subfolder}", subFolder);
            BaseHelper.SplitStorageFullPath(logFullPath, out string containerName, out string logPath);
            BaseHelper.SplitStorageRelativePath(logPath, out string dirPath, out string fileName);

            this.DirPath = dirPath;
            this.FmtLogFileName = fileName;
            this.ConnectionString = blobConnectionString;
            this.ContainerName = containerName;
            this.DicProperties = new Dictionary<string, string>();
            this.DicProperties.Add("appName", appName);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Writes the text log.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <exception cref="NotImplementedException"></exception>
        public void WriteTextLog(string content)
        {
            var blobName = this.GetBlobName(LogLevel.Information);
            try
            {
                this.AppendText(blobName, content);
                this.TraceApplicationinsights(LogLevel.Information, content);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"WriteTextLog Exception:{ex.Message},{ex.InnerException?.Message},{content}");
            }
        }

        /// <summary>
        /// Writes the text log asynchronous.
        /// </summary>
        /// <param name="content">The content.</param>
        public async Task WriteTextLogAsync(string content)
        {
            var blobName = this.GetBlobName(LogLevel.Information);
            try
            {
                await this.AppendTextAsync(blobName, content);
                this.TraceApplicationinsights(LogLevel.Information, content);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"WriteTextLogAsync Exception:{ex.Message},{ex.InnerException?.Message},{content}");
            }
        }

        /// <summary>
        /// Writes the error log.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <exception cref="NotImplementedException"></exception>
        public void WriteErrorLog(string content)
        {
            var blobName = this.GetBlobName(LogLevel.Error);
            try
            {
                this.AppendText(blobName, content);
                this.TraceApplicationinsights(LogLevel.Error, content);
                this.WriteTextLog(content);
            }
            catch (Exception ex)
            {
                this.TraceApplicationinsights(LogLevel.Error, $"WriteErrorLogAsync Exception:{ex.Message},{ex.InnerException?.Message},{content}");
            }
        }

        /// <summary>
        /// Writes the error log asynchronous.
        /// </summary>
        /// <param name="content">The content.</param>
        public async Task WriteErrorLogAsync(string content)
        {
            var blobName = this.GetBlobName(LogLevel.Error);
            try
            {
                await this.AppendTextAsync(blobName, content);
                this.TraceApplicationinsights(LogLevel.Error, content);
                await this.WriteTextLogAsync(content);
            }
            catch (Exception ex)
            {
                this.TraceApplicationinsights(LogLevel.Error, $"WriteErrorLogAsync Exception:{ex.Message},{ex.InnerException?.Message},{content}");
            }
        }

        /// <summary>
        /// Writes the warning log.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <exception cref="NotImplementedException"></exception>
        public void WriteWarningLog(string content)
        {
            var blobName = this.GetBlobName(LogLevel.Warning);
            try
            {
                this.AppendText(blobName, content);
                this.TraceApplicationinsights(LogLevel.Warning, content);
                this.WriteTextLog(content);
            }
            catch (Exception ex)
            {
                this.TraceApplicationinsights(LogLevel.Error, $"WriteWarningLogAsync Exception:{ex.Message},{ex.InnerException?.Message},{content}");
            }
        }

        /// <summary>
        /// Writes the warning log asynchronous.
        /// </summary>
        /// <param name="content">The content.</param>
        public async Task WriteWarningLogAsync(string content)
        {
            var blobName = this.GetBlobName(LogLevel.Warning);
            try
            {
                await this.AppendTextAsync(blobName, content);
                this.TraceApplicationinsights(LogLevel.Warning, content);
                await this.WriteTextLogAsync(content);
            }
            catch (Exception ex)
            {
                this.TraceApplicationinsights(LogLevel.Error, $"WriteWarningLogAsync Exception:{ex.Message},{ex.InnerException?.Message},{content}");
            }
        }

        #endregion

        #region Private Methods
        /// <summary>
        /// Formats the log path.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <returns></returns>
        private string GetBlobName(LogLevel level)
        {
            try
            {
                var strDay = DateTime.Now.ToString("yyyy-MM-dd");
                var fileName = this.FmtLogFileName.Replace("{level}", level.ToString()).Replace("{day}", strDay);
                return BaseHelper.CombineStoragePath(this.DirPath, fileName);
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Gets the content of the append log.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns></returns>
        private string GetAppendLogContent(string content)
        {
            return $"\t\t\t\t\t=>{DateTime.Now.ToString("HH:mm:ss")}:{content}\r\n";
        }

        /// <summary>
        /// Backups the file.
        /// </summary>
        /// <param name="originalBlob">The original BLOB.</param>
        /// <param name="leaseClient">release client</param>
        /// <returns></returns>
        private string BackupFile(BlobBaseClient originalBlob, BlobLeaseClient leaseClient)
        {
            var strBackup = new StringBuilder();
            try
            {
                leaseClient.Acquire(duration: TimeSpan.FromMinutes(1));
                strBackup.Append(this.GetAppendLogContent($"locking file success, backup file start"));

                //set backup filename,2021-12-09_Information.txt to 2021-12-09_Information-1.txt
                var strPrefix = originalBlob.Name.Replace(".txt", "-");
                var subNumber = 1;
                var container = originalBlob.GetParentBlobContainerClient();
                var blobItemList = container.GetBlobs(prefix: strPrefix);
                foreach (var blobItem in blobItemList)
                {
                    var tempNumber = Convert.ToInt32(blobItem.Name.Replace(strPrefix, string.Empty).Replace(".txt", string.Empty));
                    if (tempNumber >= subNumber)
                        subNumber = tempNumber + 1;
                }
                var bakupBlobName = $"{strPrefix}{subNumber.ToString("000")}.txt";
                strBackup.Append(this.GetAppendLogContent($"set backup file={bakupBlobName}"));

                //run bakup log file, backup completed, delete orginal log file
                var bakupBlob = container.GetAppendBlobClient(bakupBlobName);
                var copyOption = bakupBlob.StartCopyFromUri(originalBlob.Uri);
                strBackup.Append(this.GetAppendLogContent($"copy backup file start"));

                var dateCopyTimeout = DateTime.Now.AddMinutes(1);
                var bakupBlobProperties = bakupBlob.GetProperties();
                while (DateTime.Now <= dateCopyTimeout && bakupBlobProperties.Value.CopyStatus == CopyStatus.Pending)
                {
                    Thread.Sleep(1000);
                    bakupBlobProperties = bakupBlob.GetProperties();
                    strBackup.Append(this.GetAppendLogContent($"Pending: LeaseStatus={bakupBlobProperties.Value.LeaseStatus},LeaseState={bakupBlobProperties.Value.LeaseState}"));
                }
                strBackup.Append(this.GetAppendLogContent($"copy completed"));

                //copy completed,delete original file
                if (bakupBlobProperties.Value.CopyStatus == CopyStatus.Success)
                {
                    leaseClient.Release();
                    originalBlob.DeleteIfExists();
                    strBackup.Append(this.GetAppendLogContent($"copy completed,unlocking file and delete original name"));
                }
            }
            catch (RequestFailedException exReqBackup)
            {
                var propBlob = originalBlob.GetProperties().Value;
                strBackup.Append(this.GetAppendLogContent($"backup exception: LeaseStatus={propBlob.LeaseStatus},LeaseState={propBlob.LeaseState},ErrorCode={exReqBackup.ErrorCode}"));
            }
            catch (Exception exBackup)
            {
                var propBlob = originalBlob.GetProperties().Value;
                strBackup.Append(this.GetAppendLogContent($"backup exception:LeaseStatus={propBlob.LeaseStatus},LeaseState={propBlob.LeaseState}, {exBackup.Message},{exBackup.InnerException?.Message}"));
            }
            return strBackup.ToString();
        }

        /// <summary>
        /// Backups the file asynchronous.
        /// </summary>
        /// <param name="originalBlob">The original BLOB.</param>
        /// <param name="leaseClient">release client</param>
        private async Task<string> BackupFileAsync(BlobBaseClient originalBlob, BlobLeaseClient leaseClient)
        {
            var strBackup = new StringBuilder();
            try
            {
                await leaseClient.AcquireAsync(duration: TimeSpan.FromMinutes(1));
                strBackup.Append(this.GetAppendLogContent($"locking file success, backup file start"));

                //set backup filename,2021-12-09_Information.txt to 2021-12-09_Information-1.txt
                var strPrefix = originalBlob.Name.Replace(".txt", "-");
                var subNumber = 1;
                var container = originalBlob.GetParentBlobContainerClient();
                var blobItemList = container.GetBlobs(prefix: strPrefix);
                foreach (var blobItem in blobItemList)
                {
                    var tempNumber = Convert.ToInt32(blobItem.Name.Replace(strPrefix, string.Empty).Replace(".txt", string.Empty));
                    if (tempNumber >= subNumber)
                        subNumber = tempNumber + 1;
                }
                var bakupBlobName = $"{strPrefix}{subNumber.ToString("000")}.txt";
                strBackup.Append(this.GetAppendLogContent($"set backup file={bakupBlobName}"));

                //run bakup log file, backup completed, delete orginal log file
                var bakupBlob = container.GetAppendBlobClient(bakupBlobName);
                var copyOption = await bakupBlob.StartCopyFromUriAsync(originalBlob.Uri);
                strBackup.Append(this.GetAppendLogContent($"copy backup file start"));

                var dateCopyTimeout = DateTime.Now.AddMinutes(1);
                var bakupBlobProperties = await bakupBlob.GetPropertiesAsync();
                while (DateTime.Now <= dateCopyTimeout && bakupBlobProperties.Value.CopyStatus == CopyStatus.Pending)
                {
                    await Task.Delay(1000);
                    bakupBlobProperties = await bakupBlob.GetPropertiesAsync();
                    strBackup.Append(this.GetAppendLogContent($"Pending: LeaseStatus={bakupBlobProperties.Value.LeaseStatus},LeaseState={bakupBlobProperties.Value.LeaseState}"));
                }
                strBackup.Append(this.GetAppendLogContent($"copy completed"));

                //copy completed,delete original file
                if (bakupBlobProperties.Value.CopyStatus == CopyStatus.Success)
                {
                    await leaseClient.ReleaseAsync();
                    await originalBlob.DeleteIfExistsAsync();
                    strBackup.Append(this.GetAppendLogContent($"copy completed,unlocking file and delete original name"));
                }
            }
            catch (RequestFailedException exReqBackup)
            {
                var propBlob = (await originalBlob.GetPropertiesAsync()).Value;
                strBackup.Append(this.GetAppendLogContent($"backup exception: LeaseStatus={propBlob.LeaseStatus},LeaseState={propBlob.LeaseState},ErrorCode={exReqBackup.ErrorCode}"));
            }
            catch (Exception exBackup)
            {
                var propBlob = (await originalBlob.GetPropertiesAsync()).Value;
                strBackup.Append(this.GetAppendLogContent($"backup exception:LeaseStatus={propBlob.LeaseStatus},LeaseState={propBlob.LeaseState}, {exBackup.Message},{exBackup.InnerException?.Message}"));
            }
            return strBackup.ToString();
        }

        /// <summary>
        /// Appends the text asynchronous.
        /// </summary>
        /// <param name="blobName">Name of the BLOB.</param>
        /// <param name="content">The content.</param>
        private void AppendText(string blobName, string content)
        {
            var strContent = new StringBuilder();
            var tryIndex = 1;
            try
            {
                var container = new BlobContainerClient(this.ConnectionString, this.ContainerName);
                var appendBlob = container.GetAppendBlobClient(blobName);
                var contentType = BaseHelper.GetContentTypeByExtension(".txt");
                var httpHeader = new BlobHttpHeaders() { ContentType = contentType };
                strContent.AppendLine(content);

                //append text, timeout 3 minute
                var leaseClient = null as BlobLeaseClient;
                var dateAppendTextTimeout = DateTime.Now.AddMinutes(3);
                while (DateTime.Now <= dateAppendTextTimeout)
                {
                    try
                    {
                        if (appendBlob.Exists())
                        {
                            if (leaseClient == null)
                                leaseClient = appendBlob.GetBlobLeaseClient();

                            //1.check file is locked, waiting for locking expired, timeout 1 minute
                            var dateLockTimeout = DateTime.Now.AddMinutes(1);
                            while (DateTime.Now <= dateLockTimeout)
                            {
                                var propBlob = appendBlob.GetProperties().Value;
                                if (propBlob.LeaseStatus == LeaseStatus.Unlocked)
                                {
                                    //the file is unlocked
                                    break;
                                }
                                strContent.Append(this.GetAppendLogContent($"try {tryIndex} The file is locked, waiting for unlocking,LeaseStatus={propBlob.LeaseStatus},LeaseState={propBlob.LeaseState}"));
                                Thread.Sleep(5000);
                            }
                        }

                        //2.append text log
                        if (appendBlob.Exists() == false)
                        {
                            appendBlob.CreateIfNotExists(new AppendBlobCreateOptions() { HttpHeaders = httpHeader });
                        }
                        using (var stream = new MemoryStream(Encoding.UTF8.GetBytes($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}:{strContent.ToString()}")))
                        {
                            appendBlob.AppendBlock(stream);
                        }
                        break;
                    }
                    catch (RequestFailedException exReqFailed)
                    {
                        if (exReqFailed.ErrorCode == "BlockCountExceedsLimit")
                        {
                            //The uncommitted block count cannot exceed the maximum limit of 50,000 blocks
                            strContent.Append(this.GetAppendLogContent($"try {tryIndex} The uncommitted block count cannot exceed the maximum limit of 50,000 blocks"));
                            var backupMessage = this.BackupFile(appendBlob, leaseClient);
                            strContent.Append(backupMessage);
                        }
                        else
                        {
                            if (appendBlob.Exists())
                            {
                                var propBlob = appendBlob.GetProperties().Value;
                                strContent.Append(this.GetAppendLogContent($"try {tryIndex} append log exception: LeaseStatus={propBlob.LeaseStatus},LeaseState={propBlob.LeaseState},ErrorCode={exReqFailed.ErrorCode}"));
                            }
                            else
                            {
                                strContent.Append(this.GetAppendLogContent($"try {tryIndex} append log exception:{exReqFailed.ErrorCode}"));
                            }
                        }
                        Thread.Sleep(5000);
                    }
                    finally
                    {
                        tryIndex++;
                    }
                }
            }
            catch (Exception ex)
            {
                this.TraceApplicationinsights(LogLevel.Information, $"try {tryIndex} appendTextAsync failed:{ex.Message},{ex.InnerException?.Message}, {strContent.ToString()}");
            }
        }

        /// <summary>
        /// Appends the text asynchronous.
        /// </summary>
        /// <param name="blobName">Name of the BLOB.</param>
        /// <param name="content">The content.</param>
        private async Task AppendTextAsync(string blobName, string content)
        {
            var strContent = new StringBuilder();
            var tryIndex = 1;
            try
            {
                var container = new BlobContainerClient(this.ConnectionString, this.ContainerName);
                var appendBlob = container.GetAppendBlobClient(blobName);
                var contentType = BaseHelper.GetContentTypeByExtension(".txt");
                var httpHeader = new BlobHttpHeaders() { ContentType = contentType };
                strContent.AppendLine(content);

                var leaseClient = null as BlobLeaseClient;
                var dateAppendTextTimeout = DateTime.Now.AddMinutes(3);
                while (DateTime.Now <= dateAppendTextTimeout)
                {
                    try
                    {
                        if (await appendBlob.ExistsAsync())
                        {
                            if (leaseClient == null)
                                leaseClient = appendBlob.GetBlobLeaseClient();

                            //1.check file is locked, waiting for locking expired, timeout 1 minute
                            var dateLockTimeout = DateTime.Now.AddMinutes(1);
                            while (DateTime.Now <= dateLockTimeout)
                            {
                                var propBlob = (await appendBlob.GetPropertiesAsync()).Value;
                                if (propBlob.LeaseStatus == LeaseStatus.Unlocked)
                                {
                                    //the file is unlocked
                                    break;
                                }
                                strContent.Append(this.GetAppendLogContent($"try {tryIndex} The file is locked, waiting for unlocking,LeaseStatus={propBlob.LeaseStatus},LeaseState={propBlob.LeaseState}"));
                                await Task.Delay(5000);
                            }
                        }

                        //2.append text log
                        if (await appendBlob.ExistsAsync() == false)
                        {
                            await appendBlob.CreateIfNotExistsAsync(new AppendBlobCreateOptions() { HttpHeaders = httpHeader });
                        }
                        using (var stream = new MemoryStream(Encoding.UTF8.GetBytes($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}:{strContent.ToString()}")))
                        {
                            await appendBlob.AppendBlockAsync(stream);
                        }
                        break;
                    }
                    catch (RequestFailedException exReqFailed)
                    {
                        if (exReqFailed.ErrorCode == "BlockCountExceedsLimit")
                        {
                            //3.The uncommitted block count cannot exceed the maximum limit of 50,000 blocks
                            strContent.Append(this.GetAppendLogContent($"try {tryIndex} The uncommitted block count cannot exceed the maximum limit of 50,000 blocks"));
                            var backupMessage = await this.BackupFileAsync(appendBlob, leaseClient);
                            strContent.Append(backupMessage);
                        }
                        else
                        {
                            if (await appendBlob.ExistsAsync())
                            {
                                var propBlob = (await appendBlob.GetPropertiesAsync()).Value;
                                strContent.Append(this.GetAppendLogContent($"try {tryIndex} append log exception: LeaseStatus={propBlob.LeaseStatus},LeaseState={propBlob.LeaseState},ErrorCode={exReqFailed.ErrorCode}"));
                            }
                            else
                            {
                                strContent.Append(this.GetAppendLogContent($"try {tryIndex} append log exception:{exReqFailed.ErrorCode}"));
                            }
                        }
                        await Task.Delay(5000);
                    }
                    finally
                    {
                        tryIndex++;
                    }
                }
            }
            catch (Exception ex)
            {
                this.TraceApplicationinsights(LogLevel.Information, $"try {tryIndex} appendTextAsync failed:{ex.Message},{ex.InnerException?.Message}, {strContent.ToString()}");
            }
        }

        /// <summary>
        /// Writes the exception.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <param name="content">The content.</param>
        public void TraceApplicationinsights(LogLevel logLevel, string content)
        {
            try
            {
                switch (logLevel)
                {
                    case LogLevel.Information:
                        _telemetry.TrackTrace(content, SeverityLevel.Information, this.DicProperties);
                        break;
                    case LogLevel.Warning:
                        _telemetry.TrackTrace(content, SeverityLevel.Warning, this.DicProperties);
                        break;
                    case LogLevel.Error:
                        _telemetry.TrackTrace(content, SeverityLevel.Error, this.DicProperties);
                        _telemetry.TrackException(new Exception(content), this.DicProperties);
                        break;
                }
            }
            catch { }
        }
        #endregion
    }
}
