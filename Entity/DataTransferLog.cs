using System;
using Newtonsoft.Json;

namespace ResumableFileTransfer.Entity
{
    public class DataTransferLog
    {
        [JsonIgnore]
        public int Id { get; set; }
        public string FileID { get; set; }
        public string Path { get; set; }
        public string FileName { get; set; }
        public long? FileSize { get; set; }
        public string Note { get; set; }
        public string ClientIP { get; set; }
        [JsonIgnore]
        public DateTime CreatedAtUtc { get; set; }

        public DataTransferAction Action { get; set; }
        public DataTransferStatus Status { get; set; }
    }
}
