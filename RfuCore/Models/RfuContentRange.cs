namespace ResumableFileTransfer.RfuCore.Models
{
    public class RfuContentRange
    {
        public long Start { get; set; }
        public long End { get; set; }
        public long Total { get; set; }
    }
}
