namespace ResumableFileTransfer.RfuCore.Models
{
    public class RfuFileStatus
    {
        public RfuResult Response { get; set; }
        public long Offset { get; set; }
    }
}
