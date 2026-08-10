using Microsoft.EntityFrameworkCore;

namespace ResumableFileTransfer.Entity
{
    public class ResumableFileTransferDbContext : DbContext
    {
        public ResumableFileTransferDbContext(DbContextOptions<ResumableFileTransferDbContext> options) : base(options)
        {
        }

        public DbSet<AccountSoftwareToken> AccountSoftwareTokens { get; set; }
        public DbSet<DataTransferLog> DataTransferLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AccountSoftwareToken>(entity =>
            {
                entity.ToTable("AccountSoftwareToken");
                entity.HasKey(x => x.TokenID);
                entity.HasIndex(x => x.Token).IsUnique();

                entity.Property(x => x.TokenID).ValueGeneratedNever();
                entity.Property(x => x.AccountID).IsRequired();
                entity.Property(x => x.AccountName).IsRequired();
                entity.Property(x => x.Token).IsRequired();
                entity.Property(x => x.Status).IsRequired();
                entity.Property(x => x.CreateBy).IsRequired();

                entity.HasData(
                    new AccountSoftwareToken { TokenID = 1, AccountID = 1001, AccountName = "Demo Account A", PhoneNumber = "0000000001", DataTransferType = "API", Token = "00000000-0000-0000-0000-000000000001", DataType = "DEMO", FileFormat = "XML", TargetFileLocation = "demo-account-a", Status = 1, CreateBy = 0, ChangeBy = null },
                    new AccountSoftwareToken { TokenID = 2, AccountID = 1002, AccountName = "Demo Account B", PhoneNumber = "0000000002", DataTransferType = "API", Token = "00000000-0000-0000-0000-000000000002", DataType = "DEMO", FileFormat = "XML", TargetFileLocation = "demo-account-b", Status = 1, CreateBy = 0, ChangeBy = null },
                    new AccountSoftwareToken { TokenID = 3, AccountID = 1003, AccountName = "Disabled Demo Account", PhoneNumber = "0000000003", DataTransferType = "API", Token = "00000000-0000-0000-0000-000000000003", DataType = "DEMO", FileFormat = "XML", TargetFileLocation = "demo-account-disabled", Status = 0, CreateBy = 0, ChangeBy = null });
            });

            modelBuilder.Entity<DataTransferLog>(entity =>
            {
                entity.ToTable("DataTransferLog");
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Id).ValueGeneratedOnAdd();
                entity.Property(x => x.Action).IsRequired();
                entity.Property(x => x.Status).IsRequired();
                entity.Property(x => x.CreatedAtUtc).IsRequired();
            });
        }
    }
}
