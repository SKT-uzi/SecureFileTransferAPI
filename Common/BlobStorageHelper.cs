using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ResumableFileTransfer.Common
{
    public class BlobStorageHelper : IStorageHelper
    {
        #region Constructor
        private AzureStorageHelper StorageHelper { get; set; }
        private string ContainerName { get; set; }
        private string RelativeRootPath { get; set; }
        private string RelativePath { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureStorageHelper" /> class.
        /// </summary>
        /// <param name="storageConnectionString">The storage connection string.</param>
        /// <param name="blobFullPath">The storage root path.</param>
        public BlobStorageHelper(string storageConnectionString, string blobFullPath)
        {
            BaseHelper.SplitStorageFullPath(blobFullPath, out string containerName, out string relativeRootPath);
            this.StorageHelper = new AzureStorageHelper(storageConnectionString);
            this.ContainerName = containerName;
            this.RelativeRootPath = relativeRootPath;
            this.RelativePath = relativeRootPath;
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
                return;
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
                await Task.Run(() => { return; });
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
                    var blobName = BaseHelper.CombineStoragePath(this.RelativePath, fileName);
                    await this.StorageHelper.DeleteBlobAsync(this.ContainerName, blobName);
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
                var blobName = BaseHelper.CombineStoragePath(this.RelativePath, fileName);
                await this.StorageHelper.DeleteBlobAsync(this.ContainerName, blobName);
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
                    var blobNamePrex = BaseHelper.CombineStoragePath(this.RelativePath, folderName);
                    await this.StorageHelper.DeleteBlobFolderAsync(this.ContainerName, blobNamePrex);
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
                var blobNamePrex = BaseHelper.CombineStoragePath(this.RelativePath, folderName);
                await this.StorageHelper.DeleteBlobFolderAsync(this.ContainerName, blobNamePrex);
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
                    var itemList = await this.StorageHelper.GetBlobsByHierarchyAsync(this.ContainerName, this.RelativePath);
                    foreach (var item in itemList)
                    {
                        if (item.IsDirectory == false)
                            continue;

                        var tempFolder = BaseHelper.GetBlobFolderName(item.Name);
                        if (tempFolder == folderName)
                            return true;
                    }
                    return false;
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
                var itemList = await this.StorageHelper.GetBlobsByHierarchyAsync(this.ContainerName, this.RelativePath);
                foreach (var item in itemList)
                {
                    if (item.IsDirectory == false)
                        continue;

                    var tempFolder = BaseHelper.GetBlobFolderName(item.Name);
                    if (tempFolder == folderName)
                        return true;
                }
                return false;
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
                    var fileList = await this.StorageHelper.GetBlobsByHierarchyAsync(this.ContainerName, this.RelativePath);
                    var nameList = new List<string>();
                    foreach (var item in fileList)
                    {
                        if (item.IsDirectory)
                            continue;

                        var fileName = BaseHelper.GetFileName(item.Name);
                        if (fileName.Equals("ignore.txt"))
                            continue;

                        nameList.Add(fileName);
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
                var fileList = await this.StorageHelper.GetBlobsByHierarchyAsync(this.ContainerName, this.RelativePath);
                var nameList = new List<string>();
                foreach (var item in fileList)
                {
                    if (item.IsDirectory)
                        continue;

                    var fileName = BaseHelper.GetFileName(item.Name);
                    if (fileName.Equals("ignore.txt"))
                        continue;

                    nameList.Add(fileName);
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
                    var itemList = await this.StorageHelper.GetBlobsByHierarchyAsync(this.ContainerName, this.RelativePath);
                    var fileList = new List<StorageItemInfo>();
                    foreach (var item in itemList)
                    {
                        if (item.IsDirectory)
                            continue;

                        var fileName = BaseHelper.GetFileName(item.Name);
                        if (fileName.Equals("ignore.txt"))
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
                var itemList = await this.StorageHelper.GetBlobsByHierarchyAsync(this.ContainerName, this.RelativePath);
                var fileList = new List<StorageItemInfo>();
                foreach (var item in itemList)
                {
                    if (item.IsDirectory)
                        continue;

                    var fileName = BaseHelper.GetFileName(item.Name);
                    if (fileName.Equals("ignore.txt"))
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
                return BaseHelper.CombineStoragePath(this.ContainerName, blobName);
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
                    var fileList = await this.StorageHelper.GetBlobsByHierarchyAsync(this.ContainerName, this.RelativePath);
                    var nameList = new List<string>();
                    foreach (var item in fileList)
                    {
                        if (item.IsDirectory == false)
                            continue;

                        nameList.Add(BaseHelper.GetBlobFolderName(item.Name));
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
        /// Gets all folders asynchronous.
        /// </summary>
        /// <returns></returns>
        public async Task<List<string>> GetAllFoldersAsync()
        {
            try
            {
                var fileList = await this.StorageHelper.GetBlobsByHierarchyAsync(this.ContainerName, this.RelativePath);
                var nameList = new List<string>();
                foreach (var item in fileList)
                {
                    if (item.IsDirectory == false)
                        continue;

                    nameList.Add(BaseHelper.GetBlobFolderName(item.Name));
                }
                return nameList;
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
                if (fileName.Equals(newFileName))
                    return;

                Task.Run(async () =>
                {
                    var sourceBlobName = BaseHelper.CombineStoragePath(this.RelativePath, fileName);
                    var destBlobName = BaseHelper.CombineStoragePath(this.RelativePath, newFileName);
                    await this.StorageHelper.CopyBlobByNoneLeaseAsync(this.ContainerName, sourceBlobName, this.ContainerName, destBlobName, true);
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
                if (fileName.Equals(newFileName))
                    return;

                var sourceBlobName = BaseHelper.CombineStoragePath(this.RelativePath, fileName);
                var destBlobName = BaseHelper.CombineStoragePath(this.RelativePath, newFileName);
                await this.StorageHelper.CopyBlobByNoneLeaseAsync(this.ContainerName, sourceBlobName, this.ContainerName, destBlobName, true);
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
                    var sourceBlobName = BaseHelper.CombineStoragePath(this.RelativePath, fileName);
                    var destBlobName = BaseHelper.CombineStoragePath(this.RelativePath, destRelativeFolderPath, fileName);
                    await this.StorageHelper.CopyBlobByNoneLeaseAsync(this.ContainerName, sourceBlobName, this.ContainerName, destBlobName, true);
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

                var sourceBlobName = BaseHelper.CombineStoragePath(this.RelativePath, fileName);
                var destBlobName = BaseHelper.CombineStoragePath(this.RelativePath, destRelativeFolderPath, fileName);
                await this.StorageHelper.CopyBlobByNoneLeaseAsync(this.ContainerName, sourceBlobName, this.ContainerName, destBlobName, true);
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
                if (sourceRelativePath.Equals(destRelativePath))
                    return;

                Task.Run(async () =>
                {
                    var sourceBlobName = BaseHelper.CombineStoragePath(this.RelativePath, sourceRelativePath);
                    var destBlobName = BaseHelper.CombineStoragePath(this.RelativePath, destRelativePath);
                    await this.StorageHelper.CopyBlobByNoneLeaseAsync(this.ContainerName, sourceBlobName, this.ContainerName, destBlobName);
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
                if (sourceRelativePath.Equals(destRelativePath))
                    return;

                var sourceBlobName = BaseHelper.CombineStoragePath(this.RelativePath, sourceRelativePath);
                var destBlobName = BaseHelper.CombineStoragePath(this.RelativePath, destRelativePath);
                await this.StorageHelper.CopyBlobByNoneLeaseAsync(this.ContainerName, sourceBlobName, this.ContainerName, destBlobName);
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
                    var blobName = BaseHelper.CombineStoragePath(this.RelativePath, fileName);
                    return await this.StorageHelper.IsBlobReadyAsync(this.ContainerName, blobName);
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
                var blobName = BaseHelper.CombineStoragePath(this.RelativePath, fileName);
                return await this.StorageHelper.IsBlobReadyAsync(this.ContainerName, blobName);
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
                     var blobName = BaseHelper.CombineStoragePath(this.RelativePath, fileName);
                     await this.StorageHelper.UploadBlobAsync(this.ContainerName, blobName, stream);
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
                var blobName = BaseHelper.CombineStoragePath(this.RelativePath, fileName);
                await this.StorageHelper.UploadBlobAsync(this.ContainerName, blobName, stream);
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
                    var blobName = BaseHelper.CombineStoragePath(this.RelativePath, fileName);
                    return await this.StorageHelper.DownloadBlobAsync(this.ContainerName, blobName);
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
                var blobName = BaseHelper.CombineStoragePath(this.RelativePath, fileName);
                return await this.StorageHelper.DownloadBlobAsync(this.ContainerName, blobName);
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }
        #endregion

    }
}
