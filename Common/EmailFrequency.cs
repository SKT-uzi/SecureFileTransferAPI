using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ResumableFileTransfer.Common
{
    internal class EmailSentInfo
    {
        #region Public Properties
        /// <summary>
        /// Gets or sets the previous send time.
        /// </summary>
        /// <value>
        /// The previous send time.
        /// </value>
        public DateTime PreviousSendTime { get; set; }

        /// <summary>
        /// Gets or sets the next send time.
        /// </summary>
        /// <value>
        /// The next send time.
        /// </value>
        public DateTime NextSendTime { get; set; }

        /// <summary>
        /// Gets or sets the send timers.
        /// </summary>
        /// <value>
        /// The send timers.
        /// </value>
        public int SendTimers { get; set; }
        #endregion
    }

    /// <summary>
    /// Email Frequency Class
    /// </summary>
    public class EmailFrequency
    {
        #region Private Member Variables
        //minutes
        private int _sendMailFrequencySeed = 0;
        private static readonly ConcurrentDictionary<string, EmailSentInfo> _dicErrors = new ConcurrentDictionary<string, EmailSentInfo>();
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="EmailFrequency"/> class.
        /// </summary>
        /// <param name="mailSentFrequencySeed">The mail sent frequency seed.</param>
        public EmailFrequency(int mailSentFrequencySeed)
        {
            this._sendMailFrequencySeed = mailSentFrequencySeed > 0 ? mailSentFrequencySeed : 1;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// To determine send error email when needed
        /// </summary>
        /// <param name="mailMsg">The mail MSG.</param>
        /// <returns>The process result</returns>
        public bool NeedSendMail(string mailMsg)
        {
            try
            {
                var flag = false;
                Task.Run(async () =>
                {
                    await Task.Delay(100);
                    //Clear old error mails
                    this.CleanTimeoutKeys();

                    this.FormatMailMsg(ref mailMsg);

                    if (_dicErrors.TryGetValue(mailMsg, out EmailSentInfo sendInfo) == false)
                    {
                        //Create new email object and set default values
                        sendInfo = new EmailSentInfo();
                        sendInfo.PreviousSendTime = DateTime.Now;
                        sendInfo.SendTimers = 1;
                        sendInfo.NextSendTime = this.GetNextSendTime(sendInfo.SendTimers);
                        _dicErrors.TryAdd(mailMsg, sendInfo);
                        flag = true;
                    }
                    else if (sendInfo.NextSendTime <= DateTime.Now)
                    {
                        //Contain the same error mail
                        sendInfo.PreviousSendTime = DateTime.Now;
                        sendInfo.SendTimers++;
                        sendInfo.NextSendTime = this.GetNextSendTime(sendInfo.SendTimers);
                        flag = true;
                    }
                }).Wait();
                return flag;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        #endregion

        #region Private Methods
        /// <summary>
        /// Formats the mail MSG.
        /// </summary>
        /// <param name="mailMsg">The mail MSG.</param>
        private void FormatMailMsg(ref string mailMsg)
        {
            try
            {
                //Remove HH:mm:ss
                var regex = new Regex(@"(?:[01]\d?|2[0-3])(?::[0-5]\d?|60){2}", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                mailMsg = regex.Replace(mailMsg, string.Empty);

                //Remove GUID string
                regex = new Regex("[A-Fa-f0-9]{8}(-[A-Fa-f0-9]{4}){3}-[A-Fa-f0-9]{12}", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                mailMsg = regex.Replace(mailMsg, string.Empty);
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }
        /// <summary>
        /// Cleans the timeout keys.
        /// </summary>
        private void CleanTimeoutKeys()
        {
            try
            {
                var removeKeyList = new List<string>();
                foreach (var item in _dicErrors)
                {
                    if (item.Value.PreviousSendTime.AddDays(1) < DateTime.Now)
                        removeKeyList.Add(item.Key);
                }
                foreach (var key in removeKeyList)
                {
                    if (_dicErrors.ContainsKey(key))
                        _dicErrors.TryRemove(key, out EmailSentInfo value);
                }
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Gets the next send time.
        /// </summary>
        /// <param name="sendTimers">The send timers.</param>
        /// <returns></returns>
        private DateTime GetNextSendTime(int sendTimers)
        {
            try
            {
                var speed = Math.Pow((double)2, (double)sendTimers - 1);
                return DateTime.Now.AddMinutes(speed * this._sendMailFrequencySeed);
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }
        #endregion
    }
}