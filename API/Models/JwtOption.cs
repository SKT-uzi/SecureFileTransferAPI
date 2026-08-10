namespace ResumableFileTransfer.API.Models
{
    public class JwtOption
    {
        public string SecretKey { get; set; }
        public int ExpireMinutes { get; set; }
    }
}
