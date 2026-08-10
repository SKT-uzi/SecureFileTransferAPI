using Apache.Arrow;
using Apache.Arrow.Ipc;
using Apache.Arrow.Types;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ResumableFileTransfer.Common
{
    internal class ArrowDataReader : IDataReader
    {
        private static readonly string _clsFullName = typeof(ArrowDataReader).FullName;

        private readonly ArrowStreamReader _arrowReader;
        private RecordBatch _currentBatch;
        private IArrowArray[] _currentArrays = System.Array.Empty<IArrowArray>();
        // cached per-column getters to avoid switch on every GetValue call
        private Func<int, object>[] _columnGetters = System.Array.Empty<Func<int, object>>();
        private int _currentRowIndex = -1;
        private bool _endOfStream = false;
        private readonly Schema _schema;

        private ArrowDataReader(ArrowStreamReader arrowReader, Schema schema, RecordBatch firstBatch)
        {
            this._arrowReader = arrowReader;
            this._schema = schema ?? throw BaseHelper.CreateException(_clsFullName, new InvalidOperationException("Arrow stream schema is null."));
            this._currentBatch = firstBatch;

            if (firstBatch != null)
            {
                this._currentArrays = firstBatch.Arrays.ToArray();
                this.BuildColumnGetters();
            }
        }

        public static async Task<ArrowDataReader> CreateAsync(Stream inputStream)
        {
            if (inputStream == null)
                throw BaseHelper.CreateException(_clsFullName, new ArgumentNullException(nameof(inputStream)));

            var reader = new ArrowStreamReader(inputStream);
            var schema = reader.Schema ?? throw BaseHelper.CreateException(_clsFullName, new InvalidOperationException("Arrow stream schema is null."));

            var firstBatch = await reader.ReadNextRecordBatchAsync().ConfigureAwait(false);

            return new ArrowDataReader(reader, schema, firstBatch);
        }

        public bool Read()
        {
            if (this._endOfStream) return false;

            this._currentRowIndex++;

            if (this._currentBatch == null || this._currentRowIndex >= this._currentBatch.Length)
            {
                var nextBatch = this._arrowReader.ReadNextRecordBatchAsync().ConfigureAwait(false).GetAwaiter().GetResult();

                if (nextBatch == null)
                {
                    this._endOfStream = true;
                    this._currentArrays = System.Array.Empty<IArrowArray>();
                    this._columnGetters = System.Array.Empty<Func<int, object>>();
                    return false;
                }

                this._currentBatch = nextBatch;
                this._currentArrays = this._currentBatch.Arrays.ToArray();
                this.BuildColumnGetters();
                this._currentRowIndex = 0;
            }

            return true;
        }

        /* Old synchronous constructor and Read method
        public ArrowDataReader(Stream inputStream)
        {
            _arrowReader = new ArrowStreamReader(inputStream ?? throw new ArgumentNullException(nameof(inputStream)));
            _currentBatch = _arrowReader.ReadNextRecordBatchAsync().GetAwaiter().GetResult();
            if (_currentBatch != null)
            {
                _currentArrays = _currentBatch.Arrays.ToArray();
                BuildColumnGetters();
            }
            _schema = _arrowReader.Schema ?? throw new InvalidOperationException("Arrow stream schema is null.");
        }

        public bool Read()
        {
            if (_endOfStream) return false;

            _currentRowIndex++;

            if (_currentBatch == null || _currentRowIndex >= _currentBatch.Length)
            {
                _currentBatch = _arrowReader.ReadNextRecordBatchAsync().GetAwaiter().GetResult();
                if (_currentBatch == null)
                {
                    _endOfStream = true;
                    _currentArrays = System.Array.Empty<IArrowArray>();
                    _columnGetters = System.Array.Empty<Func<int, object>>();
                    return false;
                }
                _currentArrays = _currentBatch.Arrays.ToArray();
                BuildColumnGetters();
                _currentRowIndex = 0;
            }

            return true;
        }
        */

        public int FieldCount => this._schema.FieldsList.Count;

        public string GetName(int i) => this._schema.FieldsList[i].Name;

        public Type GetFieldType(int i)
        {
            return ArrowTypeToDotNet(this._schema.FieldsList[i].DataType);
        }

        public object GetValue(int i)
        {
            if (this._endOfStream || this._currentBatch == null) return DBNull.Value;
            if ((uint)i >= (uint)this._columnGetters.Length) throw BaseHelper.CreateException(_clsFullName, new IndexOutOfRangeException($"Field index {i} out of range."));
            var getter = this._columnGetters[i];
            return getter != null ? getter(this._currentRowIndex) : DBNull.Value;
        }

        // Build per-column delegate getters once when a new RecordBatch is loaded.
        private void BuildColumnGetters()
        {
            if (this._currentArrays == null || this._currentArrays.Length == 0)
            {
                this._columnGetters = System.Array.Empty<Func<int, object>>();
                return;
            }

            var getters = new Func<int, object>[this._currentArrays.Length];
            for (int j = 0; j < this._currentArrays.Length; j++)
            {
                var arr = this._currentArrays[j]; // local copy to avoid closure trap

                if (arr == null)
                {
                    getters[j] = _ => DBNull.Value;
                    continue;
                }

                switch (arr)
                {
                    case Int32Array a:
                        getters[j] = row => a.IsValid(row) ? (object)a.GetValue(row) : DBNull.Value;
                        break;
                    case Int64Array a:
                        getters[j] = row => a.IsValid(row) ? (object)a.GetValue(row) : DBNull.Value;
                        break;
                    case FloatArray a:
                        getters[j] = row => a.IsValid(row) ? (object)a.GetValue(row) : DBNull.Value;
                        break;
                    case DoubleArray a:
                        getters[j] = row => a.IsValid(row) ? (object)a.GetValue(row) : DBNull.Value;
                        break;
                    case BooleanArray a:
                        getters[j] = row => a.IsValid(row) ? (object)a.GetValue(row) : DBNull.Value;
                        break;
                    case StringArray a:
                        getters[j] = row => a.IsValid(row) ? (object)a.GetString(row) : DBNull.Value;
                        break;
                    case TimestampArray a:
                        getters[j] = row => a.IsValid(row) ? (object)a.GetTimestamp(row)?.DateTime : DBNull.Value;
                        break;
                    //case Date32Array a:
                    //    getters[j] = row => a.IsValid(row) ? (object)DateTime.UnixEpoch.AddDays((double)a.GetValue(row)) : DBNull.Value;
                    //    break;
                    default:
                        getters[j] = _ => DBNull.Value;
                        break;
                }
            }

            this._columnGetters = getters;
        }

        private static Type ArrowTypeToDotNet(IArrowType type)
        {
            switch (type.TypeId)
            {
                case ArrowTypeId.Int8:
                case ArrowTypeId.Int16:
                case ArrowTypeId.Int32: return typeof(int);
                case ArrowTypeId.Int64: return typeof(long);
                case ArrowTypeId.Float: return typeof(float);
                case ArrowTypeId.Double: return typeof(double);
                case ArrowTypeId.String: return typeof(string);
                case ArrowTypeId.Boolean: return typeof(bool);
                case ArrowTypeId.Timestamp:
                case ArrowTypeId.Date32: return typeof(DateTime);
                default: return typeof(object);
            }
        }


        public int GetOrdinal(string name)
        {
            for (int i = 0; i < this._schema.FieldsList.Count; i++)
                if (string.Equals(this._schema.FieldsList[i].Name, name, StringComparison.OrdinalIgnoreCase))
                    return i;
            throw BaseHelper.CreateException(_clsFullName, new IndexOutOfRangeException($"Field '{name}' not found in schema."));
        }

        public bool IsDBNull(int i)
        {
            var array = (i >= 0 && i < this._currentArrays.Length) ? this._currentArrays[i] : null;
            return array == null || !array.IsValid(this._currentRowIndex);
        }

        public int GetValues(object[] values)
        {
            int count = Math.Min(values.Length, this.FieldCount);
            for (int i = 0; i < count; i++)
                values[i] = this.GetValue(i);
            return count;
        }

        public object this[int i] => this.GetValue(i);
        public object this[string name] => this.GetValue(this.GetOrdinal(name));

        public bool NextResult() => false;
        public int Depth => 0;
        public bool IsClosed => this._endOfStream;
        public int RecordsAffected => -1;
        public bool HasRows => this._currentBatch?.Length > 0;
        public void Close() => Dispose();

        public DataTable GetSchemaTable()
        {
            var table = new DataTable();
            foreach (var field in this._schema.FieldsList)
            {
                table.Columns.Add(field.Name, ArrowTypeToDotNet(field.DataType));
            }
            return table;
        }

        public void Dispose()
        {
            this._arrowReader?.Dispose();
        }

        #region Not Implemented IDataReader Methods
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
        #endregion        
    }
}