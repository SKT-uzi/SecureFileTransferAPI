using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;

namespace ResumableFileTransfer.Entity
{
    public static class ResumableFileTransferDbContextFactory
    {
        public static ResumableFileTransferDbContext Create(string connectionStringOrPath)
        {
            var provider = DetectProvider(connectionStringOrPath);
            var optionsBuilder = new DbContextOptionsBuilder<ResumableFileTransferDbContext>();

            if (provider == ResumableFileTransferDatabaseProvider.Sqlite)
            {
                var sqliteConnectionString = NormalizeSqliteConnectionString(connectionStringOrPath);
                EnsureSqliteDatabaseDirectory(sqliteConnectionString);
                optionsBuilder.UseSqlite(sqliteConnectionString);
            }
            else
            {
                optionsBuilder.UseSqlServer(connectionStringOrPath);
            }

            return new ResumableFileTransferDbContext(optionsBuilder.Options);
        }

        public static ResumableFileTransferDatabaseProvider DetectProvider(string connectionStringOrPath)
        {
            if (string.IsNullOrWhiteSpace(connectionStringOrPath))
            {
                throw new InvalidOperationException("Database connection string is required.");
            }

            if (connectionStringOrPath.Contains("=") == false)
            {
                return ResumableFileTransferDatabaseProvider.Sqlite;
            }

            var normalized = connectionStringOrPath.Trim().ToLowerInvariant();
            var looksLikeSqlServer =
                normalized.Contains("initial catalog=") ||
                normalized.Contains("database=") ||
                normalized.Contains("server=") ||
                normalized.Contains("user id=") ||
                normalized.Contains("password=") ||
                normalized.Contains("integrated security=") ||
                normalized.Contains("trusted_connection=") ||
                normalized.Contains("trustservercertificate=") ||
                normalized.Contains("attachdbfilename=");

            return looksLikeSqlServer ? ResumableFileTransferDatabaseProvider.SqlServer : ResumableFileTransferDatabaseProvider.Sqlite;
        }

        private static string NormalizeSqliteConnectionString(string connectionStringOrPath)
        {
            if (connectionStringOrPath.Contains("="))
            {
                return connectionStringOrPath;
            }

            return new SqliteConnectionStringBuilder
            {
                DataSource = connectionStringOrPath
            }.ToString();
        }

        private static void EnsureSqliteDatabaseDirectory(string connectionString)
        {
            var builder = new SqliteConnectionStringBuilder(connectionString);
            if (string.IsNullOrWhiteSpace(builder.DataSource) || builder.DataSource == ":memory:")
            {
                return;
            }

            var fullPath = Path.GetFullPath(builder.DataSource);
            var directory = Path.GetDirectoryName(fullPath);
            if (string.IsNullOrWhiteSpace(directory) == false)
            {
                Directory.CreateDirectory(directory);
            }
        }
    }
}
