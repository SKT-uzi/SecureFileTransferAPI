using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Azure;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Files.Shares;
using Azure.Storage.Sas;
using BlobModels = Azure.Storage.Blobs.Models;
using ShareModels = Azure.Storage.Files.Shares.Models;

namespace ResumableFileTransfer.Common
{
    /// <summary>
    /// AzureStorageHelper
    /// </summary>
    public class AzureStorageHelper
    {
        #region Constructor
        private string _storageConnectionString = string.Empty;
        private StorageTransferOptions _transferOptions = new StorageTransferOptions();

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureStorageHelper" /> class.
        /// </summary>
        /// <param name="storageConnectionString">The storage connection string.</param>
        /// <param name="initialTransferSize">Initial size of the transfer.4MB</param>
        /// <param name="MaximumTransferSize">Maximum size of the transfer.4MB</param>
        /// <param name="maximumConcurrency">The maximum concurrency.5</param>
        public AzureStorageHelper(string storageConnectionString, int initialTransferSize = 4, int MaximumTransferSize = 4, int maximumConcurrency = 5)
        {
            this._storageConnectionString = storageConnectionString;
            this._transferOptions.InitialTransferSize = initialTransferSize * 1024 * 1024;
            this._transferOptions.MaximumTransferSize = MaximumTransferSize * 1024 * 1024;
            this._transferOptions.MaximumConcurrency = maximumConcurrency;
        }
        #endregion

        #region Public Methods

        #region Get Blob Methods

        /// <summary>
        /// Determines whether [is uploading BLOB asynchronous] [the specified container name].
        /// </summary>
        /// <param name="containerName">Name of the container.</param>
        /// <param name="blobName">Name of the BLOB.</param>
        /// <returns>
        ///   <c>true</c> if [is uploading BLOB asynchronous] [the specified container name]; otherwise, <c>false</c>.
        /// </returns>
        internal async Task<bool> IsBlobReadyAsync(string containerName, string blobName)
        {
            try
            {
                var container = new BlobContainerClient(this._storageConnectionString, containerName);
                var blob = container.GetBlobClient(blobName);
                return await blob.ExistsAsync();
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Determines whether [is BLOB ready asynchronous] [the specified BLOB full path].
        /// </summary>
        /// <param name="blobFullPath">The BLOB full path.</param>
        /// <returns>
        ///   <c>true</c> if [is BLOB ready asynchronous] [the specified BLOB full path]; otherwise, <c>false</c>.
        /// </returns>
        public async Task<bool> IsBlobReadyAsync(string blobFullPath)
        {
            try
            {
                BaseHelper.SplitStorageFullPath(blobFullPath, out string containerName, out string blobName);
                return await this.IsBlobReadyAsync(containerName, blobName);
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Existses the BLOB asynchronous.
        /// The full path should be like {ContainerName}/{Directory}/{FileName}.{ext}
        /// </summary>
        /// <param name="blobFullPath">The BLOB full path.</param>
        /// <returns></returns>
        /// <exception cref="Exception">Container: {containerName} doesn't exist</exception>
        public async Task<bool> ExistsBlobAsync(string blobFullPath)
        {
            try
            {
                // Container
                BaseHelper.SplitStorageFullPath(blobFullPath, out string containerName, out string blobName);
                var container = await this.GetBlobContainerClient(containerName);

                // Blob
                var blob = container.GetBlobClient(blobName);
                return await blob.ExistsAsync();
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Gets the BLOB child items asynchronous.
        /// </summary>
        /// <param name="blobFullPathPrefix">The BLOB folder path.</param>
        /// <returns></returns>
        /// <exception cref="Exception">Container: {containerName} doesn't exist</exception>
        public async Task<List<string>> GetBlobItemsAsync(string blobFullPathPrefix)
        {
            try
            {
                // Container
                BaseHelper.SplitStorageFullPath(blobFullPathPrefix, out string containerName, out string blobNamePrefix);
                var container = await this.GetBlobContainerClient(containerName);

                //auto add "/"
                if (blobNamePrefix.EndsWith("/") == false && string.IsNullOrEmpty(blobNamePrefix) == false)
                    blobNamePrefix = $"{blobNamePrefix}/";

                var blobItemList = container.GetBlobs(prefix: blobNamePrefix);
                var childList = new List<string>();
                foreach (var blobItem in blobItemList)
                {
                    childList.Add($"{containerName}/{blobItem.Name}");
                }
                return childList;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Gets the blobs asynchronous.
        /// </summary>
        /// <param name="blobFullPathPrefix">The BLOB folder path.</param>
        /// <returns></returns>
        public async Task<List<BlobItemInfo>> GetBlobsAsync(string blobFullPathPrefix)
        {
            try
            {
                // Container
                BaseHelper.SplitStorageFullPath(blobFullPathPrefix, out string containerName, out string blobNamePrefix);
                var container = await this.GetBlobContainerClient(containerName);

                //auto add "/"
                if (blobNamePrefix.EndsWith("/") == false && string.IsNullOrEmpty(blobNamePrefix) == false)
                    blobNamePrefix = $"{blobNamePrefix}/";

                var blobItemList = container.GetBlobs(prefix: blobNamePrefix);
                var childList = new List<BlobItemInfo>();
                foreach (var blobItem in blobItemList)
                {
                    var file = new BlobItemInfo();
                    file.IsDirectory = false;
                    file.Name = $"{containerName}/{blobItem.Name}";
                    file.FileSize = blobItem.Properties.ContentLength;
                    file.ModifyDate = blobItem.Properties.LastModified.Value.LocalDateTime;
                    file.CreateDate = blobItem.Properties.CreatedOn.Value.LocalDateTime;
                    file.URL = $"{container.Uri}/{blobItem.Name}";
                    childList.Add(file);
                }
                return childList;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Gets the BLOB asynchronous.
        /// </summary>
        /// <param name="blobFullPath">The BLOB full path.</param>
        /// <returns></returns>
        public async Task<BlobItemInfo> GetBlobItemAsync(string blobFullPath)
        {
            try
            {
                // Container
                BaseHelper.SplitStorageFullPath(blobFullPath, out string containerName, out string blobName);
                var container = await this.GetBlobContainerClient(containerName);

                var blob = container.GetBlobClient(blobName);
                if (await blob.ExistsAsync() == false)
                    return null;

                var blobProperties= await blob.GetPropertiesAsync();

                var file = new BlobItemInfo();
                file.IsDirectory = false;
                file.Name = $"{containerName}/{blobName}";
                file.FileSize = blobProperties.Value.ContentLength;
                file.ModifyDate = blobProperties.Value.LastModified.LocalDateTime;
                file.CreateDate = blobProperties.Value.CreatedOn.LocalDateTime;
                file.URL = BaseHelper.UrlDecode(blob.Uri.ToString());
                return file;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Gets the blobs by hierarchy asynchronous.
        /// </summary>
        /// <param name="containerName">Name of the container.</param>
        /// <param name="blobNamePrefix">Name of the pre BLOB.</param>
        /// <param name="ignoreFolder">if set to <c>true</c> [ignore folder].</param>
        /// <param name="matchExtension">The match extension.</param>
        /// <returns></returns>
        internal async Task<List<BlobItemInfo>> GetBlobsByHierarchyAsync(string containerName, string blobNamePrefix, bool ignoreFolder = false, List<string> matchExtension = null)
        {
            try
            {
                //auto add "/"
                if (blobNamePrefix.EndsWith("/") == false && string.IsNullOrEmpty(blobNamePrefix) == false)
                    blobNamePrefix = $"{blobNamePrefix}/";

                var container = await this.GetBlobContainerClient(containerName);
                var enumerator = container.GetBlobsByHierarchyAsync(prefix: blobNamePrefix, delimiter: "/").GetAsyncEnumerator();
                var childList = new List<BlobItemInfo>();
                try
                {
                    while (await enumerator.MoveNextAsync())
                    {
                        var item = enumerator.Current;
                        if (ignoreFolder && item.IsPrefix)
                            continue;

                        if (item.IsPrefix == false && matchExtension != null)
                        {
                            var match = false;
                            foreach (var matchExt in matchExtension)
                            {
                                if (item.Blob.Name.EndsWith(matchExt, StringComparison.OrdinalIgnoreCase))
                                {
                                    match = true;
                                    break;
                                }
                            }
                            if (match == false)
                                continue;
                        }

                        var file = new BlobItemInfo();
                        if (item.IsPrefix)
                        {
                            file.IsDirectory = true;
                            file.Name = $"{containerName}/{item.Prefix}";
                        }
                        else
                        {
                            file.IsDirectory = false;
                            file.Name = $"{containerName}/{item.Blob.Name}";
                            file.FileSize = item.Blob.Properties.ContentLength;
                            file.CreateDate = item.Blob.Properties.CreatedOn.Value.LocalDateTime;
                            file.ModifyDate = item.Blob.Properties.LastModified.Value.LocalDateTime;
                            file.URL = $"{container.Uri}/{item.Blob.Name}";
                        }
                        childList.Add(file);
                    }
                }
                finally
                {
                    await enumerator.DisposeAsync();
                }
                return childList;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Gets the blobs by hierarchy asynchronous.
        /// </summary>
        /// <param name="blobFullPathPrefix">The pre BLOB folder path.</param>
        /// <param name="ignoreFolder">if set to <c>true</c> [ignore folder].</param>
        /// <param name="matchExtension">The match extension.</param>
        /// <returns></returns>
        public async Task<List<BlobItemInfo>> GetBlobsByHierarchyAsync(string blobFullPathPrefix, bool ignoreFolder = false, List<string> matchExtension = null)
        {
            try
            {
                BaseHelper.SplitStorageFullPath(blobFullPathPrefix, out string containerName, out string blobNamePrefix);
                return await this.GetBlobsByHierarchyAsync(containerName, blobNamePrefix, ignoreFolder, matchExtension);
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Downloads the BLOB asynchronous.
        /// </summary>
        /// <param name="containerName">Name of the container.</param>
        /// <param name="blobName">Name of the BLOB.</param>
        /// <returns></returns>
        internal async Task<MemoryStream> DownloadBlobAsync(string containerName, string blobName)
        {
            try
            {
                // Container
                var container = await this.GetBlobContainerClient(containerName);

                // Blob
                var blob = container.GetBlobClient(blobName);
                await this.ExistsSourceAsync(blob);

                // Download
                var blobProperties = await blob.GetPropertiesAsync();
                if (blobProperties.Value.ContentLength > Int32.MaxValue)
                {
                    throw new Exception($"The file size exceeds 2GB,{blob.Uri}");
                }

                var memoryStream = new MemoryStream(Convert.ToInt32(blobProperties.Value.ContentLength));
                await blob.DownloadToAsync(memoryStream, default, this._transferOptions);
                memoryStream.Seek(0, SeekOrigin.Begin);
                return memoryStream;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Downloads the BLOB asynchronous.
        /// The full path should be like {ContainerName}/{Directory}/{FileName}.{ext}
        /// </summary>
        /// <param name="blobFullPath">Full name of the BLOB.</param>
        /// <returns></returns>
        /// <exception cref="Exception">
        /// Container: {containerName} doesn't exist
        /// or
        /// Source Blob: {blobName} doesn't exist in Container: {containerName}
        /// </exception>
        public async Task<MemoryStream> DownloadBlobAsync(string blobFullPath)
        {
            try
            {
                // Container
                BaseHelper.SplitStorageFullPath(blobFullPath, out string containerName, out string blobName);
                return await this.DownloadBlobAsync(containerName, blobName);
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Downloads the BLOB from stream asynchronous.
        /// </summary>
        /// <param name="blobFullPath">The BLOB full path.</param>
        /// <returns></returns>
        public async Task<Stream> DownloadBlobFromStreamAsync(string blobFullPath)
        {
            try
            {
                // Container
                BaseHelper.SplitStorageFullPath(blobFullPath, out string containerName, out string blobName);

                // Container
                var container = await this.GetBlobContainerClient(containerName);

                // Blob
                var blob = container.GetBlobClient(blobName);
                if (await blob.ExistsAsync() == false)
                    return null;

                return await blob.OpenReadAsync();
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Downloads the BLOB to string asynchronous.
        /// </summary>
        /// <param name="blobFullPath">The BLOB full path.</param>
        /// <returns></returns>
        public async Task<string> DownloadBlobToStringAsync(string blobFullPath)
        {
            try
            {
                // Container
                BaseHelper.SplitStorageFullPath(blobFullPath, out string containerName, out string blobName);

                // Container
                var container = await this.GetBlobContainerClient(containerName);

                // Blob
                var blob = container.GetBlobClient(blobName);
                if (await blob.ExistsAsync() == false)
                    return null;

                var downloadResult = await blob.DownloadContentAsync();
                return downloadResult.Value.Content.ToString();

            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Downloads the BLOB by URL asynchronous.
        /// </summary>
        /// <param name="blobURL">The BLOB URL.</param>
        /// <returns></returns>
        public async Task<MemoryStream> DownloadBlobByURLAsync(string blobURL)
        {
            try
            {
                var serviceClient = new BlobServiceClient(this._storageConnectionString);

                //generate SAS Credential
                var sasURL = serviceClient.GenerateAccountSasUri(AccountSasPermissions.Read, DateTimeOffset.Now.AddDays(1), AccountSasResourceTypes.All);
                var sasCredential = new AzureSasCredential(sasURL.Query);

                //Blob Client
                var blobClient = new BlobClient(new Uri(blobURL), sasCredential);
                await this.ExistsSourceAsync(blobClient);

                var blobProperties = await blobClient.GetPropertiesAsync();
                if (blobProperties.Value.ContentLength > Int32.MaxValue)
                {
                    throw new Exception($"The file size exceeds 2GB,{blobClient.Uri}");
                }

                // Download
                var memoryStream = new MemoryStream(Convert.ToInt32(blobProperties.Value.ContentLength));
                await blobClient.DownloadToAsync(memoryStream, default, this._transferOptions);
                memoryStream.Seek(0, SeekOrigin.Begin);
                return memoryStream;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Get sas uri of blob.
        /// The full path should be like {ContainerName}/{Directory}/{FileName}.{ext}
        /// </summary>
        /// <param name="blobFullPath">Full name of the BLOB.</param>
        /// <returns></returns>
        /// <exception cref="Exception">
        /// Container: {containerName} doesn't exist
        /// or
        /// Source Blob: {blobName} doesn't exist in Container: {containerName}
        /// </exception>
        public async Task<Uri> GetBlobSasUri(string blobFullPath)
        {
            try
            {
                // Container
                BaseHelper.SplitStorageFullPath(blobFullPath, out string containerName, out string blobName);

                // Container
                var container = await this.GetBlobContainerClient(containerName);

                // Blob
                var blob = container.GetBlobClient(blobName);
                await this.ExistsSourceAsync(blob);

                return GetServiceSasUriForBlob(blob);

            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }
        #endregion

        #region Upload Blob Methods
        /// <summary>
        /// Opens the write asynchronous.
        /// </summary>
        /// <param name="blobFullPath">The BLOB full path.</param>
        /// <param name="WriteContent">Content of the write.</param>
        /// <returns></returns>
        public async Task<long> OpenWriteBlobAsync(string blobFullPath, Action<Stream> WriteContent)
        {
            try
            {
                BaseHelper.SplitStorageFullPath(blobFullPath, out string containerName, out string blobName);
                var container = await this.GetBlobContainerClient(containerName);
                var blockBlob = container.GetBlockBlobClient(blobName);
                var option = this.GetBlockBlobOpenWriteOptions(blobName);

                using (var stream = await blockBlob.OpenWriteAsync(true, option))
                {
                    try
                    {
                        WriteContent(stream);
                        var properties = await blockBlob.GetPropertiesAsync();
                        return properties.Value.ContentLength;
                    }
                    catch (Exception exContent)
                    {
                        await blockBlob.DeleteIfExistsAsync();
                        throw exContent;
                    }
                }
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Uploads the block BLOB asynchronous.
        /// </summary>
        /// <param name="blobFullPath">The BLOB full path.</param>
        /// <param name="blockContent">Content of the block.</param>
        /// <param name="contentLength">Length of the content.</param>
        public async Task<UploadStatus> UploadBlockBlobAsync(string blobFullPath, byte[] blockContent,long contentLength)
        {
            try
            {
                BaseHelper.SplitStorageFullPath(blobFullPath, out string containerName, out string blobName);
                var container = await this.GetBlobContainerClient(containerName);
                var blockBlob = container.GetBlockBlobClient(blobName);

                var blockIndex = 0;
                var blockID = string.Empty;
                var unCommitLength = 0L;
                var blockIndexList = new List<int>();
                using (var stream = new MemoryStream(blockContent))
                {
                    try
                    {
                        var blockList = await blockBlob.GetBlockListAsync(BlobModels.BlockListTypes.Uncommitted);
                        //block index is after block index ++
                        foreach (var block in blockList.Value.UncommittedBlocks)
                        {
                            blockIndex = Convert.ToInt32(BaseHelper.ConvertBase64ToString(block.Name));
                            blockIndexList.Add(blockIndex);
                            unCommitLength += block.SizeLong;
                        }
                        blockIndex = blockIndexList.Count + 1;
                    }
                    catch (RequestFailedException failedEx)
                    {
                        if (failedEx.ErrorCode != "BlobNotFound")
                            throw failedEx;

                        //need create new blob, block index start 1
                        blockIndex = 1;
                    }

                    blockID = BaseHelper.ConvertStringToBase64(blockIndex.ToString("d5"));
                    await blockBlob.StageBlockAsync(blockID, stream);
                    blockIndexList.Add(blockIndex);
                    unCommitLength += blockContent.Length;
                }

                //this file upload completed, need commit
                if (unCommitLength == contentLength)
                {
                    blockIndexList.Sort();
                    var blockIDList = new List<string>();
                    foreach (var item in blockIndexList)
                    {
                        blockID = BaseHelper.ConvertStringToBase64(item.ToString("d5"));
                        blockIDList.Add(blockID);
                    }
                    var option = this.GetCommitBlockListOptions(blobName);
                    await blockBlob.CommitBlockListAsync(blockIDList, option);
                    return  UploadStatus.Completed;
                }
                return UploadStatus.InProgress;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Gets the block BLOB offset.
        /// </summary>
        /// <param name="blobFullPath">The BLOB full path.</param>
        /// <returns></returns>
        public async Task<long> GetBlockBlobOffset(string blobFullPath)
        {
            try
            {
                BaseHelper.SplitStorageFullPath(blobFullPath, out string containerName, out string blobName);
                var container = await this.GetBlobContainerClient(containerName);
                var blockBlob = container.GetBlockBlobClient(blobName);
                var unCommitLength = 0L;
                try
                {
                    var blockList = await blockBlob.GetBlockListAsync(BlobModels.BlockListTypes.All);
                    foreach (var block in blockList.Value.UncommittedBlocks)
                    {
                        unCommitLength += block.SizeLong;
                    }

                    if (blockList.Value.CommittedBlocks.Count() != 0 && unCommitLength == 0)
                    {
                        var properties = await blockBlob.GetPropertiesAsync();
                        return properties.Value.ContentLength;
                    }
                }
                catch (RequestFailedException failedEx)
                {
                    if (failedEx.ErrorCode != "BlobNotFound")
                        throw failedEx;
                }
                return unCommitLength;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Creates the BLOB asynchronous.
        /// </summary>
        /// <param name="containerName">Name of the container.</param>
        /// <param name="blobName">Name of the BLOB.</param>
        /// <param name="stream">The stream.</param>
        /// <returns></returns>
        /// <exception cref="Exception">Container: {containerName} doesn't exist</exception>
        internal async Task UploadBlobAsync(string containerName, string blobName, Stream stream)
        {
            try
            {
                stream.Seek(0, SeekOrigin.Begin);
                var container = await this.GetBlobContainerClient(containerName);
                var blob = container.GetBlobClient(blobName);
                var options = this.GetUploadOptions(blobName);
                await blob.UploadAsync(stream, options);
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Creates the BLOB asynchronous.
        /// The full path should be like {ContainerName}/{Directory}/{FileName}.{ext}
        /// </summary>
        /// <param name="blobFullPath">Full name of the BLOB.</param>
        /// <param name="stream">The stream.</param>
        /// <returns></returns>
        /// <exception cref="Exception">Container: {containerName} doesn't exist</exception>
        public async Task UploadBlobAsync(string blobFullPath, Stream stream)
        {
            try
            {
                BaseHelper.SplitStorageFullPath(blobFullPath, out string containerName, out string blobName);
                await this.UploadBlobAsync(containerName, blobName, stream);
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        #endregion

        #region Delete Blob Methods
        /// <summary>
        /// Deletes the BLOB asynchronous.
        /// </summary>
        /// <param name="containerName">Name of the container.</param>
        /// <param name="blobName">Name of the BLOB.</param>
        /// <returns></returns>
        internal async Task DeleteBlobAsync(string containerName, string blobName)
        {
            try
            {
                // Container
                var container = await this.GetBlobContainerClient(containerName);

                // Blob
                await container.DeleteBlobIfExistsAsync(blobName);
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Deletes the BLOB asynchronous.
        /// The full path should be like {ContainerName}/{Directory}/{FileName}.{ext}
        /// </summary>
        /// <param name="blobFullPath">Full name of the BLOB.</param>
        /// <returns></returns>
        /// <exception cref="Exception">Container: {containerName} doesn't exist</exception>
        public async Task DeleteBlobAsync(string blobFullPath)
        {
            try
            {
                BaseHelper.SplitStorageFullPath(blobFullPath, out string containerName, out string blobName);
                await this.DeleteBlobAsync(containerName, blobName);
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Deletes the BLOB folder asynchronous.
        /// </summary>
        /// <param name="containerName">Name of the container.</param>
        /// <param name="relativeFolderPath">The relative folder path.</param>
        /// <returns></returns>
        internal async Task DeleteBlobFolderAsync(string containerName, string relativeFolderPath)
        {
            try
            {
                if (relativeFolderPath.EndsWith("/") == false && string.IsNullOrEmpty(relativeFolderPath)==false)
                    relativeFolderPath = $"{relativeFolderPath}/";

                var container = await this.GetBlobContainerClient(containerName);
                var blobItemList = container.GetBlobs(prefix: relativeFolderPath);
                foreach (var blobItem in blobItemList)
                {
                    await container.DeleteBlobIfExistsAsync(blobItem.Name);
                }
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Deletes the BLOB folder asynchronous.
        /// </summary>
        /// <param name="folderFullPath">The folder full path.</param>
        /// <returns></returns>
        public async Task DeleteBlobFolderAsync(string folderFullPath)
        {
            try
            {
                BaseHelper.SplitStorageFullPath(folderFullPath, out string containerName, out string relativeFolderPath);
                await this.DeleteBlobFolderAsync(containerName, relativeFolderPath);
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Cleans the BLOB asynchronous.
        /// </summary>
        /// <param name="blobFolderFullPath">The BLOB folder path.</param>
        /// <param name="cleanDays">The clean days.</param>
        /// <returns></returns>
        /// <exception cref="Exception">Container: {containerName} doesn't exist</exception>
        public async Task<int> CleanBlobAsync(string blobFolderFullPath, int cleanDays)
        {
            try
            {
                // Container
                BaseHelper.SplitStorageFullPath(blobFolderFullPath, out string containerName, out string relativePath);
                if (relativePath.EndsWith("/") == false && string.IsNullOrEmpty(relativePath)==false)
                    relativePath = $"{relativePath}/";

                var cleanBeforeDate = DateTime.Now.AddDays(cleanDays * -1);
                var container = await this.GetBlobContainerClient(containerName);
                var blobItemList = container.GetBlobs(prefix: relativePath);
                var cleanCount = 0;
                foreach (var blobItem in blobItemList)
                {
                    if (blobItem.Properties.LastModified.Value.LocalDateTime < cleanBeforeDate)
                    {
                        await container.DeleteBlobIfExistsAsync(blobItem.Name);
                        cleanCount++;
                    }
                }
                return cleanCount;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }
        #endregion

        #region Other Blob Methods
        /// <summary>
        /// Releases the BLOB asynchronous.
        /// </summary>
        /// <param name="blobFullPath">The BLOB full path.</param>
        public async Task ReleaseBlobAsync(string blobFullPath)
        {
            try
            {
                BaseHelper.SplitStorageFullPath(blobFullPath, out string containerName, out string blobName);
                var container = await this.GetBlobContainerClient(containerName);
                var blob = container.GetBlobClient(blobName);
                await this.ExistsSourceAsync(blob);

                var blobLease = blob.GetBlobLeaseClient();
                await blobLease.ReleaseAsync();
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Copies the BLOB asynchronous.
        /// The full path should be like {ContainerName}/{Directory}/{FileName}.{ext}
        /// </summary>
        /// <param name="sourceBlobFullPath">The source BLOB path.</param>
        /// <param name="destBlobFullPath">The dest BLOB path.</param>
        /// <param name="deleteSource">if set to <c>true</c> [delete source].</param>
        /// <returns></returns>
        public async Task CopyBlobAsync(string sourceBlobFullPath, string destBlobFullPath, bool deleteSource = false)
        {
            var sourceBlobLease = (BlobLeaseClient)null;
            try
            {
                BaseHelper.SplitStorageFullPath(sourceBlobFullPath, out string sourceContainerName, out string sourceBlobName);
                BaseHelper.SplitStorageFullPath(destBlobFullPath, out string destContainerName, out string destBlobName);

                var sourceContainer = await this.GetBlobContainerClient(sourceContainerName);
                var destContainer = await this.GetBlobContainerClient(destContainerName);

                // Source Blob
                var sourceBlob = sourceContainer.GetBlobClient(sourceBlobName);
                await this.ExistsSourceAsync(sourceBlob);

                // Get the source blob's properties and display the lease state.
                var sourceProperties = await sourceBlob.GetPropertiesAsync();
                if (sourceProperties.Value.LeaseStatus == BlobModels.LeaseStatus.Locked)
                {
                    throw new Exception($"The file is Locked({sourceBlobFullPath})");
                }

                // Lease the source blob for the copy operation to prevent another client from modifying it.
                sourceBlobLease = sourceBlob.GetBlobLeaseClient();

                // Specifying -1 for the lease interval creates an infinite lease.
                await sourceBlobLease.AcquireAsync(TimeSpan.FromSeconds(-1));

                // Get a BlobClient representing the destination blob with a unique name.
                var destBlob = sourceContainer.GetBlobClient(destBlobName);

                // Start the copy operation.
                await destBlob.StartCopyFromUriAsync(sourceBlob.Uri);
                if (this.GetCopyCompletedStatus(destBlob, out string copyStatusMsg) == false)
                {
                    await destBlob.DeleteIfExistsAsync();
                    throw new Exception($"The file copy failed, {copyStatusMsg}, ({sourceBlobFullPath} to {destBlobFullPath})");
                }

                // Update the source blob's properties.
                await sourceBlobLease.ReleaseAsync();
                if (deleteSource == true)
                {
                    await sourceBlob.DeleteIfExistsAsync();
                }
            }
            catch (Exception ex)
            {
                if (sourceBlobLease != null)
                    await sourceBlobLease.ReleaseAsync();
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Copies the BLOB by none lease asynchronous.
        /// </summary>
        /// <param name="sourceContainerName">Name of the source container.</param>
        /// <param name="sourceBlobName">Name of the source BLOB.</param>
        /// <param name="destContainerName">Name of the dest container.</param>
        /// <param name="destBlobName">Name of the dest BLOB.</param>
        /// <param name="deleteSource">if set to <c>true</c> [delete source].</param>
        /// <exception cref="System.Exception">The file copy failed, {copyStatusMsg}, ({sourceContainerName}/{sourceBlobName} to {destContainerName}/{destBlobName})</exception>
        internal async Task CopyBlobByNoneLeaseAsync(string sourceContainerName, string sourceBlobName, string destContainerName, string destBlobName, bool deleteSource = false)
        {
            try
            {
                var sourceContainer = await this.GetBlobContainerClient(sourceContainerName);
                var destContainer = await this.GetBlobContainerClient(destContainerName);

                // Source Blob
                var sourceBlob = sourceContainer.GetBlobClient(sourceBlobName);
                await this.ExistsSourceAsync(sourceBlob);

                // Get a BlobClient representing the destination blob with a unique name.
                var destBlob = sourceContainer.GetBlobClient(destBlobName);

                // Start the copy operation.
                await destBlob.StartCopyFromUriAsync(sourceBlob.Uri);
                if (this.GetCopyCompletedStatus(destBlob, out string copyStatusMsg) == false)
                {
                    await destBlob.DeleteIfExistsAsync();
                    throw new Exception($"The file copy failed, {copyStatusMsg}, ({sourceContainerName}/{sourceBlobName} to {destContainerName}/{destBlobName})");
                }

                if (deleteSource == true)
                {
                    await sourceBlob.DeleteIfExistsAsync();
                }
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Copies the BLOB by none lease asynchronous.
        /// </summary>
        /// <param name="sourceBlobFullPath">The source BLOB full path.</param>
        /// <param name="destBlobFullPath">The dest BLOB full path.</param>
        /// <param name="deleteSource">if set to <c>true</c> [delete source].</param>
        public async Task CopyBlobByNoneLeaseAsync(string sourceBlobFullPath, string destBlobFullPath, bool deleteSource = false)
        {
            try
            {
                BaseHelper.SplitStorageFullPath(sourceBlobFullPath, out string sourceContainerName, out string sourceBlobName);
                BaseHelper.SplitStorageFullPath(destBlobFullPath, out string destContainerName, out string destBlobName);
                await this.CopyBlobByNoneLeaseAsync(sourceContainerName, sourceBlobName, destContainerName, destBlobName, deleteSource);
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Compresses the BLOB to zip asynchronous.
        /// The full path should be like {ContainerName}/{Directory}/{FileName}.{ext}
        /// </summary>
        /// <param name="sourceBlobFullPath">The source BLOB full path.</param>
        /// <param name="destBlobFullPath">The dest BLOB full path.</param>
        /// <param name="deleteSource">if set to <c>true</c> [delete source].</param>
        /// <returns></returns>
        /// <exception cref="Exception">Container: {containerName} doesn't exist
        /// or
        /// Blob: {sourceBlobName} doesn't exist</exception>
        public async Task CompressBlobToZipAsync(string sourceBlobFullPath, string destBlobFullPath, bool deleteSource = false)
        {
            try
            {
                BaseHelper.SplitStorageFullPath(sourceBlobFullPath, out string sourceContainerName, out string sourceBlobName);
                BaseHelper.SplitStorageFullPath(destBlobFullPath, out string destContainerName, out string destBlobName);

                // Container
                var sourceContainer = await this.GetBlobContainerClient(sourceContainerName);
                var destContainer = await this.GetBlobContainerClient(destContainerName);

                // Blob
                var sourceBlob = sourceContainer.GetBlobClient(sourceBlobName);
                await this.ExistsSourceAsync(sourceBlob);

                var sourceBlobProperties= await sourceBlob.GetPropertiesAsync();
                if (sourceBlobProperties.Value.ContentLength > Int32.MaxValue)
                {
                    throw new Exception($"The file size exceeds 2GB,{sourceBlob.Uri}");
                }
                
                // Download
                var sourceFileName = BaseHelper.GetFileName(sourceBlobName);
                using (var streamZip = new MemoryStream())
                {
                    using (var zipHelper = new ZipHelper(streamZip))
                    {
                        using (var streamFile = new MemoryStream(Convert.ToInt32(sourceBlobProperties.Value.ContentLength)))
                        {
                            await sourceBlob.DownloadToAsync(streamFile, default, this._transferOptions);
                            streamFile.Seek(0, SeekOrigin.Begin);
                            zipHelper.AddContent(sourceFileName, streamFile);
                        }
                    }
                    // upload Destination Blob
                    var destBlob = destContainer.GetBlobClient(destBlobName);
                    await destBlob.DeleteIfExistsAsync();
                    streamZip.Seek(0, SeekOrigin.Begin);

                    var option = this.GetUploadOptions(destBlobName);
                    await destBlob.UploadAsync(streamZip, option);
                }

                if (deleteSource == true)
                {
                    await sourceBlob.DeleteIfExistsAsync();
                }
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Copies the BLOB to share asynchronous.
        /// The full path should be like {ContainerName}/{Directory}/{FileName}.{ext}
        /// </summary>
        /// <param name="sourceBlobFullPath">The source BLOB full path.</param>
        /// <param name="destShareFileFullPath">The dest share file full path.</param>
        /// <returns></returns>
        /// <exception cref="Exception">
        /// Container: {sourceContainerName} doesn't exist
        /// or
        /// Share: {destShareName} doesn't exist
        /// </exception>
        public async Task CopyBlobToShareFileAsync(string sourceBlobFullPath, string destShareFileFullPath)
        {
            try
            {
                BaseHelper.SplitStorageFullPath(sourceBlobFullPath, out string sourceContainerName, out string sourceBlobName);
                BaseHelper.SplitStorageFullPath(destShareFileFullPath, out string destShareName, out string destShareRelativePath);
                BaseHelper.SplitStorageRelativePath(destShareRelativePath, out string destShareDirName, out string destFileName);

                // Container
                var sourceContainer = await this.GetBlobContainerClient(sourceContainerName);

                // Source blob
                var sourceBlob = sourceContainer.GetBlobClient(sourceBlobName);
                await this.ExistsSourceAsync(sourceBlob);

                // Dest file share
                var destShare = await this.GetShareClient(destShareName);
                var destShareDir = await this.CreateShareDirectoryAsync(destShare, destShareDirName);
                var destShareFile = destShareDir.GetFileClient(destFileName);
               
                var sasURL = sourceBlob.GenerateSasUri(BlobSasPermissions.Read, DateTimeOffset.Now.AddDays(1));
                await destShareFile.StartCopyAsync(sasURL);
                if (this.GetCopyCompletedStatus(destShareFile, out string copyStatusMsg) == false)
                {
                    await destShareFile.DeleteIfExistsAsync();
                    throw new Exception($"The file copy failed, {copyStatusMsg}, ({sourceBlobFullPath} to {destShareFileFullPath})");
                }
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }
        #endregion

        #region Get FileShare Methods

        /// <summary>
        /// Determines whether the specified share name is uploading.
        /// </summary>
        /// <param name="shareName">Name of the share.</param>
        /// <param name="shareRelativePath">The share relative path.</param>
        /// <returns>
        ///   <c>true</c> if the specified share name is uploading; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="Exception">Share: {shareName} doesn't exist</exception>
        internal async Task<bool> IsFileShareReadyAsync(string shareName, string shareRelativePath)
        {
            try
            {
                BaseHelper.SplitStorageRelativePath(shareRelativePath, out string dirPath, out string fileName);
                var share = await this.GetShareClient(shareName);
                var shareDir = share.GetDirectoryClient(dirPath);
                var shareFile = shareDir.GetFileClient(fileName);
                if (await shareFile.ExistsAsync() == false)
                    return false;

                //file share completed: 1. FileChangedOn<>FileCreatedOn 
                var download = await shareFile.DownloadAsync();
                var subProperties = download.Value.Details.SmbProperties;
                var totalChangeOn = subProperties.FileChangedOn.Value.Subtract(subProperties.FileCreatedOn.Value).TotalMilliseconds;
                if (totalChangeOn != 0)
                    return true;

                //2. PreDownload.LastModified = NextDownload LastModified or FileChangedOn<>FileCreatedOn, 
                await Task.Delay(60000);
                var download2 = await shareFile.DownloadAsync();
                var subProperties2 = download2.Value.Details.SmbProperties;
                var totalChangeOn2 = subProperties2.FileChangedOn.Value.Subtract(subProperties2.FileCreatedOn.Value).TotalMilliseconds;
                var totalLastModified2 = download2.Value.Details.LastModified.Subtract(download.Value.Details.LastModified).TotalMilliseconds;

                //totalLastModified2=0: after 1 minute and the file none change, so the file completed
                return totalLastModified2 == 0 || totalChangeOn2 != 0;
            }
            catch (Exception ex)
            {
                //filezilla inprogress upload
                if (ex.Message.Contains("The specified resource may be in use by an SMB client."))
                    return false;

                //the file 0 byte
                if (ex.Message.Contains("The range specified is invalid for the current size of the resource"))
                    return false;

                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Determines whether [is file share ready asynchronous] [the specified share file full path].
        /// </summary>
        /// <param name="shareFullPath">The share file full path.</param>
        /// <returns>
        ///   <c>true</c> if [is file share ready asynchronous] [the specified share file full path]; otherwise, <c>false</c>.
        /// </returns>
        public async Task<bool> IsFileShareReadyAsync(string shareFullPath)
        {
            try
            {
                BaseHelper.SplitStorageFullPath(shareFullPath, out string shareName, out string shareRelativePath);
                return await this.IsFileShareReadyAsync(shareName, shareRelativePath);
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Existses the share file folder asynchronous.
        /// </summary>
        /// <param name="shareName">Name of the share.</param>
        /// <param name="shareRelativePath">The share relative path.</param>
        /// <returns></returns>
        /// <exception cref="Exception">Share: {shareName} doesn't exist</exception>
        internal async Task<bool> ExistsShareFileFolderAsync(string shareName, string shareRelativePath)
        {
            try
            {
                // File Share
                var share = await this.GetShareClient(shareName);

                // Directory
                var shareDir = share.GetDirectoryClient(shareRelativePath);
                return await shareDir.ExistsAsync();
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Existses the share file folder asynchronous.
        /// </summary>
        /// <param name="shareFullPath">The share file folder full path.</param>
        /// <returns></returns>
        public async Task<bool> ExistsShareFileFolderAsync(string shareFullPath)
        {
            try
            {
                BaseHelper.SplitStorageFullPath(shareFullPath, out string shareName, out string shareRelativePath);
                return await this.ExistsShareFileFolderAsync(shareName, shareRelativePath);
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Existses the file asynchronous.
        /// The full path should be like {ShareName}/{Directory}/{FileName}.{ext}
        /// </summary>
        /// <param name="shareFullPath">Full name of the file share.</param>
        /// <returns></returns>
        /// <exception cref="Exception">Share: {shareName} doesn't exist</exception>
        public async Task<bool> ExistsShareFileAsync(string shareFullPath)
        {
            try
            {
                BaseHelper.SplitStorageFullPath(shareFullPath, out string shareName, out string shareRelativePath);
                BaseHelper.SplitStorageRelativePath(shareRelativePath, out string dirPath, out string fileName);

                // File Share
                var share = await this.GetShareClient(shareName);
                
                // Directory
                var shareDir = share.GetDirectoryClient(dirPath);
                if (await shareDir.ExistsAsync() == false)
                    return false;

                var shareFile = shareDir.GetFileClient(fileName);
                return await shareFile.ExistsAsync();
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Gets the share file child items asynchronous.
        /// </summary>
        /// <param name="shareName">Name of the share.</param>
        /// <param name="shareRelativePath">The share relative path.</param>
        /// <param name="ignoreFolder">if set to <c>true</c> [ignore folder].</param>
        /// <returns></returns>
        /// <exception cref="Exception">Share: {shareName} doesn't exist</exception>
        internal async Task<List<FileShareItemInfo>> GetShareFilesAsync(string shareName, string shareRelativePath, bool ignoreFolder = false)
        {
            try
            {
                var share = await this.GetShareClient(shareName);

                // Directory
                var fileList = new List<FileShareItemInfo>();
                var shareDir = share.GetDirectoryClient(shareRelativePath);
                if (await shareDir.ExistsAsync() == false)
                    return fileList;

                var enumerator = shareDir.GetFilesAndDirectoriesAsync().GetAsyncEnumerator();
                try
                {
                    while (await enumerator.MoveNextAsync())
                    {
                        var item = enumerator.Current;
                        if (ignoreFolder && item.IsDirectory)
                            continue;

                        var file = new FileShareItemInfo();
                        file.Name = item.Name;
                        file.DirName = shareRelativePath;
                        file.IsDirectory = item.IsDirectory;
                        if (item.IsDirectory == false)
                        {
                            file.FileSize = item.FileSize;
                            var fileClient = shareDir.GetFileClient(item.Name);
                            var fileProperties = await fileClient.GetPropertiesAsync();
                            file.ModifyDate = fileProperties.Value.LastModified.LocalDateTime;
                            file.CreateDate = fileProperties.Value.SmbProperties.FileCreatedOn.Value.LocalDateTime;
                            file.URL = fileClient.Uri.ToString();
                        }
                        fileList.Add(file);
                    }
                }
                finally
                {
                    await enumerator.DisposeAsync();
                }
                return fileList;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Gets the file asynchronous.
        /// The full path should be like {ShareName}/{Directory}/{FileName}.{ext}
        /// </summary>
        /// <param name="shareFullPath">The share folder full path.</param>
        /// <returns></returns>
        /// <exception cref="Exception">Share: {shareName} doesn't exist</exception>
        public async Task<List<FileShareItemInfo>> GetShareFilesAsync(string shareFullPath)
        {
            try
            {
                BaseHelper.SplitStorageFullPath(shareFullPath, out string shareName, out string shareRelativePath);
                return await this.GetShareFilesAsync(shareName, shareRelativePath);
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Gets the share file URL asynchronous.
        /// </summary>
        /// <param name="shareFullPath">The share file full path.</param>
        /// <returns></returns>
        public async Task<string> GetShareFileURLAsync(string shareFullPath)
        {
            try
            {
                BaseHelper.SplitStorageFullPath(shareFullPath, out string shareName, out string shareRelativePath);
                BaseHelper.SplitStorageRelativePath(shareRelativePath, out string dirPath, out string fileName);

                var share = await this.GetShareClient(shareName);
                var shareDir = share.GetDirectoryClient(dirPath);
                if (await shareDir.ExistsAsync() == false)
                    return string.Empty;

                var shareFile = shareDir.GetFileClient(fileName);
                if (await shareFile.ExistsAsync() == false)
                    return string.Empty;

                return shareFile.Uri.ToString();
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Downloads the share file asynchronous.
        /// </summary>
        /// <param name="shareName">Name of the share.</param>
        /// <param name="shareRelativePath">The file path.</param>
        /// <returns></returns>
        /// <exception cref="Exception">
        /// Directory: {dirPath} doesn't exist
        /// or
        /// File: {filePath} doesn't exist
        /// </exception>
        internal async Task<MemoryStream> DownloadShareFileAsync(string shareName, string shareRelativePath)
        {
            try
            {
                BaseHelper.SplitStorageRelativePath(shareRelativePath, out string dirPath, out string fileName);
                var share = await this.GetShareClient(shareName);

                var shareDir = share.GetDirectoryClient(dirPath);
                await this.ExistsSourceAsync(shareDir);

                var shareFile = shareDir.GetFileClient(fileName);
                await this.ExistsSourceAsync(shareFile);

                var shareProperties = await shareFile.GetPropertiesAsync();
                if (shareProperties.Value.ContentLength > Int32.MaxValue)
                {
                    throw new Exception($"The file size exceeds 2GB,{shareFile.Uri}");
                }

                var download = await shareFile.DownloadAsync();
                var memoryStream = new MemoryStream(Convert.ToInt32(shareProperties.Value.ContentLength));
                await download.Value.Content.CopyToAsync(memoryStream, BaseHelper.BufferSize);
                memoryStream.Seek(0, SeekOrigin.Begin);
                return memoryStream;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Downloads the share file asynchronous.
        /// The full path should be like {ShareName}/{Directory}/{FileName}.{ext}
        /// </summary>
        /// <param name="shareFullPath">Full name of the share file.</param>
        /// <returns></returns>
        /// <exception cref="Exception">
        /// Share: {shareName} doesn't exist
        /// or
        /// Directory: {fileDirectory} doesn't exist
        /// or
        /// File: {fileName} doesn't exist
        /// </exception>
        public async Task<MemoryStream> DownloadShareFileAsync(string shareFullPath)
        {
            try
            {
                // Share
                BaseHelper.SplitStorageFullPath(shareFullPath, out string shareName, out string shareRelativePath);
                return await this.DownloadShareFileAsync(shareName, shareRelativePath);
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Downloads the share file by URL asynchronous.
        /// </summary>
        /// <param name="fileURL">The file URL.</param>
        /// <returns></returns>
        public async Task<MemoryStream> DownloadShareFileByURLAsync(string fileURL)
        {
            try
            {
                var service = new ShareServiceClient(this._storageConnectionString);

                //SAS Credential
                var sasURL = service.GenerateAccountSasUri(AccountSasPermissions.Read, DateTimeOffset.Now.AddDays(1), AccountSasResourceTypes.All);
                var sasCredential = new AzureSasCredential(sasURL.Query);

                //File Client
                var shareFile = new ShareFileClient(new Uri(fileURL), sasCredential);
                await this.ExistsSourceAsync(shareFile);

                var shareProperties= await shareFile.GetPropertiesAsync();
                if (shareProperties.Value.ContentLength > Int32.MaxValue)
                {
                    throw new Exception($"The file size exceeds 2GB,{shareFile.Uri}");
                }

                var download = await shareFile.DownloadAsync();
                var memoryStream = new MemoryStream(Convert.ToInt32(shareProperties.Value.ContentLength));
                await download.Value.Content.CopyToAsync(memoryStream, BaseHelper.BufferSize);
                memoryStream.Seek(0, SeekOrigin.Begin);
                return memoryStream;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }
            
        #endregion

        #region Upoad FileShare Methods
        /// <summary>
        /// Creates the share file folder asynchronous.
        /// </summary>
        /// <param name="shareName">Name of the share.</param>
        /// <param name="shareDirName">The share relative path.</param>
        /// <returns></returns>
        /// <exception cref="Exception">Share doesn't exist</exception>
        internal async Task CreateShareDirectoryAsync(string shareName, string shareDirName)
        {
            try
            {
                var share = await this.GetShareClient(shareName);
                await this.CreateShareDirectoryAsync(share, shareDirName);
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Creates the share directory asynchronous.
        /// The directory full path should be like {ShareName}/{Directory}
        /// </summary>
        /// <param name="shareFolderFullPath">The directory full path.</param>
        /// <returns></returns>
        /// <exception cref="Exception">Share: {shareName} doesn't exist</exception>
        public async Task CreateShareDirectoryAsync(string shareFolderFullPath)
        {
            try
            {
                // Source Share
                BaseHelper.SplitStorageFullPath(shareFolderFullPath, out string shareName, out string shareRelativePath);
                await this.CreateShareDirectoryAsync(shareName, shareRelativePath);
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Opens the write share file asynchronous.
        /// </summary>
        /// <param name="shareFullPath">The file full path.</param>
        /// <param name="fileSize">Size of the file.</param>
        /// <param name="WriteContent">Content of the write.</param>
        public async Task OpenWriteShareFileAsync(string shareFullPath, long fileSize, Action<Stream> WriteContent)
        {
            try
            {
                BaseHelper.SplitStorageFullPath(shareFullPath, out string shareName, out string filePath);
                BaseHelper.SplitStorageRelativePath(filePath, out string shareDirName, out string fileName);
                var share = await this.GetShareClient(shareName);
                var shareDir = await this.CreateShareDirectoryAsync(share, shareDirName);

                // File
                var shareFile = shareDir.GetFileClient(fileName);
                await shareFile.DeleteIfExistsAsync();

                var httpHeaders = this.GetFleShareHeader(fileName);
                await shareFile.CreateAsync(fileSize, httpHeaders);
                using (var stream = await shareFile.OpenWriteAsync(false, 0))
                {
                    try
                    {
                        WriteContent(stream);
                    }
                    catch (Exception exContent)
                    {
                        await shareFile.DeleteIfExistsAsync();
                        throw exContent;
                    }
                }
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Uploads the share file asynchronous.
        /// </summary>
        /// <param name="shareName">Name of the share.</param>
        /// <param name="shareRelativePath">The file path.</param>
        /// <param name="stream">The stream.</param>
        /// <returns></returns>
        /// <exception cref="Exception">Directory: {dirPath} doesn't exist</exception>
        internal async Task UploadShareFileAsync(string shareName, string shareRelativePath, Stream stream)
        {
            try
            {
                BaseHelper.SplitStorageRelativePath(shareRelativePath, out string shareDirName, out string fileName);
                var share = await this.GetShareClient(shareName);
                var shareDir = await this.CreateShareDirectoryAsync(share, shareDirName);

                // File
                var shareFile = shareDir.GetFileClient(fileName);
                stream.Seek(0, SeekOrigin.Begin);

                var httpHeaders = this.GetFleShareHeader(fileName);
                await shareFile.CreateAsync(stream.Length, httpHeaders);
                await shareFile.UploadAsync(stream);
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Uploads the share file asynchronous.
        /// </summary>
        /// <param name="shareFullPath">The file full path.</param>
        /// <param name="stream">The stream.</param>
        /// <returns></returns>
        public async Task UploadShareFileAsync(string shareFullPath, Stream stream)
        {
            try
            {
                BaseHelper.SplitStorageFullPath(shareFullPath, out string shareName, out string shareRelativePath);
                await this.UploadShareFileAsync(shareName, shareRelativePath, stream);
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }
        #endregion

        #region Delete FileShare Methods
        /// <summary>
        /// Deletes the share file asynchronous.
        /// </summary>
        /// <param name="shareName">Name of the share.</param>
        /// <param name="shareRelativePath">The file relative path.</param>
        /// <returns></returns>
        /// <exception cref="Exception">Directory: {dirPath} doesn't exist</exception>
        internal async Task DeleteShareFileAsync(string shareName, string shareRelativePath)
        {
            try
            {
                BaseHelper.SplitStorageRelativePath(shareRelativePath, out string dirPath, out string fileName);
                var share = await this.GetShareClient(shareName);
               
                var shareDir = share.GetDirectoryClient(dirPath);
                await this.ExistsSourceAsync(shareDir);
              
                // File
                var shareFile = shareDir.GetFileClient(fileName);
                await shareFile.DeleteIfExistsAsync();
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Deletes the file asynchronous.
        /// The full path should be like {ShareName}/{Directory}/{FileName}.{ext}
        /// </summary>
        /// <param name="shareFullPath">Full name of the file share.</param>
        /// <returns></returns>
        /// <exception cref="Exception">Share: {shareName} doesn't exist</exception>
        public async Task DeleteShareFileAsync(string shareFullPath)
        {
            try
            {
                BaseHelper.SplitStorageFullPath(shareFullPath, out string shareName, out string shareRelativePath);
                await this.DeleteShareFileAsync(shareName, shareRelativePath);
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Deletes the share file folder asynchronous.
        /// </summary>
        /// <param name="shareName">Name of the share.</param>
        /// <param name="shareRelativePath">The share relative path.</param>
        /// <returns></returns>
        /// <exception cref="Exception">Share doesn't exist</exception>
        public async Task DeleteShareFolderAsync(string shareName, string shareRelativePath)
        {
            try
            {
                var share = await this.GetShareClient(shareName);

                var shareDir = share.GetDirectoryClient(shareRelativePath);
                if (await shareDir.ExistsAsync() == false)
                    return;

                var itemList = shareDir.GetFilesAndDirectories();
                foreach (var item in itemList)
                {
                    if (item.IsDirectory)
                    {
                        var sharePath = BaseHelper.CombineStoragePath(shareRelativePath, item.Name);
                        await this.DeleteShareFolderAsync(shareName, sharePath);
                        continue;
                    }
                    await shareDir.GetFileClient(item.Name).DeleteIfExistsAsync();
                }
                await shareDir.DeleteIfExistsAsync();
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Deletes the share file folder asynchronous.
        /// </summary>
        /// <param name="shareFullPath">The folder full path.</param>
        /// <returns></returns>
        /// <exception cref="Exception">Directory: {dirPath} doesn't exist</exception>
        public async Task DeleteShareFolderAsync(string shareFullPath)
        {
            try
            {
                BaseHelper.SplitStorageFullPath(shareFullPath, out string shareName, out string shareRelativePath);
                await this.DeleteShareFolderAsync(shareName, shareRelativePath);
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }
        #endregion

        #region Other FileShare Methods

        /// <summary>
        /// Copies the share file asynchronous.
        /// </summary>
        /// <param name="sourceShareName">Name of the source share.</param>
        /// <param name="sourceRelativePath">The source relative path.</param>
        /// <param name="destShareName">Name of the dest share.</param>
        /// <param name="destRelativePath">The dest relative path.</param>
        /// <param name="deleteSource">if set to <c>true</c> [delete source].</param>
        /// <exception cref="System.Exception">The file copy failed, {copyStatusMsg}, ({sourceRelativePath} to {destRelativePath})</exception>
        internal async Task CopyShareFileAsync(string sourceShareName, string sourceRelativePath, string destShareName, string destRelativePath, bool deleteSource = false)
        {
            try
            {
                BaseHelper.SplitStorageRelativePath(sourceRelativePath, out string sourceDirName, out string sourceFileName);
                BaseHelper.SplitStorageRelativePath(destRelativePath, out string destDirName, out string destFileName);

                // Source Share
                var sourceShare = await this.GetShareClient(sourceShareName);

                // Source Directory
                var sourceShareDir = sourceShare.GetDirectoryClient(sourceDirName);
                await this.ExistsSourceAsync(sourceShareDir);

                // Source File
                var sourceShareFile = sourceShareDir.GetFileClient(sourceFileName);
                await this.ExistsSourceAsync(sourceShareFile);

                // Destination Share
                var destShare = await this.GetShareClient(destShareName);
                var destShareDir = await this.CreateShareDirectoryAsync(destShare, destDirName);

                // Destination File
                var destShareFile = destShareDir.GetFileClient(destFileName);
                await destShareFile.StartCopyAsync(sourceShareFile.Uri);
                if (this.GetCopyCompletedStatus(destShareFile, out string copyStatusMsg) == false)
                {
                    await destShareFile.DeleteIfExistsAsync();
                    throw new Exception($"The file copy failed, {copyStatusMsg}, ({sourceShareName}/{sourceRelativePath} to {destShareName}/{destRelativePath})");
                }

                if (deleteSource == true)
                {
                    await sourceShareFile.DeleteIfExistsAsync();
                }
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Copies the file asynchronous.
        /// The full path should be like {ShareName}/{Directory}/{FileName}.{ext}
        /// </summary>
        /// <param name="sourceShareFullPath">The source file full path.</param>
        /// <param name="destShareFullPath">The dest file full path.</param>
        /// <param name="deleteSource">if set to <c>true</c> [delete source].</param>
        /// <returns></returns>
        public async Task CopyShareFileAsync(string sourceShareFullPath, string destShareFullPath, bool deleteSource = false)
        {
            try
            {
                BaseHelper.SplitStorageFullPath(sourceShareFullPath, out string sourceShareName, out string sourceRelativePath);
                BaseHelper.SplitStorageFullPath(destShareFullPath, out string destShareName, out string destRelativePath);
                await this.CopyShareFileAsync(sourceShareName, sourceRelativePath, destShareName, destRelativePath, deleteSource);
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Compresses the share file to zip asynchronous.
        /// The file full path should be like {ShareName}/{Directory}/{FileName}.{ext}
        /// </summary>
        /// <param name="sourceShareFullPath">The source share file full path.</param>
        /// <param name="destShareFullPath">The dest share file full path.</param>
        /// <param name="deleteSource">if set to <c>true</c> [delete source].</param>
        /// <returns></returns>
        public async Task CompressShareFileToZipAsync(string sourceShareFullPath, string destShareFullPath, bool deleteSource = false)
        {
            try
            {
                BaseHelper.SplitStorageFullPath(sourceShareFullPath, out string sourceShareName, out string sourceRelativePath);
                BaseHelper.SplitStorageFullPath(destShareFullPath, out string destShareName, out string destRelativePath);
                BaseHelper.SplitStorageRelativePath(sourceRelativePath, out string sourceDirName, out string sourceFileName);
                BaseHelper.SplitStorageRelativePath(destRelativePath, out string destDirName, out string destFileName);

                // Source Share
                var sourceShare = await this.GetShareClient(sourceShareName);

                // Source Directory
                var sourceShareDir = sourceShare.GetDirectoryClient(sourceDirName);
                await this.ExistsSourceAsync(sourceShareDir);

                // Source File
                var sourceShareFile = sourceShareDir.GetFileClient(sourceFileName);
                await this.ExistsSourceAsync(sourceShareFile);

                // Dest Share
                var destShare = await this.GetShareClient(destShareName);
                var destShareDir = await this.CreateShareDirectoryAsync(destShare, destDirName);

                // Download
                using (var streamZip = new MemoryStream())
                {
                    // Update mode with cost too much memory, use Create to instead
                    using (var zipHelper = new ZipHelper(streamZip, ZipArchiveMode.Create))
                    {
                        // Update mode with cost too much memory, use Create to instead
                        var download = await sourceShareFile.DownloadAsync();
                        zipHelper.AddContent(sourceFileName, download.Value.Content);
                    }

                    // Destination File
                    var destShareFile = destShareDir.GetFileClient(destFileName);
                    streamZip.Seek(0, SeekOrigin.Begin);

                    var httpHeaders = this.GetFleShareHeader(destFileName);
                    await destShareFile.CreateAsync(streamZip.Length, httpHeaders);
                    await destShareFile.UploadAsync(streamZip);
                }

                if (deleteSource == true)
                {
                    await sourceShareFile.DeleteIfExistsAsync();
                }
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Copies the share file to BLOB asynchronous.
        /// The full path should be like {ContainerName}/{Directory}/{FileName}.{ext}
        /// </summary>
        /// <param name="sourceShareFullPath">The source share file full path.</param>
        /// <param name="destBlobFullPath">The dest BLOB full path.</param>
        /// <returns></returns>
        /// <exception cref="Exception">
        /// Container: {destContainerName} doesn't exist
        /// or
        /// Share: {sourceShareName} doesn't exist
        /// </exception>
        public async Task CopyShareFileToBlobAsync(string sourceShareFullPath, string destBlobFullPath)
        {
            try
            {
                BaseHelper.SplitStorageFullPath(sourceShareFullPath, out string sourceShareName, out string sourceShareRelativePath);
                BaseHelper.SplitStorageFullPath(destBlobFullPath, out string destContainerName, out string destBlobName);
                BaseHelper.SplitStorageRelativePath(sourceShareRelativePath, out string sourceDirName, out string sourceFileName);

                // Source file share
                var sourceShare = await this.GetShareClient(sourceShareName);
                await this.ExistsSourceAsync(sourceShare);

                var sourceShareDir = sourceShare.GetDirectoryClient(sourceDirName);
                await this.ExistsSourceAsync(sourceShareDir);

                var sourceShareFile = sourceShareDir.GetFileClient(sourceFileName);
                await this.ExistsSourceAsync(sourceShareFile);

                // Dest blob
                var container = await this.GetBlobContainerClient(destContainerName);
                var destBlob = container.GetBlobClient(destBlobName);

                // download source file
                var sasURL = sourceShareFile.GenerateSasUri(ShareFileSasPermissions.Read, DateTimeOffset.Now.AddDays(1));
                await destBlob.StartCopyFromUriAsync(sasURL);
                if (this.GetCopyCompletedStatus(destBlob, out string copyStatusMsg) == false)
                {
                    await destBlob.DeleteIfExistsAsync();
                    throw new Exception($"The file copy failed, {copyStatusMsg}, ({sourceShareFullPath} to {destBlobFullPath})");
                }
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }
        #endregion

        #endregion

        #region Private Methods

        /// <summary>
        /// Creates the BLOB container client.
        /// </summary>
        /// <param name="containerName">Name of the container.</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private async Task<BlobContainerClient> GetBlobContainerClient(string containerName)
        {
            var container = new BlobContainerClient(this._storageConnectionString, containerName);
            if (await container.ExistsAsync() == false)
            {
                throw new Exception($"{container.Uri} doesn't exist");
            }
            return container;
        }

        /// <summary>
        /// Creates the BLOB container client.
        /// </summary>
        /// <param name="blobClient">Client of the blob.</param>
        /// <param name="storedPolicyName">Stored policy name.</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private static Uri GetServiceSasUriForBlob(BlobClient blobClient, string storedPolicyName = null)
        {
            // Check whether this BlobClient object has been authorized with Shared Key.
            if (blobClient.CanGenerateSasUri)
            {
                // Create a SAS token that's valid for one hour.
                BlobSasBuilder sasBuilder = new BlobSasBuilder()
                {
                    BlobContainerName = blobClient.GetParentBlobContainerClient().Name,
                    BlobName = blobClient.Name,
                    Resource = "b"
                };

                if (storedPolicyName == null)
                {
                    sasBuilder.ExpiresOn = DateTimeOffset.UtcNow.AddHours(1);
                    sasBuilder.SetPermissions(BlobSasPermissions.Read |
                        BlobSasPermissions.Write);
                }
                else
                {
                    sasBuilder.Identifier = storedPolicyName;
                }

                Uri sasUri = blobClient.GenerateSasUri(sasBuilder);
                Console.WriteLine("SAS URI for blob is: {0}", sasUri);
                Console.WriteLine();

                return sasUri;
            }
            else
            {
                Console.WriteLine(@"BlobClient must be authorized with Shared Key credentials to create a service SAS.");
                return null;
            }
        }

        /// <summary>
        /// Gets the share client.
        /// </summary>
        /// <param name="shareName">Name of the share.</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private async Task<ShareClient> GetShareClient(string shareName)
        {
            var share = new ShareClient(this._storageConnectionString, shareName);
            if (await share.ExistsAsync() == false)
            {
                throw new Exception($"{share.Uri} doesn't exist");
            }
            return share;
        }

        /// <summary>
        /// Checks the source exist.
        /// </summary>
        /// <param name="blob">The client.</param>
        /// <exception cref="Exception"></exception>
        private async Task ExistsSourceAsync(BlobClient blob)
        {
            if (await blob.ExistsAsync() == false)
            {
                throw new Exception($"{blob.Uri} doesn't exist");
            }
        }

        /// <summary>
        /// Checks the source exist asynchronous.
        /// </summary>
        /// <param name="shareDir">The client.</param>
        /// <exception cref="Exception"></exception>
        private async Task ExistsSourceAsync(ShareDirectoryClient shareDir)
        {
            if (await shareDir.ExistsAsync() == false)
            {
                throw new Exception($"{shareDir.Uri} doesn't exist");
            }
        }

        /// <summary>
        /// Checks the source exist asynchronous.
        /// </summary>
        /// <param name="shareFile">The client.</param>
        /// <exception cref="Exception"></exception>
        private async Task ExistsSourceAsync(ShareFileClient shareFile)
        {
            if (await shareFile.ExistsAsync() == false)
            {
                throw new Exception($"{shareFile.Uri} doesn't exist");
            }
        }

        /// <summary>
        /// Checks the source exist asynchronous.
        /// </summary>
        /// <param name="share">The client.</param>
        /// <exception cref="Exception"></exception>
        private async Task ExistsSourceAsync(ShareClient share)
        {
            if (await share.ExistsAsync() == false)
            {
                throw new Exception($"{share.Uri} doesn't exist");
            }
        }

        /// <summary>
        /// Gets the upload options.
        /// </summary>
        /// <param name="blobName">Name of the BLOB.</param>
        /// <returns></returns>
        private BlobModels.BlobUploadOptions GetUploadOptions(string blobName)
        {
            var fileName = BaseHelper.GetFileName(blobName);
            var extName = BaseHelper.GetFileExtension(fileName);
            var contentType = BaseHelper.GetContentTypeByExtension(extName);
            return new BlobModels.BlobUploadOptions()
            {
                TransferOptions = this._transferOptions,
                HttpHeaders = new BlobModels.BlobHttpHeaders() { ContentType = contentType}
            };
        }

        /// <summary>
        /// Gets the block BLOB open write options.
        /// </summary>
        /// <param name="blobName">Name of the BLOB.</param>
        /// <returns></returns>
        private BlobModels.BlockBlobOpenWriteOptions GetBlockBlobOpenWriteOptions(string blobName)
        {
            var fileName = BaseHelper.GetFileName(blobName);
            var extName = BaseHelper.GetFileExtension(fileName);
            var contentType = BaseHelper.GetContentTypeByExtension(extName);
            return new BlobModels.BlockBlobOpenWriteOptions()
            {
                BufferSize = this._transferOptions.MaximumTransferSize,
                HttpHeaders = new BlobModels.BlobHttpHeaders() { ContentType = contentType }
            };
        }

        /// <summary>
        /// Gets the commit block list options.
        /// </summary>
        /// <param name="blobName">Name of the BLOB.</param>
        /// <returns></returns>
        private BlobModels.CommitBlockListOptions GetCommitBlockListOptions(string blobName)
        {
            var fileName = BaseHelper.GetFileName(blobName);
            var extName = BaseHelper.GetFileExtension(fileName);
            var contentType = BaseHelper.GetContentTypeByExtension(extName);
            return new BlobModels.CommitBlockListOptions()
            {
                HttpHeaders = new BlobModels.BlobHttpHeaders() { ContentType = contentType },
            };
        }

        /// <summary>
        /// Gets the fle share header.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        private ShareModels.ShareFileHttpHeaders GetFleShareHeader(string fileName)
        {
            var extName = BaseHelper.GetFileExtension(fileName);
            var contentType = BaseHelper.GetContentTypeByExtension(extName);
            return new ShareModels.ShareFileHttpHeaders() { ContentType = contentType };
        }

        /// <summary>
        /// Creates the share directory asynchronous.
        /// </summary>
        /// <param name="share">The share.</param>
        /// <param name="shareDirName">Name of the share dir.</param>
        private async Task<ShareDirectoryClient> CreateShareDirectoryAsync(ShareClient share, string shareDirName)
        {
            var arrFolderName = BaseHelper.Split(shareDirName, "/");
            var dirName = string.Empty;
            var shareDir = (ShareDirectoryClient)null;
            foreach (var folderName in arrFolderName)
            {
                // Create All Directory
                dirName = BaseHelper.CombineStoragePath(dirName, folderName);
                shareDir = share.GetDirectoryClient(dirName);
                await shareDir.CreateIfNotExistsAsync();
            }
            return shareDir;
        }

        /// <summary>
        /// Waits the copy completed asynchronous.
        /// </summary>
        /// <param name="shareFile">The share file.</param>
        /// <param name="copyStatusMsg">The copy status MSG.</param>
        /// <returns></returns>
        private bool GetCopyCompletedStatus(ShareFileClient shareFile, out string copyStatusMsg)
        {
            var properties = (Response<ShareModels.ShareFileProperties>)null;
            Task.Run(async () =>
             {
                 properties = await shareFile.GetPropertiesAsync();
                 while (properties.Value.CopyStatus == ShareModels.CopyStatus.Pending)
                 {
                     await Task.Delay(500);
                     properties = await shareFile.GetPropertiesAsync();
                 }
             }).Wait();

            copyStatusMsg = properties.Value.CopyStatusDescription;
            return properties.Value.CopyStatus == ShareModels.CopyStatus.Success;
        }

        /// <summary>
        /// Gets the copy completed status.
        /// </summary>
        /// <param name="blobClient">The BLOB client.</param>
        /// <param name="copyStatusMsg">The copy status MSG.</param>
        /// <returns></returns>
        private bool GetCopyCompletedStatus(BlobClient blobClient, out string copyStatusMsg)
        {
            var properties = (Response<BlobModels.BlobProperties>)null;
            Task.Run(async () =>
            {
                properties = await blobClient.GetPropertiesAsync();
                while (properties.Value.CopyStatus == BlobModels.CopyStatus.Pending)
                {
                    await Task.Delay(500);
                    properties = await blobClient.GetPropertiesAsync();
                }
            }).Wait();
            copyStatusMsg = properties.Value.CopyStatusDescription;
            return properties.Value.CopyStatus == BlobModels.CopyStatus.Success;
        }
        #endregion
    }
}
