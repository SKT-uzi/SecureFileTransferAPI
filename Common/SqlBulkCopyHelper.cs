using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System.Transactions;

namespace ResumableFileTransfer.Common
{
    public class SqlBulkCopyHelper
    {
        private static readonly string _clsFullName = typeof(SqlBulkCopyHelper).FullName;

        public static void WriteToServer(
        IDataReader reader,
        string connectionString,
        SqlBulkCopySetting sqlBulkCopySetting,
        int timeoutSeconds,
        ILogHelper log)
        {
            using (TransactionScope scope = new TransactionScope(
                       TransactionScopeOption.RequiresNew,
                       TimeSpan.FromSeconds(timeoutSeconds)))
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection))
                    {
                        bulkCopy.BulkCopyTimeout = timeoutSeconds;

                        SetColumnMappings(bulkCopy, sqlBulkCopySetting);

                        try
                        {
                            bulkCopy.WriteToServer(reader);
                        }
                        catch (Exception ex)
                        {
                            log.WriteTextLog($"Bulk copy failed: {ex.Message}");
                            throw BaseHelper.CreateException(_clsFullName, ex);
                        }
                        finally
                        {
                            reader.Close();
                        }
                    }
                }

                scope.Complete();
            }
        }

        public static async Task WriteToServerAsync(IDataReader reader, SqlBulkCopy bulkCopy, ILogHelper log)
        {
            try
            {
                await bulkCopy.WriteToServerAsync(reader);
            }
            catch (Exception ex)
            {
                log.WriteTextLog($"Bulk copy failed: {ex.Message}");
                throw BaseHelper.CreateException(_clsFullName, ex);
            }
            finally
            {
                reader.Close();
            }
        }

        public static async Task WriteToServerAsync(
        IDataReader reader,
        string connectionString,
        SqlBulkCopySetting sqlBulkCopySetting,
        int timeoutSeconds,
        ILogHelper log)
        {
            using (TransactionScope scope = new TransactionScope(
                       TransactionScopeOption.RequiresNew,
                       TimeSpan.FromSeconds(timeoutSeconds),
                       TransactionScopeAsyncFlowOption.Enabled))
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection))
                    {
                        bulkCopy.BulkCopyTimeout = timeoutSeconds;
                        bulkCopy.EnableStreaming = true;
                        bulkCopy.BatchSize = 10000;

                        SetColumnMappings(bulkCopy, sqlBulkCopySetting);

                        try
                        {
                            await bulkCopy.WriteToServerAsync(reader);
                        }
                        catch (Exception ex)
                        {
                            log.WriteTextLog($"Bulk copy failed: {ex.Message}");
                            throw BaseHelper.CreateException(_clsFullName, ex);
                        }
                        finally
                        {
                            reader.Close();
                        }
                    }
                }
                scope.Complete();
            }
        }

        public static async Task WriteToServerAsync(
        DataTable dt,
        string connectionString,
        SqlBulkCopySetting sqlBulkCopySetting,
        int timeoutSeconds,
        ILogHelper log)
        {
            using (TransactionScope scope = new TransactionScope(
                       TransactionScopeOption.RequiresNew,
                       TimeSpan.FromSeconds(timeoutSeconds),
                       TransactionScopeAsyncFlowOption.Enabled))
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection))
                    {
                        bulkCopy.BulkCopyTimeout = timeoutSeconds;
                        bulkCopy.EnableStreaming = true;

                        SetColumnMappings(bulkCopy, sqlBulkCopySetting);

                        try
                        {
                            await bulkCopy.WriteToServerAsync(dt);
                        }
                        catch (Exception ex)
                        {
                            log.WriteTextLog($"Bulk copy failed: {ex.Message}");
                            throw BaseHelper.CreateException(_clsFullName, ex);
                        }
                        finally
                        {
                            dt.Dispose();
                        }
                    }
                }
                scope.Complete();
            }
        }

        public static async Task WriteToServerAsync(
        List<DataTable> dts,
        string connectionString,
        SqlBulkCopySetting sqlBulkCopySetting,
        int timeoutSeconds,
        ILogHelper log)
        {
            using (TransactionScope scope = new TransactionScope(
                       TransactionScopeOption.RequiresNew,
                       TimeSpan.FromSeconds(timeoutSeconds),
                       TransactionScopeAsyncFlowOption.Enabled))
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection))
                    {
                        bulkCopy.BulkCopyTimeout = timeoutSeconds;
                        bulkCopy.EnableStreaming = true;

                        SetColumnMappings(bulkCopy, sqlBulkCopySetting);

                        try
                        {
                            foreach (var dt in dts)
                            {
                                await bulkCopy.WriteToServerAsync(dt);
                            }                                
                        }
                        catch (Exception ex)
                        {
                            log.WriteTextLog($"Bulk copy failed: {ex.Message}");
                            throw BaseHelper.CreateException(_clsFullName, ex);
                        }
                        finally
                        {
                            foreach (var dt in dts)
                            {
                                if (dt != null)
                                    dt.Dispose();
                            }
                        }
                    }
                }
                scope.Complete();
            }
        }

        public static void SetColumnMappings(SqlBulkCopy bulkCopy, SqlBulkCopySetting sqlBulkCopySetting)
        {
            bulkCopy.DestinationTableName = sqlBulkCopySetting.DestinationTable;
            foreach (var mapping in sqlBulkCopySetting.ColumnMappings)
            {
                bulkCopy.ColumnMappings.Add(mapping.Key, mapping.Value);
            }
        }
    }

    public class SqlBulkCopySetting
    {
        public string DestinationTable { get; set; }
        public Dictionary<string, string> ColumnMappings { get; set; }
    }
}
