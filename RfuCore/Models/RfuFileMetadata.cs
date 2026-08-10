using Newtonsoft.Json;

namespace ResumableFileTransfer.RfuCore.Models
{
    public class RfuFileMetadata
    {
        [JsonProperty("name")]
        public string FileName { get; set; }
        [JsonProperty("path")]
        public string Path { get; set; }
        [JsonProperty("rootFolder")]
        public string RootFolder { get; set; }

        public RfuFileMetadata()
        {
            Path = string.Empty;
        }
    }
}
