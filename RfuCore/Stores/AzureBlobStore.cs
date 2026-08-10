using ResumableFileTransfer.RfuCore.Models;
using System.Threading.Tasks;

namespace ResumableFileTransfer.RfuCore.Stores
{
    public class AzureBlobStore : BaseDataStore
    {
        public AzureBlobStore()
        {
        }

        public AzureBlobStore(string runtimefilesStorageConnectionString)
            : base(runtimefilesStorageConnectionString)
        {
        }

        public override async Task<bool> CompleteAsync(RfuFile file)
        {
            if (file.Metadata == null) throw new System.Exception("RfuFile.Metadata is required before moving to target folder");

            var helper = CreateStorageHelper();

            await retryPolicy.ExecuteAsync(async () =>
            {
                if (await helper.ExistsBlobAsync(file.UploadFullPath))
                {
                    await helper.CopyBlobAsync(file.UploadFullPath, file.TargetFullPath, true);
                }

                if (await helper.ExistsBlobAsync(GetMetadataFullPath(file)))
                {
                    await helper.DeleteBlobAsync(GetMetadataFullPath(file));
                }
            });

            return true;
        }
    }
}
