using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ResumableFileTransfer.Common
{
    public abstract class DABase
    {
        #region Constructor
        protected string _connectionString = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="DABase" /> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        public DABase(string connectionString)
        {
            this._connectionString = connectionString;
        }

        #endregion

        #region Run Command

        /// <summary>
        /// Creates the parameter.
        /// </summary>
        /// <param name="paraName">Name of the para.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        protected IDataParameter CreateParameter(string paraName, object value)
        {
            return SQLHelper.CreateParameter(paraName, value);
        }

        /// <summary>
        /// Creates the parameter.
        /// </summary>
        /// <param name="paraName">Name of the para.</param>
        /// <param name="dbType">Type of the database.</param>
        /// <returns></returns>
        protected IDataParameter CreateParameter(string paraName, SqlDbType dbType)
        {
            return SQLHelper.CreateParameter(paraName, dbType);
        }

        /// <summary>
        /// Creates the parameter.
        /// </summary>
        /// <param name="paraName">Name of the para.</param>
        /// <param name="dbType">Type of the database.</param>
        /// <param name="size">The size.</param>
        /// <param name="direction">The direction.</param>
        /// <returns></returns>
        protected IDataParameter CreateParameter(string paraName, SqlDbType dbType, int size, ParameterDirection direction)
        {
            return SQLHelper.CreateParameter(paraName, dbType, size, direction);
        }

        /// <summary>
        /// Executes the reader.
        /// </summary>
        /// <param name="procName">Name of the proc.</param>
        /// <param name="commandParameters">The command parameters.</param>
        /// <param name="timeout">The timeout.</param>
        /// <returns></returns>
        protected IDataReader ExecuteReader(string procName, List<IDataParameter> commandParameters = null, int timeout = 1800)
        {
            return SQLHelper.ExecuteReader(this._connectionString, CommandType.StoredProcedure, procName, commandParameters, timeout);
        }

        /// <summary>
        /// Executes the reader asynchronous.
        /// </summary>
        /// <param name="procName">Name of the proc.</param>
        /// <param name="commandParameters">The command parameters.</param>
        /// <param name="timeout">The timeout.</param>
        /// <returns></returns>
        protected async Task<DbDataReader> ExecuteReaderAsync(string procName, List<IDataParameter> commandParameters = null, int timeout = 1800)
        {
            return await SQLHelper.ExecuteReaderAsync(this._connectionString, CommandType.StoredProcedure, procName, commandParameters, timeout);
        }

        /// <summary>
        /// Executes the SQL reader.
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <param name="commandParameters">The command parameters.</param>
        /// <param name="timeout">The timeout.</param>
        /// <returns></returns>
        protected IDataReader ExecuteSQLReader(string sql, List<IDataParameter> commandParameters = null, int timeout = 1800)
        {
            return SQLHelper.ExecuteReader(this._connectionString, CommandType.Text, sql, commandParameters, timeout);
        }

        /// <summary>
        /// Executes the SQL reader asynchronous.
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <param name="commandParameters">The command parameters.</param>
        /// <param name="timeout">The timeout.</param>
        /// <returns></returns>
        protected async Task<DbDataReader> ExecuteSQLReaderAsync(string sql, List<IDataParameter> commandParameters = null, int timeout = 1800)
        {
            return await SQLHelper.ExecuteReaderAsync(this._connectionString, CommandType.Text, sql, commandParameters, timeout);
        }

        /// <summary>
        /// Executes the non query.
        /// </summary>
        /// <param name="procName">Name of the proc.</param>
        /// <param name="commandParameters">The command parameters.</param>
        /// <param name="timeout">The timeout.</param>
        /// <returns></returns>
        protected int ExecuteNonQuery(string procName, List<IDataParameter> commandParameters = null, int timeout = 1800)
        {
            return SQLHelper.ExecuteNonQuery(this._connectionString, CommandType.StoredProcedure, procName, commandParameters, timeout);
        }

        /// <summary>
        /// Executes the non query asynchronous.
        /// </summary>
        /// <param name="procName">Name of the proc.</param>
        /// <param name="commandParameters">The command parameters.</param>
        /// <param name="timeout">The timeout.</param>
        /// <returns></returns>
        protected async Task<int> ExecuteNonQueryAsync(string procName, List<IDataParameter> commandParameters = null, int timeout = 1800)
        {
            return await SQLHelper.ExecuteNonQueryAsync(this._connectionString, CommandType.StoredProcedure, procName, commandParameters, timeout);
        }

        /// <summary>
        /// Executes the non query.
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <param name="commandParameters">The command parameters.</param>
        /// <param name="timeout">The timeout.</param>
        /// <returns></returns>
        protected int ExecuteSQLNonQuery(string sql, List<IDataParameter> commandParameters = null, int timeout = 1800)
        {
            return SQLHelper.ExecuteNonQuery(this._connectionString, CommandType.Text, sql, commandParameters, timeout);
        }

        /// <summary>
        /// Executes the SQL non query asynchronous.
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <param name="commandParameters">The command parameters.</param>
        /// <param name="timeout">The timeout.</param>
        /// <returns></returns>
        protected async Task<int> ExecuteSQLNonQueryAsync(string sql, List<IDataParameter> commandParameters = null, int timeout = 1800)
        {
            return await SQLHelper.ExecuteNonQueryAsync(this._connectionString, CommandType.Text, sql, commandParameters, timeout);
        }

        /// <summary>
        /// Executes the return.
        /// </summary>
        /// <param name="procName">Name of the proc.</param>
        /// <param name="commandParameters">The command parameters.</param>
        /// <param name="timeout">The timeout.</param>
        /// <returns></returns>
        protected int ExecuteReturn(string procName, List<IDataParameter> commandParameters = null, int timeout = 1800)
        {
            return SQLHelper.ExecuteReturn(this._connectionString, CommandType.StoredProcedure, procName, commandParameters, timeout);
        }

        /// <summary>
        /// Executes the return asynchronous.
        /// </summary>
        /// <param name="procName">Name of the proc.</param>
        /// <param name="commandParameters">The command parameters.</param>
        /// <param name="timeout">The timeout.</param>
        /// <returns></returns>
        protected async Task<int> ExecuteReturnAsync(string procName, List<IDataParameter> commandParameters = null, int timeout = 1800)
        {
            return await SQLHelper.ExecuteReturnAsync(this._connectionString, CommandType.StoredProcedure, procName, commandParameters, timeout);
        }

        /// <summary>
        /// Executes the return object.
        /// </summary>
        /// <param name="procName">Name of the proc.</param>
        /// <param name="commandParameters">The command parameters.</param>
        /// <param name="timeout">The timeout.</param>
        /// <returns></returns>
        protected object ExecuteReturnObj(string procName, List<IDataParameter> commandParameters = null, int timeout = 1800)
        {
            return SQLHelper.ExecuteReturnObj(this._connectionString, CommandType.StoredProcedure, procName, commandParameters, timeout);
        }


        /// <summary>
        /// Executes the return object asynchronous.
        /// </summary>
        /// <param name="procName">Name of the proc.</param>
        /// <param name="commandParameters">The command parameters.</param>
        /// <param name="timeout">The timeout.</param>
        /// <returns></returns>
        protected async Task<object> ExecuteReturnObjAsync(string procName, List<IDataParameter> commandParameters = null, int timeout = 1800)
        {
            return await SQLHelper.ExecuteReturnObjAsync(this._connectionString, CommandType.StoredProcedure, procName, commandParameters, timeout);
        }

        /// <summary>
        /// Executes the scalar.
        /// </summary>
        /// <param name="procName">Name of the proc.</param>
        /// <param name="commandParameters">The command parameters.</param>
        /// <param name="timeout">The timeout.</param>
        /// <returns></returns>
        protected object ExecuteScalar(string procName, List<IDataParameter> commandParameters = null, int timeout = 1800)
        {
            return SQLHelper.ExecuteScalar(this._connectionString, CommandType.StoredProcedure, procName, commandParameters, timeout);
        }

        /// <summary>
        /// Executes the scalar.
        /// </summary>
        /// <param name="procName">Name of the proc.</param>
        /// <param name="commandParameters">The command parameters.</param>
        /// <param name="timeout">The timeout.</param>
        /// <returns></returns>
        protected async Task<object> ExecuteScalarAsync(string procName, List<IDataParameter> commandParameters = null, int timeout = 1800)
        {
            return await SQLHelper.ExecuteScalarAsync(this._connectionString, CommandType.StoredProcedure, procName, commandParameters, timeout);
        }

        /// <summary>
        /// Executes the SQL scalar.
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <param name="commandParameters">The command parameters.</param>
        /// <param name="timeout">The timeout.</param>
        /// <returns></returns>
        protected object ExecuteSQLScalar(string sql, List<IDataParameter> commandParameters = null, int timeout = 1800)
        {
            return SQLHelper.ExecuteScalar(this._connectionString, CommandType.Text, sql, commandParameters, timeout);
        }

        /// <summary>
        /// Executes the SQL scalar asynchronous.
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <param name="commandParameters">The command parameters.</param>
        /// <param name="timeout">The timeout.</param>
        /// <returns></returns>
        protected async Task<object> ExecuteSQLScalarAsync(string sql, List<IDataParameter> commandParameters = null, int timeout = 1800)
        {
            return await SQLHelper.ExecuteScalarAsync(this._connectionString, CommandType.Text, sql, commandParameters, timeout);
        }

        /// <summary>
        /// Executes the data table.
        /// </summary>
        /// <param name="procName">Name of the proc.</param>
        /// <param name="commandParameters">The command parameters.</param>
        /// <param name="timeout">The timeout.</param>
        /// <returns></returns>
        protected DataTable ExecuteDataTable(string procName, List<IDataParameter> commandParameters = null, int timeout = 1800)
        {
            return SQLHelper.ExecuteDataTable(this._connectionString, CommandType.StoredProcedure, procName, commandParameters, timeout);

        }

        /// <summary>
        /// Executes the SQL data table.
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <param name="commandParameters">The command parameters.</param>
        /// <param name="timeout">The timeout.</param>
        /// <returns></returns>
        protected DataTable ExecuteSQLDataTable(string sql, List<IDataParameter> commandParameters = null, int timeout = 1800)
        {
            return SQLHelper.ExecuteDataTable(this._connectionString, CommandType.Text, sql, commandParameters, timeout);
        }

        /// <summary>
        /// Executes the data set.
        /// </summary>
        /// <param name="procName">Name of the proc.</param>
        /// <param name="commandParameters">The command parameters.</param>
        /// <param name="timeout">The timeout.</param>
        /// <returns></returns>
        protected DataSet ExecuteDataSet(string procName, List<IDataParameter> commandParameters = null, int timeout = 1800)
        {
            return SQLHelper.ExecuteDataSet(this._connectionString, CommandType.StoredProcedure, procName, commandParameters, timeout);
        }

        /// <summary>
        /// Executes the SQL data set.
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <param name="commandParameters">The command parameters.</param>
        /// <param name="timeout">The timeout.</param>
        /// <returns></returns>
        protected DataSet ExecuteSQLDataSet(string sql, List<IDataParameter> commandParameters=null, int timeout = 1800)
        {
            return SQLHelper.ExecuteDataSet(this._connectionString, CommandType.Text, sql, commandParameters, timeout);
        }

        /// <summary>
        /// Datas the access exception.
        /// </summary>
        /// <param name="ex">The ex.</param>
        /// <param name="procNameOrSQL">The proc name or SQL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="memberName">Name of the member.</param>
        /// <returns></returns>
        protected Exception DataAccessException(Exception ex, string procNameOrSQL, List<IDataParameter> parameters=null, [CallerMemberName] string memberName = "")
        {
            var sbMessage = new StringBuilder();
            sbMessage.AppendLine($"{this.GetType().FullName}.{memberName} DataAccessException:");
            sbMessage.AppendLine(procNameOrSQL);
            if (parameters != null)
            {
                sbMessage.AppendLine("ProcParameter:{");
                foreach (IDataParameter parameter in parameters)
                {
                    if (parameter == null)
                        continue;

                    sbMessage.AppendLine($"{parameter.ParameterName}:{parameter.Value}");
                }

                sbMessage.AppendLine("}");
            }
            sbMessage.Append($"Exception:{BaseHelper.GetExMessageBody(ex)}");
            return new Exception(sbMessage.ToString());
        }

        #endregion

        #region Get Field Value

        /// <summary>
        /// Gets all fields.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns></returns>
        protected List<string> GetAllFields(IDataReader reader)
        {
            var fieldList = new List<string>();
            var dtSchema = reader.GetSchemaTable();
            if (dtSchema != null)
            {
                foreach (DataRow item in dtSchema.Rows)
                {
                    var colName = Convert.ToString(item["ColumnName"]);
                    if (colName != null && string.IsNullOrEmpty(colName)==false)
                    {
                        fieldList.Add(colName);
                    }
                }
            }
            fieldList.TrimExcess();
            return fieldList;
        }

        /// <summary>
        /// Gets the unique identifier value.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns></returns>
        protected Guid GetGuidValue(IDataReader reader, string fieldName)
        {
            try
            {
                int i = reader.GetOrdinal(fieldName);
                if (!reader.IsDBNull(i))
                    return reader.GetGuid(i);

                return Guid.Empty;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex, $"FieldName={fieldName}");
            }
        }

        /// <summary>
        /// Gets the unique identifier null value.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns></returns>
        protected Guid? GetGuidNullValue(IDataReader reader, string fieldName)
        {
            try
            {
                int i = reader.GetOrdinal(fieldName);
                if (!reader.IsDBNull(i))
                    return reader.GetGuid(i);

                return null;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex, $"FieldName={fieldName}");
            }
        }

        /// <summary>
        /// Gets the string value.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>
        /// return value
        /// </returns>
        protected string GetStringValue(IDataReader reader, string fieldName)
        {
            try
            {
                int i = reader.GetOrdinal(fieldName);
                if (!reader.IsDBNull(i))
                    return reader.GetString(i).Trim();

                return string.Empty;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex, $"FieldName={fieldName}");
            }
        }

        /// <summary>
        /// Gets the int value.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>
        /// return value
        /// </returns>
        protected byte GetByteValue(IDataReader reader, string fieldName)
        {
            try
            {
                int i = reader.GetOrdinal(fieldName);
                if (!reader.IsDBNull(i))
                {
                    try
                    {
                        return reader.GetByte(i);
                    }
                    catch
                    {
                        return Convert.ToByte(reader[i]);
                    }
                }

                return 0;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex, $"FieldName={fieldName}");
            }
        }

        /// <summary>
        /// Gets the byte null value.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns></returns>
        protected byte? GetByteNullValue(IDataReader reader, string fieldName)
        {
            try
            {
                int i = reader.GetOrdinal(fieldName);
                if (!reader.IsDBNull(i))
                {
                    try
                    {
                        return reader.GetByte(i);
                    }
                    catch
                    {
                        return Convert.ToByte(reader[i]);
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex, $"FieldName={fieldName}");
            }
        }

        /// <summary>
        /// Gets the bytes value.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns></returns>
        protected byte[] GetByteArrayValue(IDataReader reader, string fieldName)
        {
            try
            {
                int i = reader.GetOrdinal(fieldName);
                if (!reader.IsDBNull(i))
                {
                    return (byte[])reader.GetValue(i);
                }
                return null;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex, $"FieldName={fieldName}");
            }
        }

        /// <summary>
        /// Gets the int value.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>
        /// return value
        /// </returns>
        protected int GetIntValue(IDataReader reader, string fieldName)
        {
            try
            {
                int i = reader.GetOrdinal(fieldName);
                if (!reader.IsDBNull(i))
                {
                    try
                    {
                        return reader.GetInt32(i);
                    }
                    catch
                    {
                        return Convert.ToInt32(reader[i]);
                    }
                }

                return 0;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex, $"FieldName={fieldName}");
            }
        }

        /// <summary>
        /// Gets the int null value.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>
        /// return value
        /// </returns>
        protected int? GetIntNullValue(IDataReader reader, string fieldName)
        {
            try
            {
                int i = reader.GetOrdinal(fieldName);
                if (!reader.IsDBNull(i))
                {
                    try
                    {
                        return reader.GetInt32(i);
                    }
                    catch
                    {
                        return Convert.ToInt32(reader[i]);
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex, $"FieldName={fieldName}");
            }
        }

        /// <summary>
        /// Gets the int64 value.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns></returns>
        protected Int64 GetInt64Value(IDataReader reader, string fieldName)
        {
            try
            {
                int i = reader.GetOrdinal(fieldName);
                if (!reader.IsDBNull(i))
                {
                    try
                    {
                        return reader.GetInt64(i);
                    }
                    catch
                    {
                        return Convert.ToInt64(reader[i]);
                    }
                }

                return 0;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex, $"FieldName={fieldName}");
            }
        }

        /// <summary>
        /// Gets the int64 null value.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns></returns>
        protected Int64? GetInt64NullValue(IDataReader reader, string fieldName)
        {
            try
            {
                int i = reader.GetOrdinal(fieldName);
                if (!reader.IsDBNull(i))
                {
                    try
                    {
                        return reader.GetInt64(i);
                    }
                    catch
                    {
                        return Convert.ToInt64(reader[i]);
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex, $"FieldName={fieldName}");
            }
        }
        /// <summary>
        /// Gets the bool value.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>
        /// return value
        /// </returns>
        protected bool GetBoolValue(IDataReader reader, string fieldName)
        {
            try
            {
                int i = reader.GetOrdinal(fieldName);
                if (!reader.IsDBNull(i))
                    return reader.GetBoolean(i);

                return false;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex, $"FieldName={fieldName}");
            }
        }

        /// <summary>
        /// Gets the bool null value.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>return value</returns>
        protected bool? GetBoolNullValue(IDataReader reader, string fieldName)
        {
            try
            {
                int i = reader.GetOrdinal(fieldName);
                if (!reader.IsDBNull(i))
                    return reader.GetBoolean(i);

                return null;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex, $"FieldName={fieldName}");
            }
        }

        /// <summary>
        /// Gets the date value.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>
        /// return value
        /// </returns>
        protected DateTime GetDateValue(IDataReader reader, string fieldName)
        {
            try
            {
                int i = reader.GetOrdinal(fieldName);
                if (!reader.IsDBNull(i))
                {
                    try
                    {
                        return reader.GetDateTime(i);
                    }
                    catch
                    {
                        DateTime dtValue;
                        DateTime.TryParse(reader.GetString(i), out dtValue);
                        return dtValue;
                    }
                }

                return DateTime.MinValue;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex, $"FieldName={fieldName}");
            }
        }

        /// <summary>
        /// Gets the date value.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="format">The format.</param>
        /// <returns></returns>
        protected string GetDateValue(IDataReader reader, string fieldName, string format)
        {
            try
            {
                int i = reader.GetOrdinal(fieldName);
                if (!reader.IsDBNull(i))
                {
                    try
                    {
                        return reader.GetDateTime(i).ToString(format);
                    }
                    catch
                    {
                        DateTime dtValue;
                        DateTime.TryParse(reader.GetString(i), out dtValue);
                        return dtValue.ToString(format);
                    }
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex, $"FieldName={fieldName}");
            }
        }

        /// <summary>
        /// Gets the date null value.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>
        /// return value
        /// </returns>
        protected DateTime? GetDateNullValue(IDataReader reader, string fieldName)
        {
            try
            {
                int i = reader.GetOrdinal(fieldName);
                if (!reader.IsDBNull(i))
                {
                    try
                    {
                        return reader.GetDateTime(i);
                    }
                    catch
                    {
                        DateTime dtValue;
                        DateTime.TryParse(reader.GetString(i), out dtValue);
                        return dtValue;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex, $"FieldName={fieldName}");
            }
        }

        /// <summary>
        /// Gets the double value.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>return value</returns>
        protected double GetDoubleValue(IDataReader reader, string fieldName)
        {
            try
            {
                int i = reader.GetOrdinal(fieldName);
                if (!reader.IsDBNull(i))
                {
                    try
                    {
                        return reader.GetDouble(i);
                    }
                    catch
                    {
                        return Convert.ToDouble(reader[i]);
                    }
                }

                return 0;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex, $"FieldName={fieldName}");
            }
        }

        /// <summary>
        /// Gets the double null value.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>return value</returns>
        protected double? GetDoubleNullValue(IDataReader reader, string fieldName)
        {
            try
            {
                int i = reader.GetOrdinal(fieldName);
                if (!reader.IsDBNull(i))
                {
                    try
                    {
                        return reader.GetDouble(i);
                    }
                    catch
                    {
                        return Convert.ToDouble(reader[i]);
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex, $"FieldName={fieldName}");
            }
        }

        /// <summary>
        /// Gets the decimal value.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns></returns>
        protected decimal GetDecimalValue(IDataReader reader, string fieldName)
        {
            try
            {
                int i = reader.GetOrdinal(fieldName);
                if (!reader.IsDBNull(i))
                {
                    try
                    {
                        return reader.GetDecimal(i);
                    }
                    catch
                    {
                        return Convert.ToDecimal(reader[i]);
                    }
                }

                return 0;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex, $"FieldName={fieldName}");
            }
        }

        /// <summary>
        /// Gets the decimal null value.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns></returns>
        protected decimal? GetDecimalNullValue(IDataReader reader, string fieldName)
        {
            try
            {
                int i = reader.GetOrdinal(fieldName);
                if (!reader.IsDBNull(i))
                {
                    try
                    {
                        return reader.GetDecimal(i);
                    }
                    catch
                    {
                        return Convert.ToDecimal(reader[i]);
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex, $"FieldName={fieldName}");
            }
        }

        /// <summary>
        /// Gets the float value.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns></returns>
        protected float GetFloatValue(IDataReader reader, string fieldName)
        {
            try
            {
                int i = reader.GetOrdinal(fieldName);
                if (!reader.IsDBNull(i))
                {
                    try
                    {
                        return reader.GetFloat(i);
                    }
                    catch
                    {
                        float fValue = 0;
                        float.TryParse(Convert.ToString(reader[i]), out fValue);
                        return fValue;
                    }
                }

                return 0;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex, $"FieldName={fieldName}");
            }
        }

        /// <summary>
        /// Gets the float null value.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns></returns>
        protected float? GetFloatNullValue(IDataReader reader, string fieldName)
        {
            try
            {
                int i = reader.GetOrdinal(fieldName);
                if (!reader.IsDBNull(i))
                {
                    try
                    {
                        return reader.GetFloat(i);
                    }
                    catch
                    {
                        float fValue = 0;
                        float.TryParse(Convert.ToString(reader[i]), out fValue);
                        return fValue;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex, $"FieldName={fieldName}");
            }
        }
        #endregion
    }
}
