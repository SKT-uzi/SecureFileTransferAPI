using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace ResumableFileTransfer.Common
{
    internal class SQLHelper
    {
        #region Public Methods
        /// <summary>
        /// Creates the parameter.
        /// </summary>
        /// <param name="paraName">Name of the para.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static IDataParameter CreateParameter(string paraName, object value)
        {
            return new SqlParameter(paraName, value);
        }

        /// <summary>
        /// Creates the parameter.
        /// </summary>
        /// <param name="paraName">Name of the para.</param>
        /// <param name="dbType">Type of the database.</param>
        /// <returns></returns>
        public static IDataParameter CreateParameter(string paraName, SqlDbType dbType)
        {
            return new SqlParameter(paraName, dbType);
        }

        /// <summary>
        /// Creates the parameter.
        /// </summary>
        /// <param name="paraName">Name of the para.</param>
        /// <param name="dbType">Type of the database.</param>
        /// <param name="size">The size.</param>
        /// <param name="direction">The direction.</param>
        /// <returns></returns>
        public static IDataParameter CreateParameter(string paraName, SqlDbType dbType, int size, ParameterDirection direction)
        {
            return new SqlParameter(paraName, dbType, size) { Direction = direction };
        }

        /// <summary>
        /// Executes the reader.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="commandText">The command text.</param>
        /// <param name="commandParameters">The command parameters.</param>
        /// <param name="timeout">The timeout.</param>
        /// <returns></returns>
        public static DbDataReader ExecuteReader(string connectionString, CommandType commandType, string commandText, List<IDataParameter> commandParameters, int timeout)
        {
            // Create & open a SqlConnection, and dispose of it after we are done
            var commandID = Guid.NewGuid();
            var method = "ExecuteReader";
            TrackingLogHelper.WriteTextLog($"[{commandID}]{method} start, {commandText}");
            var connection = new SqlConnection(connectionString);
            connection.Open();
            // Create a command and prepare it for execution
            using (var command = SQLHelper.CreateCommand(connection, commandType, commandText, timeout))
            {
                SQLHelper.LoadParameter(command, commandParameters);
                try
                {
                    return command.ExecuteReader(CommandBehavior.CloseConnection);
                }
                catch (Exception ex)
                {
                    connection.Dispose();
                    TrackingLogHelper.WriteTextLog($"[{commandID}]{method} ex:{ex.Message},{ex.InnerException?.Message}");
                    throw new Exception(BaseHelper.GetExMessageBody(ex));
                }
                finally
                {
                    TrackingLogHelper.WriteTextLog($"[{commandID}]{method} end");
                }
            }
        }

        /// <summary>
        /// Executes the reader async.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="commandText">The command text.</param>
        /// <param name="commandParameters">The command parameters.</param>
        /// <param name="timeout">The timeout.</param>
        /// <returns></returns>
        public static async Task<DbDataReader> ExecuteReaderAsync(string connectionString, CommandType commandType, string commandText, List<IDataParameter> commandParameters, int timeout)
        {
            var commandID = Guid.NewGuid();
            var method = "ExecuteReaderAsync";
            TrackingLogHelper.WriteTextLog($"[{commandID}]{method} start, {commandText}");

            // Create & open a SqlConnection, and dispose of it after we are done
            var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            // Create a command and prepare it for execution
            using (var command = SQLHelper.CreateCommand(connection, commandType, commandText, timeout))
            {
                SQLHelper.LoadParameter(command, commandParameters);
                try
                {
                    return await command.ExecuteReaderAsync(CommandBehavior.CloseConnection);
                }
                catch (Exception ex)
                {
                    connection.Dispose();
                    await TrackingLogHelper.WriteTextLogAsync($"[{commandID}]{method} ex:{ex.Message},{ex.InnerException?.Message}");
                    throw new Exception(BaseHelper.GetExMessageBody(ex));
                }
                finally
                {
                    await TrackingLogHelper.WriteTextLogAsync($"[{commandID}]{method} end");
                }
            }
        }

        /// <summary>
        /// Executes the non query.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="commandText">The command text.</param>
        /// <param name="commandParameters">The command parameters.</param>
        /// <param name="timeout">The timeout.</param>
        /// <returns></returns>
        public static int ExecuteNonQuery(string connectionString, CommandType commandType, string commandText, List<IDataParameter> commandParameters, int timeout)
        {
            var commandID = Guid.NewGuid();
            var method = "ExecuteNonQuery";
            TrackingLogHelper.WriteTextLog($"[{commandID}]{method} start, {commandText}");

            // Create & open a SqlConnection, and dispose of it after we are done
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                // Create a command and prepare it for execution
                using (var command = SQLHelper.CreateCommand(connection, commandType, commandText, timeout))
                {
                    SQLHelper.LoadParameter(command, commandParameters);
                    try
                    {
                        return command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        TrackingLogHelper.WriteTextLog($"[{commandID}]{method} ex:{ex.Message},{ex.InnerException?.Message}");
                        throw new Exception(BaseHelper.GetExMessageBody(ex));
                    }
                    finally
                    {
                        TrackingLogHelper.WriteTextLog($"[{commandID}]{method} end");
                    }
                }
            }
        }

        /// <summary>
        /// Executes the non query asynchronous.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="commandText">The command text.</param>
        /// <param name="commandParameters">The command parameters.</param>
        /// <param name="timeout">The timeout.</param>
        /// <returns></returns>
        public static async Task<int> ExecuteNonQueryAsync(string connectionString, CommandType commandType, string commandText, List<IDataParameter> commandParameters, int timeout)
        {
            var commandID = Guid.NewGuid();
            var method = "ExecuteNonQueryAsync";
            await TrackingLogHelper.WriteTextLogAsync($"[{commandID}]{method} start, {commandText}");

            // Create & open a SqlConnection, and dispose of it after we are done
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                // Create a command and prepare it for execution
                using (var command = SQLHelper.CreateCommand(connection, commandType, commandText, timeout))
                {
                    SQLHelper.LoadParameter(command, commandParameters);
                    try
                    {
                        return await command.ExecuteNonQueryAsync();
                    }
                    catch (Exception ex)
                    {
                        await TrackingLogHelper.WriteTextLogAsync($"[{commandID}]{method} ex:{ex.Message},{ex.InnerException?.Message}");
                        throw new Exception(BaseHelper.GetExMessageBody(ex));
                    }
                    finally
                    {
                        await TrackingLogHelper.WriteTextLogAsync($"[{commandID}]{method} end");
                    }
                }
            }
        }

        /// <summary>
        /// Executes the data table.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="commandText">The command text.</param>
        /// <param name="commandParameters">The command parameters.</param>
        /// <param name="timeout">The timeout.</param>
        /// <returns></returns>
        public static DataTable ExecuteDataTable(string connectionString, CommandType commandType, string commandText, List<IDataParameter> commandParameters, int timeout)
        {
            var commandID = Guid.NewGuid();
            var method = "ExecuteDataTable";
            TrackingLogHelper.WriteTextLog($"[{commandID}]{method} start, {commandText}");

            // Create & open a SqlConnection, and dispose of it after we are done
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                // Create a command and prepare it for execution
                using (var command = SQLHelper.CreateCommand(connection, commandType, commandText, timeout))
                {
                    SQLHelper.LoadParameter(command, commandParameters);

                    // Create the DataAdapter & DataSet
                    using (var da = new SqlDataAdapter(command))
                    {
                        try
                        {
                            DataSet ds = new DataSet();
                            da.Fill(ds);
                            // Return the datatable
                            if (ds != null && ds.Tables.Count > 0)
                                return ds.Tables[0];

                            return new DataTable();
                        }
                        catch (Exception ex)
                        {
                            TrackingLogHelper.WriteTextLog($"[{commandID}]{method} ex:{ex.Message},{ex.InnerException?.Message}");
                            throw new Exception(BaseHelper.GetExMessageBody(ex));
                        }
                        finally
                        {
                            TrackingLogHelper.WriteTextLog($"[{commandID}]{method} end");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Executes the data set.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="commandText">The command text.</param>
        /// <param name="commandParameters">The command parameters.</param>
        /// <param name="timeout">The timeout.</param>
        /// <returns></returns>
        public static DataSet ExecuteDataSet(string connectionString, CommandType commandType, string commandText, List<IDataParameter> commandParameters, int timeout)
        {
            var commandID = Guid.NewGuid();
            var method = "ExecuteDataSet";
            TrackingLogHelper.WriteTextLog($"[{commandID}]{method} start, {commandText}");

            // Create & open a SqlConnection, and dispose of it after we are done
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Create a command and prepare it for execution
                using (var command = SQLHelper.CreateCommand(connection, commandType, commandText, timeout))
                {
                    SQLHelper.LoadParameter(command, commandParameters);

                    // Create the DataAdapter & DataSet
                    using (var da = new SqlDataAdapter(command))
                    {
                        try
                        {
                            DataSet ds = new DataSet();
                            da.Fill(ds);
                            return ds;
                        }
                        catch (Exception ex)
                        {
                            TrackingLogHelper.WriteTextLog($"[{commandID}]{method} ex:{ex.Message},{ex.InnerException?.Message}");
                            throw new Exception(BaseHelper.GetExMessageBody(ex));
                        }
                        finally
                        {
                            TrackingLogHelper.WriteTextLog($"[{commandID}]{method} end");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Executes the scalar.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="commandText">The command text.</param>
        /// <param name="commandParameters">The command parameters.</param>
        /// <param name="timeout">The timeout.</param>
        /// <returns></returns>
        public static object ExecuteScalar(string connectionString, CommandType commandType, string commandText, List<IDataParameter> commandParameters, int timeout)
        {
            var commandID = Guid.NewGuid();
            var method = "ExecuteScalar";
            TrackingLogHelper.WriteTextLog($"[{commandID}]{method} start, {commandText}");

            // Create & open a SqlConnection, and dispose of it after we are done
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Create a command and prepare it for execution
                using (var command = SQLHelper.CreateCommand(connection, commandType, commandText, timeout))
                {
                    SQLHelper.LoadParameter(command, commandParameters);
                    try
                    {
                        return command.ExecuteScalar();
                    }
                    catch (Exception ex)
                    {
                        TrackingLogHelper.WriteTextLog($"[{commandID}]{method} ex:{ex.Message},{ex.InnerException?.Message}");
                        throw new Exception(BaseHelper.GetExMessageBody(ex));
                    }
                    finally
                    {
                        TrackingLogHelper.WriteTextLog($"[{commandID}]{method} end");
                    }
                }
            }
        }

        /// <summary>
        /// Executes the scalar asynchronous.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="commandText">The command text.</param>
        /// <param name="commandParameters">The command parameters.</param>
        /// <param name="timeout">The timeout.</param>
        /// <returns></returns>
        public static async Task<object> ExecuteScalarAsync(string connectionString, CommandType commandType, string commandText, List<IDataParameter> commandParameters, int timeout)
        {
            var commandID = Guid.NewGuid();
            var method = "ExecuteScalarAsync";
            await TrackingLogHelper.WriteTextLogAsync($"[{commandID}]{method} start, {commandText}");

            // Create & open a SqlConnection, and dispose of it after we are done
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                // Create a command and prepare it for execution
                using (var command = SQLHelper.CreateCommand(connection, commandType, commandText, timeout))
                {
                    SQLHelper.LoadParameter(command, commandParameters);
                    try
                    {
                        return await command.ExecuteScalarAsync();
                    }
                    catch (Exception ex)
                    {
                        await TrackingLogHelper.WriteTextLogAsync($"[{commandID}]{method} ex:{ex.Message},{ex.InnerException?.Message}");
                        throw new Exception(BaseHelper.GetExMessageBody(ex));
                    }
                    finally
                    {
                        TrackingLogHelper.WriteTextLog($"[{commandID}]{method} end");
                    }
                }
            }
        }
        /// <summary>
        /// Executes the return.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="commandText">The command text.</param>
        /// <param name="commandParameters">The command parameters.</param>
        /// <param name="timeout">The timeout.</param>
        /// <returns></returns>
        public static int ExecuteReturn(string connectionString, CommandType commandType, string commandText, List<IDataParameter> commandParameters, int timeout)
        {
            var commandID = Guid.NewGuid();
            var method = "ExecuteReturn";
            TrackingLogHelper.WriteTextLog($"[{commandID}]{method} start, {commandText}");

            // Create & open a SqlConnection, and dispose of it after we are done
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Create a command and prepare it for execution
                using (var command = SQLHelper.CreateCommand(connection, commandType, commandText, timeout))
                {
                    var returnParam = SQLHelper.CreateReturnParameter(SqlDbType.Int, 0);
                    SQLHelper.LoadParameter(command, commandParameters, returnParam);
                    try
                    {
                        command.ExecuteNonQuery();
                        return Convert.ToInt32(returnParam.Value);
                    }
                    catch (Exception ex)
                    {
                        TrackingLogHelper.WriteTextLog($"[{commandID}]{method} ex:{ex.Message},{ex.InnerException?.Message}");
                        throw new Exception(BaseHelper.GetExMessageBody(ex));
                    }
                    finally
                    {
                        TrackingLogHelper.WriteTextLog($"[{commandID}]{method} end");
                    }
                }
            }
        }

        /// <summary>
        /// Executes the return asynchronous.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="commandText">The command text.</param>
        /// <param name="commandParameters">The command parameters.</param>
        /// <param name="timeout">The timeout.</param>
        /// <returns></returns>
        public static async Task<int> ExecuteReturnAsync(string connectionString, CommandType commandType, string commandText, List<IDataParameter> commandParameters, int timeout)
        {
            var commandID = Guid.NewGuid();
            var method = "ExecuteReturnAsync";
            await TrackingLogHelper.WriteTextLogAsync($"[{commandID}]{method} start, {commandText}");
            // Create & open a SqlConnection, and dispose of it after we are done
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                // Create a command and prepare it for execution
                using (var command = SQLHelper.CreateCommand(connection, commandType, commandText, timeout))
                {
                    var returnParam = SQLHelper.CreateReturnParameter(SqlDbType.Int, 0);
                    SQLHelper.LoadParameter(command, commandParameters, returnParam);
                    try
                    {
                        await command.ExecuteNonQueryAsync();
                        return Convert.ToInt32(returnParam.Value);
                    }
                    catch (Exception ex)
                    {
                        await TrackingLogHelper.WriteTextLogAsync($"[{commandID}]{method} ex:{ex.Message},{ex.InnerException?.Message}");
                        throw new Exception(BaseHelper.GetExMessageBody(ex));
                    }
                    finally
                    {
                        await TrackingLogHelper.WriteTextLogAsync($"[{commandID}]{method} end");
                    }
                }
            }
        }

        /// <summary>
        /// Executes the return object.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="commandText">The command text.</param>
        /// <param name="commandParameters">The command parameters.</param>
        /// <param name="timeout">The timeout.</param>
        /// <returns></returns>
        public static object ExecuteReturnObj(string connectionString, CommandType commandType, string commandText, List<IDataParameter> commandParameters, int timeout)
        {
            var commandID = Guid.NewGuid();
            var method = "ExecuteReturnObj";
            TrackingLogHelper.WriteTextLog($"[{commandID}]{method} start, {commandText}");
            // Create & open a SqlConnection, and dispose of it after we are done
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Create a command and prepare it for execution
                using (var command = SQLHelper.CreateCommand(connection, commandType, commandText, timeout))
                {
                    var returnParam = SQLHelper.CreateReturnParameter(SqlDbType.Variant, null);
                    SQLHelper.LoadParameter(command, commandParameters, returnParam);
                    try
                    {
                        command.ExecuteNonQuery();
                        return returnParam.Value;
                    }
                    catch (Exception ex)
                    {
                        TrackingLogHelper.WriteTextLog($"[{commandID}]{method} ex:{ex.Message},{ex.InnerException?.Message}");
                        throw new Exception(BaseHelper.GetExMessageBody(ex));
                    }
                    finally
                    {
                        TrackingLogHelper.WriteTextLog($"[{commandID}]{method} end");
                    }
                }
            }
        }

        /// <summary>
        /// Executes the return asynchronous.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="commandText">The command text.</param>
        /// <param name="commandParameters">The command parameters.</param>
        /// <param name="timeout">The timeout.</param>
        /// <returns></returns>
        public static async Task<object> ExecuteReturnObjAsync(string connectionString, CommandType commandType, string commandText, List<IDataParameter> commandParameters, int timeout)
        {
            var commandID = Guid.NewGuid();
            var method = "ExecuteReturnObjAsync";
            await TrackingLogHelper.WriteTextLogAsync($"[{commandID}]{method} start, {commandText}");
            // Create & open a SqlConnection, and dispose of it after we are done
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                // Create a command and prepare it for execution
                using (var command = SQLHelper.CreateCommand(connection, commandType, commandText, timeout))
                {
                    var returnParam = SQLHelper.CreateReturnParameter(SqlDbType.Variant, null);
                    SQLHelper.LoadParameter(command, commandParameters, returnParam);
                    try
                    {
                        await command.ExecuteNonQueryAsync();
                        return returnParam.Value;
                    }
                    catch (Exception ex)
                    {
                        await TrackingLogHelper.WriteTextLogAsync($"[{commandID}]{method} ex:{ex.Message},{ex.InnerException?.Message}");
                        throw new Exception(BaseHelper.GetExMessageBody(ex));
                    }
                    finally
                    {
                        await TrackingLogHelper.WriteTextLogAsync($"[{commandID}]{method} end");
                    }
                }
            }
        }

        #endregion

        #region Private Methods
        /// <summary>
        /// Creates the command.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="commandText">The command text.</param>
        /// <param name="timeout">The timeout.</param>
        /// <returns></returns>
        private static SqlCommand CreateCommand(SqlConnection connection, CommandType commandType, string commandText, int timeout)
        {
            var command = connection.CreateCommand();
            command.CommandText = commandText;
            command.CommandType = commandType;
            command.CommandTimeout = timeout;
            return command;
        }

        /// <summary>
        /// Creates the return parameter.
        /// </summary>
        /// <returns></returns>
        private static IDataParameter CreateReturnParameter(SqlDbType dbType, object value)
        {
            var returnParam = new SqlParameter("@RETURN", dbType);
            returnParam.Direction = ParameterDirection.ReturnValue;
            returnParam.Value = value ?? DBNull.Value;
            return returnParam;
        }

        /// <summary>
        /// Loads the parameter.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="commandParameters">The command parameters.</param>
        /// <param name="returnParameter">The return parameter.</param>
        private static void LoadParameter(SqlCommand command, List<IDataParameter> commandParameters, IDataParameter returnParameter = null)
        {
            if (commandParameters != null)
            {
                foreach (var p in commandParameters)
                {
                    if (p.Value == null)
                        p.Value = DBNull.Value;

                    command.Parameters.Add(p);
                }
            }
            if (returnParameter != null)
                command.Parameters.Add(returnParameter);
        }

        #endregion
    }
}
