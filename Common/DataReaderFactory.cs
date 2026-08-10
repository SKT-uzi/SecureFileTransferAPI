using System;
using System.Data;
using System.IO;
using System.Threading.Tasks;

namespace ResumableFileTransfer.Common
{
    internal class DataReaderFactory
    {
        private static readonly string _clsFullName = typeof(DataReaderFactory).FullName;

        public static async Task<IDataReader> CreateAsync(string format, Stream stream, dynamic schemaNode)
        {
            switch (format.ToUpper())
            {
                case "ARROW_STREAM":
                    return await ArrowDataReader.CreateAsync(stream);
                case "JSON":
                    return new JsonDataReader(stream, schemaNode);
                default:
                    throw BaseHelper.CreateException(_clsFullName, new Exception($"Unsupported format: {format}"));
            }
        }
    }
}
