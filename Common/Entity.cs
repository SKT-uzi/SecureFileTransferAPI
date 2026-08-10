using System;
using System.Collections.Generic;
using System.Text;

namespace ResumableFileTransfer.Common
{
    #region Enum

    /// <summary>
    /// LogLevel
    /// </summary>
    public enum LogLevel
    {
        Information, 
        Error, 
        Warning
    }

    public enum UploadStatus
    {
        InProgress,
        Completed
    }

    public enum MailBoxAuthMode
    {
        AZLogicAppRequest
    }

    #endregion

    #region Class
    /// <summary>
    /// CacheInfo
    /// </summary>
    internal class CacheInfo
    {
        /// <summary>
        /// Gets or sets the expire date.
        /// </summary>
        /// <value>
        /// The expire date.
        /// </value>
        public DateTime ExpireDate { get; set; }

        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        public object Data { get; set; }
    }

    /// <summary>
    /// StorageItemInfo
    /// </summary>
    public class StorageItemInfo
    {
        /// <summary>
        /// Gets or sets the name of the file.
        /// contains container name
        /// </summary>
        /// <value>
        /// The name of the file.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the URL.
        /// </summary>
        /// <value>
        /// The URL.
        /// </value>
        public string URL { get; set; }

        /// <summary>
        /// Gets or sets the modify date.
        /// </summary>
        /// <value>
        /// The modify date.
        /// </value>
        public DateTime? CreateDate { get; set; }

        /// <summary>
        /// Gets or sets the modify date.
        /// </summary>
        /// <value>
        /// The modify date.
        /// </value>
        public DateTime? ModifyDate { get; set; }

        /// <summary>
        /// Gets or sets the size of the file.
        /// </summary>
        /// <value>
        /// The size of the file.
        /// </value>
        public long? FileSize { get; set; }
    }

    /// <summary>
    /// BlobItemInfo
    /// </summary>
    public class BlobItemInfo : StorageItemInfo
    {
        /// <summary>
        /// Gets or sets a value indicating whether this instance has folder.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has folder; otherwise, <c>false</c>.
        /// </value>
        public bool IsDirectory { get; set; }
    }

    /// <summary>
    /// FileItemInfo
    /// </summary>
    public class FileShareItemInfo: StorageItemInfo
    {
        /// <summary>
        /// Gets or sets the name of the dir.
        /// </summary>
        /// <value>
        /// The name of the dir.
        /// </value>
        public string DirName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance has folder.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has folder; otherwise, <c>false</c>.
        /// </value>
        public bool IsDirectory { get; set; }
    }

    /// <summary>
    /// Mail Attachment File
    /// </summary>
    public class MailAttachFile
    { 
        public string Name { get; set; }
        public long Size { get; set; }
    }

    /// <summary>
    /// Mail Attachment Content
    /// </summary>
    public class MailAttachContent
    {
        public string Name { get; set; }
        public Byte[] Content { get; set; }
    }

    /// <summary>
    /// Mail Item
    /// </summary>
    public class MailItem
    {
        public string ID { get; set; }
        public string From { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public DateTime ReceivedDate { get; set; }
        public bool HasAttachments { get; set; }
        public MailAttachFile[] Attachments { get; set; }
    }

    #endregion
}
