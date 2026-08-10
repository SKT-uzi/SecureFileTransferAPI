using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace ResumableFileTransfer.Common
{
    internal class JsonDataReader : IDataReader
    {
        private static readonly string _clsFullName = typeof(JsonDataReader).FullName;

        private readonly StreamReader _reader;
        private JsonTextReader _jsonReader;
        private readonly List<string> _columnNames;
        private readonly Dictionary<string, int> _columnIndexes;
        private JToken _currentRow;
        private bool _isClosed = false;
        private int _currentRowIndex = -1;

        public JsonDataReader(Stream jsonStream, dynamic schema)
        {
            this._reader = new StreamReader(jsonStream);
            this._jsonReader = new JsonTextReader(this._reader);
            this._jsonReader.Read(); // StartArray

            this._columnNames = new List<string>();
            this._columnIndexes = new Dictionary<string, int>();

            int i = 0;
            foreach (var col in schema.columns)
            {
                string name = col.name;
                this._columnNames.Add(name);
                this._columnIndexes[name] = i;
                i++;
            }
        }

        public bool Read()
        {
            if (this._jsonReader.Read())
            {
                if (this._jsonReader.TokenType == JsonToken.StartArray)
                {
                    this._currentRow = JArray.Load(this._jsonReader);
                    this._currentRowIndex++;
                    return true;
                }
            }
            return false;
        }

        public int FieldCount => this._columnNames.Count;

        public string GetName(int ordinal)
        {
            return this._columnNames[ordinal];
        }

        public Type GetFieldType(int ordinal)
        {
            return typeof(string);
        }

        public object GetValue(int ordinal)
        {
            var value = this._currentRow[ordinal] != null ? this._currentRow[ordinal].ToString() : null;

            if (value == null)
            {
                return DBNull.Value;
            }
            else
            {
                return value;
            }
        }

        public int GetOrdinal(string name)
        {
            return this._columnIndexes.TryGetValue(name, out var index) ? index : -1;
        }

        public bool IsDBNull(int ordinal)
        {
            return this.GetValue(ordinal) == DBNull.Value;
        }

        #region Not Implemented IDataReader Methods

        public void Close()
        {
            this._reader.Close();
            this._isClosed = true;
        }

        public void Dispose()
        {
            this._reader.Dispose();
        }

        public DataTable GetSchemaTable()
        {
            throw new NotImplementedException();
        }

        public bool NextResult()
        {
            throw new NotImplementedException();
        }

        public bool GetBoolean(int i)
        {
            throw new NotImplementedException();
        }

        public byte GetByte(int i)
        {
            throw new NotImplementedException();
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public char GetChar(int i)
        {
            throw new NotImplementedException();
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public IDataReader GetData(int i)
        {
            throw new NotImplementedException();
        }

        public string GetDataTypeName(int i)
        {
            throw new NotImplementedException();
        }

        public DateTime GetDateTime(int i)
        {
            throw new NotImplementedException();
        }

        public decimal GetDecimal(int i)
        {
            throw new NotImplementedException();
        }

        public double GetDouble(int i)
        {
            throw new NotImplementedException();
        }

        public float GetFloat(int i)
        {
            throw new NotImplementedException();
        }

        public Guid GetGuid(int i)
        {
            throw new NotImplementedException();
        }

        public short GetInt16(int i)
        {
            throw new NotImplementedException();
        }

        public int GetInt32(int i)
        {
            throw new NotImplementedException();
        }

        public long GetInt64(int i)
        {
            throw new NotImplementedException();
        }

        public string GetString(int i)
        {
            throw new NotImplementedException();
        }

        public int GetValues(object[] values)
        {
            throw new NotImplementedException();
        }

        public int Depth => 0;

        public bool IsClosed => this._isClosed;

        public int RecordsAffected => -1;

        public object this[string name] => this.GetValue(this.GetOrdinal(name));

        public object this[int ordinal] => this.GetValue(ordinal);

        #endregion
    }

}
