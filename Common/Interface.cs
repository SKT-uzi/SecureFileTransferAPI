using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ResumableFileTransfer.Common
{
    public interface IStorageHelper
    {
        /// <summary>
        /// Changes the dir.
        /// </summary>
        /// <param name="dirPath">The dir path.</param>
        void OpenDir(string dirPath);

        /// <summary>
        /// Deletes the file.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        void DeleteFile(string fileName);

        /// <summary>
        /// Deletes the file asynchronous.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        Task DeleteFileAsync(string fileName);

        /// <summary>
        /// Deletes the folder.
        /// </summary>
        /// <param name="folderName">Name of the folder.</param>
        void DeleteFolder(string folderName);

        /// <summary>
        /// Deletes the folder asynchronous.
        /// </summary>
        /// <param name="folderName">Name of the folder.</param>
        /// <returns></returns>
        Task DeleteFolderAsync(string folderName);

        /// <summary>
        /// Exists the folder.
        /// </summary>
        /// <param name="folderName">Name of the folder.</param>
        /// <returns></returns>
        bool ExistsFolder(string folderName);

        /// <summary>
        /// Exists the folder asynchronous.
        /// </summary>
        /// <param name="folderName">Name of the folder.</param>
        /// <returns></returns>
        Task<bool> ExistsFolderAsync(string folderName);

        /// <summary>
        /// Creates the folder.
        /// </summary>
        /// <param name="foldrName">Name of the foldr.</param>
        void CreateFolder(string foldrName);

        /// <summary>
        /// Creates the folder asynchronous.
        /// </summary>
        /// <param name="foldrName">Name of the foldr.</param>
        /// <returns></returns>
        Task CreateFolderAsync(string foldrName);

        /// <summary>
        /// Gets all files.
        /// </summary>
        /// <returns></returns>
        List<string> GetAllFiles();

        /// <summary>
        /// Gets the full path.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="subFolder">The sub folder.</param>
        /// <returns></returns>
        string GetFullPath(string fileName, string subFolder = "");

        /// <summary>
        /// Gets all files asynchronous.
        /// </summary>
        /// <returns></returns>
        Task<List<string>> GetAllFilesAsync();

        /// <summary>
        /// Gets all files.
        /// </summary>
        /// <returns></returns>
        List<StorageItemInfo> GetAllFileInfo();

        /// <summary>
        /// Gets all file information asynchronous.
        /// </summary>
        /// <returns></returns>
        Task<List<StorageItemInfo>> GetAllFileInfoAsync();

        /// <summary>
        /// Gets all folders.
        /// </summary>
        /// <returns></returns>
        List<string> GetAllFolders();

        /// <summary>
        /// Gets all folders asynchronous.
        /// </summary>
        /// <returns></returns>
        Task<List<string>> GetAllFoldersAsync();

        /// <summary>
        /// Renames the specified file name.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="newFileName">New name of the file.</param>
        /// <returns></returns>
        void Rename(string fileName, string newFileName);

        /// <summary>
        /// Renames the asynchronous.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="newFileName">New name of the file.</param>
        /// <returns></returns>
        Task RenameAsync(string fileName, string newFileName);

        /// <summary>
        /// Renames the specified file name.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="destRelativePath">The dest relative path.</param>
        /// <returns></returns>
        void Move(string fileName, string destRelativePath);

        /// <summary>
        /// Renames the asynchronous.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="destRelativeFolderPath">The dest relative folder path.</param>
        /// <returns></returns>
        Task MoveAsync(string fileName, string destRelativeFolderPath);

        /// <summary>
        /// Copies the specified old path.
        /// </summary>
        /// <param name="sourceRelativePath">The source relative path.</param>
        /// <param name="destRelativeFolderPath">The dest relative folder path.</param>
        /// <returns></returns>
        void Copy(string sourceRelativePath, string destRelativeFolderPath);

        /// <summary>
        /// Copies the asynchronous.
        /// </summary>
        /// <param name="sourceRelativePath">The source relative path.</param>
        /// <param name="destRelativePath">The dest relative path.</param>
        /// <returns></returns>
        Task CopyAsync(string sourceRelativePath, string destRelativePath);

        /// <summary>
        /// Uploads the file.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="stream">The stream.</param>
        void UploadFile(string fileName, Stream stream);

        /// <summary>
        /// Uploads the file asynchronous.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="stream">The stream.</param>
        /// <returns></returns>
        Task UploadFileAsync(string fileName, Stream stream);

        /// <summary>
        /// Determines whether the specified file name has uploading.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>
        ///   <c>true</c> if the specified file name has uploading; otherwise, <c>false</c>.
        /// </returns>
        bool IsUploadCompleted(string fileName);

        /// <summary>
        /// Determines whether [is uploading asynchronous] [the specified file name].
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        Task<bool> IsUploadCompletedAsync(string fileName);

        /// <summary>
        /// Downloads the specified file name.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        Stream Download(string fileName);

        /// <summary>
        /// Downloads the asynchronous.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        Task<Stream> DownloadAsync(string fileName);
    }

    /// <summary>
    /// ILogHelper
    /// </summary>
    public interface ILogHelper
    {
        /// <summary>
        /// Writes the text log.
        /// </summary>
        /// <param name="content">The content.</param>
        void WriteTextLog(string content);

        /// <summary>
        /// Writes the text log asynchronous.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns></returns>
        Task WriteTextLogAsync(string content);

        /// <summary>
        /// Writes the error log.
        /// </summary>
        /// <param name="content">The content.</param>
        void WriteErrorLog(string content);

        /// <summary>
        /// Writes the error log asynchronous.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns></returns>
        Task WriteErrorLogAsync(string content);

        /// <summary>
        /// Writes the warning log.
        /// </summary>
        /// <param name="content">The content.</param>
        void WriteWarningLog(string content);

        /// <summary>
        /// Writes the warning log asynchronous.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns></returns>
        Task WriteWarningLogAsync(string content);
    }

    /// <summary>
    /// IMailBox
    /// </summary>
    public interface IMailBox
    {
        List<MailItem> GetEmailList(string folderName="Inbox");
        List<MailAttachContent> DownloadAttachments(string emailID);
        bool MoveEmail(string emailID,string destFolder);
    }
}
