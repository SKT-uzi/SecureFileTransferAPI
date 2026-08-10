using ResumableFileTransfer.Entity;

namespace ResumableFileTransfer.API.Interfaces
{
    public interface IDBContext
    {
        public SoftwareTokenValidateStatus CheckSoftwareTokenStatus(string softwareToken);
        public AccountSoftwareToken GetTokenInfo(string softwareToken);
        public void SaveDataTransferLog(DataTransferLog log);
    }
}
