namespace ResumableFileTransfer.Entity
{
    public class AccountSoftwareToken
    {
        public int TokenID { get; set; }
        public int AccountID { get; set; }
        public string AccountName { get; set; }
        public string PhoneNumber { get; set; }
        public string DataTransferType { get; set; }
        public string Token { get; set; }
        public string DataType { get; set; }
        public string FileFormat { get; set; }
        public string TargetFileLocation { get; set; }
        public int Status { get; set; }
        public int CreateBy { get; set; }
        public int? ChangeBy { get; set; }
    }
}
