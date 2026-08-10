namespace ResumableFileTransfer.RfuCore.Models
{
    public class RfuFile
    {
        // Properties of file
        public string FileID { get; set; }
        public RfuFileMetadata Metadata { get; set; }

        // Store
        public string UploadBasePath { get; set; }
        public string TargetBasePath { get; set; }
        public string MalwareBasePath { get; set; }

        // 
        public string Location { get; set; }
        public string UploadFullPath => FormatPath($"{FormatPath(UploadBasePath)}/{FileID}");
        public string MalwareFullPath => FormatPath($"{FormatPath(MalwareBasePath)}/{FileID}");
        public string TargetFullPath => FormatPath($"{FormatPath(TargetBasePath)}/{FormatPath(Metadata.RootFolder)}/{FormatPath(Metadata.Path)}/{Metadata.FileName}");

        // Upload info
        public RfuContentRange ContentRange { get; set; }
        public byte[] UploadContent { get; set; }
        public bool IsEmpty => ContentRange.Total == 0;

        private string FormatPath(string path)
        {
            path = path.Trim();
            path = path.Replace("\\", "/");

            while (path.Contains("//"))
                path = path.Replace("//", "/");

            while (path.StartsWith("/"))
            {
                if (path.Length > 1)
                    path = path.Substring(1);
                else
                    path = string.Empty;
            }

            while (path.EndsWith("/"))
            {
                if (path.Length > 1)
                    path = path.Substring(0, path.Length - 1);
            }

            return path;
        }
    }
}
