using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ResumableFileTransfer.Entity
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum DataTransferAction
    {
        Create = 0,
        Start,
        Resume,
        Complete
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum DataTransferStatus
    {
        Failed = 0,
        Successfully
    }

    public enum SoftwareTokenStatus
    {
        Inactive = 0,
        Active
    }

    public enum SoftwareTokenValidateStatus
    {
        NotExists = 0,
        Disabled,
        Valid
    }

    public enum StorageType
    {
        AzureFileShare = 0,
        AzureBlob
    }
}
