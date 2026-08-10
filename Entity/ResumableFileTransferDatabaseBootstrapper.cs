namespace ResumableFileTransfer.Entity
{
    public static class ResumableFileTransferDatabaseBootstrapper
    {
        public static void Initialize(string connectionStringOrPath)
        {
            if (ResumableFileTransferDbContextFactory.DetectProvider(connectionStringOrPath) != ResumableFileTransferDatabaseProvider.Sqlite)
            {
                return;
            }

            using var dbContext = ResumableFileTransferDbContextFactory.Create(connectionStringOrPath);
            dbContext.Database.EnsureCreated();
        }
    }
}
