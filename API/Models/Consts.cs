namespace ResumableFileTransfer.API.Models
{
    public static class Consts
    {
        public static string SOFTWARE_TOKEN = "token";
        public static string ENCRYPTED_SOFTWARE_TOKEN { get; private set; }

        public static string DATA_TRANSFER_TYPE_INSTALLER = "Resumable File Transfer Installer";
        public static string DATA_TRANSFER_TYPE_API = "API";
        public static string DATA_TRANSFER_TYPE_PORTAL = "Web Portal";

        public static void Initialize()
        {
            ENCRYPTED_SOFTWARE_TOKEN ??= CommonHelper.Encrypt(SOFTWARE_TOKEN);
        }
    }
}
