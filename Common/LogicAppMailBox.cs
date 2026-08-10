using Azure;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ResumableFileTransfer.Common
{
    class LogicAppMailBoxRequest
    {
        public string MethodName { get; set; }
        public string EmailUserName { get; set; }
        public string InboxName { get; set; }
        public string EmailID { get; set; }
        public string MoveToFolder { get; set; }
    }

    /// <summary>
    /// Mail Result
    /// </summary>
    class LogicAppMailBoxResult
    {
        public bool Success { get; set; }
        public object Data { get; set; }
    }

    /// <summary>
    /// AZLogicAppMailBox
    /// </summary>
    /// <seealso cref="ResumableFileTransfer.Common.IMailBox" />
    public class LogicAppMailBox : IMailBox
    {
        /// <summary>
        /// Gets or sets the request URL.
        /// </summary>
        /// <value>
        /// The request URL.
        /// </value>
        public string RequestURL { get; set; }

        /// <summary>
        /// Gets or sets the name of the mail box.
        /// </summary>
        /// <value>
        /// The name of the mail box.
        /// </value>
        public string MailBoxName { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogicAppMailBox"/> class.
        /// </summary>
        /// <param name="requestURL">The request URL.</param>
        /// <param name="mailBoxName">The mail box name</param>
        public LogicAppMailBox(string requestURL,string mailBoxName)
        {
            this.RequestURL = requestURL;
            this.MailBoxName = mailBoxName;
        }

        /// <summary>
        /// Gets the email list.
        /// </summary>
        /// <param name="folderName">Name of the folder.</param>
        /// <returns></returns>
        public List<MailItem> GetEmailList(string folderName)
        {
            try
            {
                var request = new LogicAppMailBoxRequest()
                {
                    MethodName = "GetEmailList",
                    EmailUserName = this.MailBoxName,
                    InboxName = folderName
                };

                var responseText = BaseHelper.PostRequest<LogicAppMailBoxRequest>(this.RequestURL, request, 5 * 60);
                var result = BaseHelper.DeserializeObject<LogicAppMailBoxResult>(responseText);
                if (result.Success == false)
                    throw new Exception(Convert.ToString(result.Data));

                return BaseHelper.DeserializeObject<List<MailItem>>(result.Data.ToString());
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }


        /// <summary>
        /// Downloads the attachments.
        /// </summary>
        /// <param name="emailID">The mail identifier.</param>
        /// <returns></returns>
        public List<MailAttachContent> DownloadAttachments(string emailID)
        {
            try
            {
                var request = new LogicAppMailBoxRequest()
                {
                    MethodName = "DownloadAttachments",
                    EmailUserName = this.MailBoxName,
                    EmailID = emailID
                };

                var responseText = BaseHelper.PostRequest<LogicAppMailBoxRequest>(this.RequestURL, request, 5 * 60);
                var result = BaseHelper.DeserializeObject<LogicAppMailBoxResult>(responseText);
                if (result.Success == false)
                    throw new Exception(Convert.ToString(result.Data));

                return BaseHelper.DeserializeObject<List<MailAttachContent>>(result.Data.ToString());
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Moves the email.
        /// </summary>
        /// <param name="emailID">The email identifier.</param>
        /// <param name="destFolder">The dest folder.</param>
        /// <returns></returns>
        public bool MoveEmail(string emailID, string destFolder)
        {
            try
            {
                var request = new LogicAppMailBoxRequest()
                {
                    MethodName = "MoveEmail",
                    EmailUserName = this.MailBoxName,
                    EmailID = emailID,
                    MoveToFolder = destFolder
                };
                var responseText = BaseHelper.PostRequest<LogicAppMailBoxRequest>(this.RequestURL, request, 5 * 60);
                var result = BaseHelper.DeserializeObject<LogicAppMailBoxResult>(responseText);
                if (result.Success == false)
                    throw new Exception(Convert.ToString(result.Data));

                var data = BaseHelper.DeserializeObject<List<bool>>(result.Data.ToString());
                return data[0];
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }
    }
}
