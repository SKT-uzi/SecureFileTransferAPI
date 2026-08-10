using ResumableFileTransfer.RfuCore.Interfaces;

namespace ResumableFileTransfer.RfuCore.Models
{
    public interface IRfuConfiguration
    {
        public long MaxFileSize { get; set; }
        public int MaxFileNameLength { get; set; }
        public IDataStore DataStore { get; set; }
        public string UploadBasePath { get; set; }
        public string TargetBasePath { get; set; }
        public string RootFolder { get; set; }
        public string[] AllowedExtensions { get; set; }
        public string[] InvalidCharactersInName { get; set; }
    }
}
