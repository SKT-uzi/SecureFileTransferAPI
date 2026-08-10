namespace ResumableFileTransfer.API.Models
{
    public class AuthResponseResult
    {
        public string JWTToken { get; set; }
        public string CompanyName { get; set; }
        public string PhoneNumber { get; set; }
        public string[] AllowedExtensions { get; set; }
    }
}
