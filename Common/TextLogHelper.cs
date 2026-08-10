using System;
using System.IO;
using System.Threading.Tasks;

namespace ResumableFileTransfer.Common
{
    public class TextLogHelper : ILogHelper
    {
        private string DirPath { get; set; }
        private string FmtLogFileName { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobLogHelper" /> class.
        /// fmtLogPath:
        /// D:\RuntimeFiles\AzureLogs\{appname}\{level}_{day}.txt
        /// D:\RuntimeFiles\AzureLogs\{appname}\{subfolder}\{level}_{day}.txt
        /// </summary>
        /// <param name="fmtLogPath">The FMT log path.</param>
        /// <param name="appName">Name of the application.</param>
        /// <param name="subFolder">The sub folder.</param>
        public TextLogHelper(string fmtLogPath, string appName, string subFolder)
        {
            var logPath = fmtLogPath.Replace("{appname}", appName).Replace("{subfolder}", subFolder);
            BaseHelper.SplitFileFullPath(logPath, out string dirPath, out string fileName);
            if (Directory.Exists(dirPath) == false)
                Directory.CreateDirectory(dirPath);

            this.FmtLogFileName = fileName;
            this.DirPath = dirPath;
        }

        /// <summary>
        /// Writes the text log.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <exception cref="NotImplementedException"></exception>
        public void WriteTextLog(string content)
        {
            try
            {
                var logText = this.GetFormatLogContent(content);
                var filePath = this.GetFilePath(LogLevel.Information);
                this.AppendText(filePath, logText);
            }
            catch { }
        }

        /// <summary>
        /// Writes the text log asynchronous.
        /// </summary>
        /// <param name="content">The content.</param>
        public async Task WriteTextLogAsync(string content)
        {
            try
            {
                var logText = this.GetFormatLogContent(content);
                var filePath = this.GetFilePath(LogLevel.Information);
                await this.AppendTextAsync(filePath, logText);
            }
            catch { }
        }

        /// <summary>
        /// Writes the error log.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <exception cref="NotImplementedException"></exception>
        public void WriteErrorLog(string content)
        {
            try
            {
                var logText = this.GetFormatLogContent(content);
                var filePath = this.GetFilePath(LogLevel.Error);
                this.AppendText(filePath, logText);

                filePath = this.GetFilePath(LogLevel.Information);
                this.AppendText(filePath, logText);
            }
            catch { }
        }

        /// <summary>
        /// Writes the error log asynchronous.
        /// </summary>
        /// <param name="content">The content.</param>
        public async Task WriteErrorLogAsync(string content)
        {
            try
            {
                var logText = this.GetFormatLogContent(content);
                var filePath = this.GetFilePath(LogLevel.Error);
                await this.AppendTextAsync(filePath, logText);

                filePath = this.GetFilePath(LogLevel.Information);
                await this.AppendTextAsync(filePath, logText);
            }
            catch { }
        }

        /// <summary>
        /// Writes the warning log.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <exception cref="NotImplementedException"></exception>
        public void WriteWarningLog(string content)
        {
            try
            {
                var logText = this.GetFormatLogContent(content);
                var filePath = this.GetFilePath(LogLevel.Warning);
                this.AppendText(filePath, logText);

                filePath = this.GetFilePath(LogLevel.Information);
                this.AppendText(filePath, logText);
            }
            catch { }
        }

        /// <summary>
        /// Writes the warning log asynchronous.
        /// </summary>
        /// <param name="content">The content.</param>
        public async Task WriteWarningLogAsync(string content)
        {
            try
            {
                var logText = this.GetFormatLogContent(content);

                var filePath = this.GetFilePath(LogLevel.Warning);
                await this.AppendTextAsync(filePath, logText);

                filePath = this.GetFilePath(LogLevel.Information);
                await this.AppendTextAsync(filePath, logText);

            }
            catch { }
        }

        #region Private Methods
        /// <summary>
        /// Formats the log path.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <returns></returns>
        private string GetFilePath(LogLevel level)
        {
            var strDay = DateTime.Now.ToString("yyyy-MM-dd");
            var fileName= this.FmtLogFileName.Replace("{level}", level.ToString()).Replace("{day}", strDay);
            return Path.Combine(this.DirPath, fileName);
        }

        /// <summary>
        /// Formats the content of the log.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns></returns>
        private string GetFormatLogContent(string content)
        {
            return $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}: {content}\r\n";
        }

        /// <summary>
        /// Appends the text.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="content">The content.</param>
        private void AppendText(string filePath, string content)
        {
            using (StreamWriter sw = new StreamWriter(filePath, true))
            {
                sw.Write(content);
            }
        }

        /// <summary>
        /// Appends the text asynchronous.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="content">The content.</param>
        private async Task AppendTextAsync(string filePath, string content)
        {
            await Task.Run(() =>
            {
                using (StreamWriter sw = new StreamWriter(filePath, true))
                {
                    sw.Write(content);
                }
                return true;
            });
        }

        
        #endregion
    }
}
