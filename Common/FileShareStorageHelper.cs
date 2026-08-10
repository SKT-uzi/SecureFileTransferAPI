using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ResumableFileTransfer.Common
{
    public class FileShareStorageHelper : IStorageHelper
    {
        #region Constructor
        private AzureStorageHelper StorageHelper { get; set; }
        private string ShareName { get; set; }
        private string RelativeRootPath { get; set; }
        private string RelativePath { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureStorageHelper" /> class.
        /// </summary>
        /// <param name="storageConnectionString">The storage connection string.</param>
        /// <param name="storageRootPath">The storage root path.</param>
        public FileShareStorageHelper(string storageConnectionString, string storageRootPath)
        {
            BaseHelper.SplitStorageFullPath(storageRootPath, out string shareName, out string relativeRootPath);
            this.StorageHelper = new AzureStorageHelper(storageConnectionString);
            this.ShareName = shareName;
            this.RelativeRootPath = relativeRootPath;
            this.RelativePath = this.RelativeRootPath;
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Changes the dir.
        /// </summary>
        /// <param name="dirPath">The dir path.</param>
        public void OpenDir(string dirPath)
        {
            this.RelativePath = BaseHelper.CombineStoragePath(this.RelativeRootPath, dirPath);
        }

        /// <summary>
        /// Creates the folder.
        /// </summary>
        /// <param name="foldrName">Name of the foldr.</param>
        public void CreateFolder(string foldrName)
        {
            try
            {
                Task.Run(async () =>
                {
                    var folderRelativePath = BaseHelper.CombineStoragePath(this.RelativePath, foldrName);
                    await this.StorageHelper.CreateShareDirectoryAsync(this.ShareName, folderRelativePath);
                }).Wait();
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Creates the folder asynchronous.
        /// </summary>
        /// <param name="foldrName">Name of the foldr.</param>
        /// <returns></returns>
        public async Task CreateFolderAsync(string foldrName)
        {
            try
            {
                var folderRelativePath = BaseHelper.CombineStoragePath(this.RelativePath, foldrName);
                await this.StorageHelper.CreateShareDirectoryAsync(this.ShareName, folderRelativePath);
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Deletes the file.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        public void DeleteFile(string fileName)
        {
            try
            {
                Task.Run(async () =>
                {
                    var fileRelativePath = BaseHelper.CombineStoragePath(this.RelativePath, fileName);
                    await this.StorageHelper.DeleteShareFileAsync(this.ShareName, fileRelativePath);
                }).Wait();
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Deletes the file asynchronous.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        public async Task DeleteFileAsync(string fileName)
        {
            try
            {
                var fileRelativePath = BaseHelper.CombineStoragePath(this.RelativePath, fileName);
                await this.StorageHelper.DeleteShareFileAsync(this.ShareName, fileRelativePath);
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Deletes the folder.
        /// </summary>
        /// <param name="folderName">Name of the folder.</param>
        public void DeleteFolder(string folderName)
        {
            try
            {
                Task.Run(async () =>
                 {
                     var folderRelativePath = BaseHelper.CombineStoragePath(this.RelativePath, folderName);
                     await this.StorageHelper.DeleteShareFolderAsync(this.ShareName, folderRelativePath);
                 }).Wait();
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Deletes the folder asynchronous.
        /// </summary>
        /// <param name="folderName">Name of the folder.</param>
        /// <returns></returns>
        public async Task DeleteFolderAsync(string folderName)
        {
            try
            {
                var folderRelativePath = BaseHelper.CombineStoragePath(this.RelativePath, folderName);
                await this.StorageHelper.DeleteShareFolderAsync(this.ShareName, folderRelativePath);
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Exists the folder.
        /// </summary>
        /// <param name="folderName">Name of the folder.</param>
        /// <returns></returns>
        public bool ExistsFolder(string folderName)
        {
            try
            {
                var task = Task.Run(async () =>
                {
                    var folderRelativePath = BaseHelper.CombineStoragePath(this.RelativePath, folderName);
                    return await this.StorageHelper.ExistsShareFileFolderAsync(this.ShareName, folderRelativePath);
                });
                return task.Result;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Exists the folder asynchronous.
        /// </summary>
        /// <param name="folderName">Name of the folder.</param>
        /// <returns></returns>
        public async Task<bool> ExistsFolderAsync(string folderName)
        {
            try
            {
                var folderRelativePath = BaseHelper.CombineStoragePath(this.RelativePath, folderName);
                return await this.StorageHelper.ExistsShareFileFolderAsync(this.ShareName, folderRelativePath);
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Gets all files.
        /// </summary>
        /// <returns></returns>
        public List<string> GetAllFiles()
        {
            try
            {
                var task = Task.Run(async () =>
                {
                    var itemList = await this.StorageHelper.GetShareFilesAsync(this.ShareName, this.RelativePath);
                    var nameList = new List<string>();
                    foreach (var item in itemList)
                    {
                        if (item.IsDirectory)
                            continue;

                        nameList.Add(item.Name);
                    }
                    return nameList;
                });
                return task.Result;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Gets all files asynchronous.
        /// </summary>
        /// <returns></returns>
        public async Task<List<string>> GetAllFilesAsync()
        {
            try
            {
                var itemList = await this.StorageHelper.GetShareFilesAsync(this.ShareName, this.RelativePath);
                var nameList = new List<string>();
                foreach (var item in itemList)
                {
                    if (item.IsDirectory)
                        continue;

                    nameList.Add(item.Name);
                }
                return nameList;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Gets all files.
        /// </summary>
        /// <returns></returns>
        public List<StorageItemInfo> GetAllFileInfo()
        {
            try
            {
                var task = Task.Run(async () =>
                {
                    var itemList = await this.StorageHelper.GetShareFilesAsync(this.ShareName, this.RelativePath);
                    var fileList = new List<StorageItemInfo>();
                    foreach (var item in itemList)
                    {
                        if (item.IsDirectory)
                            continue;

                        fileList.Add(item);
                    }
                    return fileList;
                });
                return task.Result;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Gets all file information asynchronous.
        /// </summary>
        /// <returns></returns>
        public async Task<List<StorageItemInfo>> GetAllFileInfoAsync()
        {
            try
            {
                var itemList = await this.StorageHelper.GetShareFilesAsync(this.ShareName, this.RelativePath);
                var fileList = new List<StorageItemInfo>();
                foreach (var item in itemList)
                {
                    if (item.IsDirectory)
                        continue;

                    fileList.Add(item);
                }
                return fileList;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Gets the full path.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="subFolder">The sub folder.</param>
        /// <returns></returns>
        public string GetFullPath(string fileName, string subFolder = "")
        {
            try
            {
                var blobName = BaseHelper.CombineStoragePath(this.RelativePath, subFolder, fileName);
                return BaseHelper.CombineStoragePath(this.ShareName, blobName);
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Gets all folders.
        /// </summary>
        /// <returns></returns>
        public List<string> GetAllFolders()
        {
            try
            {
                var task = Task.Run(async () =>
                {
                    var itemList = await this.StorageHelper.GetShareFilesAsync(this.ShareName, this.RelativePath);
                    var folderList = new List<string>();
                    foreach (var item in itemList)
                    {
                        if (item.IsDirectory == false)
                            continue;

                        folderList.Add(item.Name);
                    }
                    return folderList;
                });
                return task.Result;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Gets all folders asynchronous.
        /// </summary>
        /// <returns></returns>
        public async Task<List<string>> GetAllFoldersAsync()
        {
            try
            {
                var itemList = await this.StorageHelper.GetShareFilesAsync(this.ShareName, this.RelativePath);
                var folderList = new List<string>();
                foreach (var item in itemList)
                {
                    if (item.IsDirectory == false)
                        continue;

                    folderList.Add(item.Name);
                }
                return folderList;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Renames the specified file name.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="newFileName">New name of the file.</param>
        /// <returns></returns>
        public void Rename(string fileName, string newFileName)
        {
            try
            {
                if (BaseHelper.EqualsIgnoreCase(fileName, newFileName))
                    return;

                Task.Run(async () =>
                {
                    var fileRelativePath = BaseHelper.CombineStoragePath(this.RelativePath, fileName);
                    var newFileRelativePath = BaseHelper.CombineStoragePath(this.RelativePath, newFileName);
                    await this.StorageHelper.CopyShareFileAsync(this.ShareName, fileRelativePath, this.ShareName, newFileRelativePath, true);
                }).Wait();
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Renames the asynchronous.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="newFileName">New name of the file.</param>
        /// <returns></returns>
        public async Task RenameAsync(string fileName, string newFileName)
        {
            try
            {
                if (BaseHelper.EqualsIgnoreCase(fileName, newFileName))
                    return;

                var fileRelativePath = BaseHelper.CombineStoragePath(this.RelativePath, fileName);
                var newFileRelativePath = BaseHelper.CombineStoragePath(this.RelativePath, newFileName);
                await this.StorageHelper.CopyShareFileAsync(this.ShareName, fileRelativePath, this.ShareName, newFileRelativePath, true);
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Renames the specified file name.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="destRelativeFolderPath">The dest relative path.</param>
        /// <returns></returns>
        public void Move(string fileName, string destRelativeFolderPath)
        {
            try
            {
                if (string.IsNullOrEmpty(destRelativeFolderPath))
                    return;

                Task.Run(async () =>
                {
                    var destDir = BaseHelper.CombineStoragePath(this.RelativePath, destRelativeFolderPath);
                    var fileRelativePath = BaseHelper.CombineStoragePath(this.RelativePath, fileName);
                    var destRelativePath = BaseHelper.CombineStoragePath(destDir, fileName);
                    await this.StorageHelper.CopyShareFileAsync(this.ShareName, fileRelativePath, this.ShareName, destRelativePath, true);
                }).Wait();
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Renames the asynchronous.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="destRelativeFolderPath">The dest relative path.</param>
        /// <returns></returns>
        public async Task MoveAsync(string fileName, string destRelativeFolderPath)
        {
            try
            {
                if (string.IsNullOrEmpty(destRelativeFolderPath))
                    return;

                var destDir = BaseHelper.CombineStoragePath(this.RelativePath, destRelativeFolderPath);
                var fileRelativePath = BaseHelper.CombineStoragePath(this.RelativePath, fileName);
                var destRelativePath = BaseHelper.CombineStoragePath(destDir, fileName);
                await this.StorageHelper.CopyShareFileAsync(this.ShareName, fileRelativePath, this.ShareName, destRelativePath, true);
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Copies the specified old path.
        /// </summary>
        /// <param name="sourceRelativePath">The source relative path.</param>
        /// <param name="destRelativePath">The dest relative path.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public void Copy(string sourceRelativePath, string destRelativePath)
        {
            try
            {
                if (BaseHelper.EqualsIgnoreCase(sourceRelativePath, destRelativePath))
                    return;

                Task.Run(async () =>
                {
                    var sourcePath = BaseHelper.CombineStoragePath(this.RelativePath, sourceRelativePath);
                    var destPath = BaseHelper.CombineStoragePath(this.RelativePath, destRelativePath);
                    await this.StorageHelper.CopyShareFileAsync(this.ShareName, sourcePath, this.ShareName, destRelativePath);
                }).Wait();
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Copies the asynchronous.
        /// </summary>
        /// <param name="sourceRelativePath">The source relative path.</param>
        /// <param name="destRelativePath">The dest relative path.</param>
        /// <returns></returns>
        public async Task CopyAsync(string sourceRelativePath, string destRelativePath)
        {
            try
            {
                if (BaseHelper.EqualsIgnoreCase(sourceRelativePath, destRelativePath))
                    return;

                var sourcePath = BaseHelper.CombineStoragePath(this.RelativePath, sourceRelativePath);
                var destPath = BaseHelper.CombineStoragePath(this.RelativePath, destRelativePath);
                await this.StorageHelper.CopyShareFileAsync(this.ShareName, sourcePath, this.ShareName, destRelativePath);
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Determines whether the specified file name has uploading.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>
        ///   <c>true</c> if the specified file name has uploading; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="NotImplementedException"></exception>
        public bool IsUploadCompleted(string fileName)
        {
            try
            {
                var task = Task.Run(async () =>
                {
                    var fileRelativePath = BaseHelper.CombineStoragePath(this.RelativePath, fileName);
                    return await this.StorageHelper.IsFileShareReadyAsync(this.ShareName, fileRelativePath);
                });
                return task.Result;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Determines whether [is uploading asynchronous] [the specified file name].
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>
        ///   <c>true</c> if [is uploading asynchronous] [the specified file name]; otherwise, <c>false</c>.
        /// </returns>
        public async Task<bool> IsUploadCompletedAsync(string fileName)
        {
            try
            {
                var fileRelativePath = BaseHelper.CombineStoragePath(this.RelativePath, fileName);
                return await this.StorageHelper.IsFileShareReadyAsync(this.ShareName, fileRelativePath);
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Uploads the file.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="stream">The stream.</param>
        /// <returns></returns>
        public void UploadFile(string fileName, Stream stream)
        {
            try
            {
                Task.Run(async () =>
                {
                    var fileRelativePath = BaseHelper.CombineStoragePath(this.RelativePath, fileName);
                    await this.StorageHelper.UploadShareFileAsync(this.ShareName, fileRelativePath, stream);
                }).Wait();
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Uploads the file asynchronous.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="stream">The stream.</param>
        /// <returns></returns>
        public async Task UploadFileAsync(string fileName, Stream stream)
        {
            try
            {
                var fileRelativePath = BaseHelper.CombineStoragePath(this.RelativePath, fileName);
                await this.StorageHelper.UploadShareFileAsync(this.ShareName, fileRelativePath, stream);
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Downloads the specified file name.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        public Stream Download(string fileName)
        {
            try
            {
                var task = Task.Run(async () =>
                {
                    var fileRelativePath = BaseHelper.CombineStoragePath(this.RelativePath, fileName);
                    return await this.StorageHelper.DownloadShareFileAsync(this.ShareName, fileRelativePath);
                });
                return task.Result;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Downloads the asynchronous.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        public async Task<Stream> DownloadAsync(string fileName)
        {
            try
            {
                var fileRelativePath = BaseHelper.CombineStoragePath(this.RelativePath, fileName);
                return await this.StorageHelper.DownloadShareFileAsync(this.ShareName, fileRelativePath);
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }
        #endregion

    }
}
