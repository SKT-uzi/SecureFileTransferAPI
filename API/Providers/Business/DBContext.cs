using ResumableFileTransfer.API.Interfaces;
using ResumableFileTransfer.API.Models;
using ResumableFileTransfer.Entity;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace ResumableFileTransfer.API.Providers
{
    public class DBContext : IDBContext
    {
        private readonly ResumableFileTransferDbContext _dbContext;

        public DBContext(ResumableFileTransferDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public SoftwareTokenValidateStatus CheckSoftwareTokenStatus(string softwareToken)
        {
            var tokenInfo = GetTokenInfo(softwareToken);

            if (tokenInfo == null || string.IsNullOrEmpty(tokenInfo.Token))
            {
                return SoftwareTokenValidateStatus.NotExists;
            }
            else if (tokenInfo.Status == SoftwareTokenStatus.Inactive.GetHashCode())
            {
                return SoftwareTokenValidateStatus.Disabled;
            }

            return SoftwareTokenValidateStatus.Valid;
        }

        public AccountSoftwareToken GetTokenInfo(string softwareToken)
        {
            return _dbContext.AccountSoftwareTokens
                .AsNoTracking()
                .FirstOrDefault(x => x.Token == softwareToken) ?? new AccountSoftwareToken();
        }

        public void SaveDataTransferLog(DataTransferLog log)
        {
            log.CreatedAtUtc = log.CreatedAtUtc == default ? System.DateTime.UtcNow : log.CreatedAtUtc;
            _dbContext.DataTransferLogs.Add(log);
            _dbContext.SaveChanges();
        }
    }
}
