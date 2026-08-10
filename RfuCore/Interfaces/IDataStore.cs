using ResumableFileTransfer.Common;
using ResumableFileTransfer.RfuCore.Models;
using System.IO;
using System.Threading.Tasks;

namespace ResumableFileTransfer.RfuCore.Interfaces
{
    public interface IDataStore
    {
        public Task<BlobItemInfo> GetAsync(string fileFullPath);
        public Task<bool> ExistsAsync(string fileFullPath);
        public Task<MemoryStream> ReadAsync(string fileFullPath);
        public Task<RfuFile> CreateAsync(RfuFile file);
        public Task<bool> DeleteAsync(RfuFile file);
        public Task<bool> MoveToMalwareAsync(RfuFile file);
        public Task<long> WriteAsync(RfuFile file);
        public Task<long> GetOffsetAsync(RfuFile file);
        public Task<bool> CompleteAsync(RfuFile file);
        public Task<RfuFileMetadata> ReadMetadataAsync(RfuFile file);
        public Task<RfuFileStatus> GetStatusAsync(RfuFile file);
    }
}
