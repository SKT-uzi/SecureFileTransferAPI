using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using System.Xml;

namespace ResumableFileTransfer.Common
{
    /// <summary>
    /// BaseHelper
    /// </summary>
    public class BaseHelper
    {
        #region Base Methods
        /// <summary>
        /// The CLS full name
        /// </summary>
        private static readonly string _clsFullName = typeof(BaseHelper).FullName;

        /// <summary>
        /// memory Stream Buffer Length 8KB
        /// </summary>
        public const int BufferSize = 8 * 1024;

        /// <summary>
        /// memory Stream Buffer Length 1MB
        /// </summary>
        public const int BufferSize_1MB = 1024 * 1024;

        /// <summary>
        /// memory Stream Buffer Length 4MB
        /// </summary>
        public const int BufferSize_4MB = 4 * 1024 * 1024;

        /// <summary>
        /// Equalses the ignore case.
        /// </summary>
        /// <param name="value1">The value1.</param>
        /// <param name="value2">The value2.</param>
        /// <returns></returns>
        public static bool EqualsIgnoreCase(string value1, string value2)
        {
            try
            {
                if (value1 == null)
                    value1 = string.Empty;

                if (value2 == null)
                    value2 = string.Empty;

                return value1.Trim().Equals(value2.Trim(), StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(_clsFullName, ex);
            }
        }

        /// <summary>
        /// Equalses the list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value1List">The value1 list.</param>
        /// <param name="value2List">The value2 list.</param>
        /// <param name="equalFun">The equal fun.</param>
        /// <returns></returns>
        public static bool EqualsList<T>(List<T> value1List, List<T> value2List, Func<T, T, bool> equalFun)
        {
            try
            {
                if (value1List?.Count != value2List?.Count)
                    return false;

                for (var i = 0; i < value1List?.Count; i++)
                {
                    if (equalFun(value1List[i], value2List[i]) == false)
                        return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(_clsFullName, ex);
            }
        }

        /// <summary>
        /// Contains the value.
        /// </summary>
        /// <param name="valueList">The value list.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static bool ContainIgnoreCase(List<string> valueList, string value)
        {
            try
            {
                foreach (var item in valueList)
                {
                    if (BaseHelper.EqualsIgnoreCase(item, value))
                        return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(_clsFullName, ex);
            }
        }

        /// <summary>
        /// Contains the specified value list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="valueList">The value list.</param>
        /// <param name="value">The value.</param>
        /// <param name="equalFun">The equal fun.</param>
        /// <returns></returns>
        public static bool Contain<T>(List<T> valueList, T value, Func<T, T, bool> equalFun)
        {
            try
            {
                foreach (var item in valueList)
                {
                    if (equalFun(item, value))
                        return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(_clsFullName, ex);
            }
        }

        /// <summary>
        /// Indexes the of ignore case.
        /// </summary>
        /// <param name="valueList">The value list.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static int IndexOfIgnoreCase(List<string> valueList, string value)
        {
            try
            {
                for (var i = 0; i < valueList.Count; i++)
                {
                    if (BaseHelper.EqualsIgnoreCase(valueList[i], value))
                        return i;
                }

                return -1;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(_clsFullName, ex);
            }
        }

        /// <summary>
        /// Determines whether the specified ip is ip.
        /// </summary>
        /// <param name="ip">The ip.</param>
        /// <returns>
        ///   <c>true</c> if the specified ip is ip; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsIP(string ip)
        {
            try
            {
                return Regex.IsMatch(ip, @"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)$");
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(_clsFullName, ex);
            }
        }

        /// <summary>
        /// Passwords the equals.
        /// </summary>
        /// <param name="b1">The b1.</param>
        /// <param name="b2">The b2.</param>
        /// <returns></returns>
        public static bool PasswordEquals(byte[] b1, byte[] b2)
        {
            try
            {
                if (b1.Length != b2.Length) return false;
                if (b1 == null || b2 == null) return false;
                for (int i = 0; i < b1.Length; i++)
                    if (b1[i] != b2[i])
                        return false;
                return true;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(_clsFullName, ex);
            }
        }

        /// <summary>
        /// Gets the new identifier.
        /// </summary>
        /// <returns></returns>
        public static string GetNewGuid()
        {
            try
            {
                return Guid.NewGuid().ToString();
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(_clsFullName, ex);
            }
        }

        /// <summary>
        /// Adds the items to list.
        /// </summary>
        /// <param name="sourceList">The source list.</param>
        /// <param name="addItems">The add items.</param>
        public static void AddItemsToList(List<string> sourceList, List<string> addItems)
        {
            try
            {
                foreach (string item in addItems)
                {
                    if (sourceList.Contains(item))
                        continue;

                    sourceList.Add(item);
                }
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(_clsFullName, ex);
            }
        }

        /// <summary>
        /// HTTPs the file stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="attachment">if set to <c>true</c> [attachment].</param>
        /// <returns></returns>
        public static HttpResponseMessage HttpFileStream(MemoryStream stream, string fileName, bool attachment = true)
        {
            try
            {
                var ext = BaseHelper.GetFileExtension(fileName);
                var contentType = BaseHelper.GetContentTypeByExtension(ext);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new ByteArrayContent(stream.ToArray());
                response.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
                if (attachment)
                {
                    response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                    {
                        FileName = fileName
                    };
                }
                return response;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(_clsFullName, ex);
            }
        }

        /// <summary>
        /// Gets the image stream.
        /// </summary>
        /// <param name="imageBase64String">The image base64 string.</param>
        /// <returns></returns>
        /// <exception cref="Exception">imagedata format not data:image/png;base64,</exception>
        public static MemoryStream GetImageStream(string imageBase64String)
        {
            try
            {
                //data:image/png;base64,
                if (imageBase64String.StartsWith("data:image/png;base64,") == false)
                {
                    throw new Exception("imagedata format not data:image/png;base64,");
                }

                byte[] imageBytes = Convert.FromBase64String(imageBase64String.Substring("data:image/png;base64,".Length));
                return new MemoryStream(imageBytes);
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(_clsFullName, ex);
            }
        }

        /// <summary>
        /// Gets the width of the text.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="fontName">Name of the font.</param>
        /// <param name="fontSize">Size of the font.</param>
        /// <returns></returns>
        public static double GetTextGraphicsWidth(string content, string fontName, int fontSize)
        {
            try
            {
                var graphics = Graphics.FromHwndInternal(IntPtr.Zero);
                var textSize = graphics.MeasureString(content, new System.Drawing.Font(fontName, fontSize));
                double width = (double)(((textSize.Width / (double)7) * 256) - (128 / 7)) / 256;
                return (double)decimal.Round((decimal)width + 0.2M, 2);
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(_clsFullName, ex);
            }
        }

        /// <summary>
        /// Gets the excel col key.
        /// </summary>
        /// <param name="colIndex">Index of the col.</param>
        /// <returns></returns>
        public static string GetExcelColKey(uint colIndex)
        {
            try
            {
                var col1 = colIndex / 26;
                var col2 = colIndex % 26;
                if (col2 == 0)
                {
                    col2 = 26;
                    if (col1 > 0)
                    {
                        col1--;
                    }
                }

                var colKey1 = col1 > 0 ? Convert.ToString((char)(col1 + 64)) : string.Empty;
                var colKey2 = Convert.ToString((char)(col2 + 64));
                return colKey1 + colKey2;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(_clsFullName, ex);
            }
        }

        /// <summary>
        /// Gets the index of the excel col.
        /// </summary>
        /// <param name="colKey">The col key.</param>
        /// <returns></returns>
        public static uint GetExcelColIndex(string colKey)
        {
            try
            {
                var keys = colKey.ToCharArray();
                if (keys.Length == 2)
                    return ((uint)keys[0] - 64) * 26 + (uint)keys[1] - 64;

                if (keys.Length == 1)
                    return (uint)keys[0] - 64;

                return 0;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(_clsFullName, ex);
            }
        }

        /// <summary>
        /// Gets the search pattern.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="extList">The ext list.</param>
        /// <returns></returns>
        public static List<string> GetFiles(string path, string[] extList = null)
        {
            try
            {
                var arrData = new List<string>();
                if (extList == null)
                    arrData.AddRange(Directory.GetFiles(path));
                else
                {
                    foreach (var ext in extList)
                    {
                        arrData.AddRange(Directory.GetFiles(path, $"*{ext}"));
                    }
                }
                return arrData;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(_clsFullName, ex);
            }
        }

        /// <summary>
        /// Gets the assemply version.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <returns></returns>
        public static string GetAssemplyVersion(Assembly assembly)
        {
            try
            {
                var assemblyInformationalVersion = string.Empty;
                var assemblyTitle = string.Empty;
                foreach (var attr in assembly.CustomAttributes)
                {
                    switch (attr.AttributeType.Name)
                    {
                        case "AssemblyInformationalVersionAttribute":
                            assemblyInformationalVersion = Convert.ToString(attr.ConstructorArguments[0].Value);
                            break;
                        case "AssemblyTitleAttribute":
                            assemblyTitle = Convert.ToString(attr.ConstructorArguments[0].Value);
                            break;
                    }
                }

                return $"{assemblyTitle} {assemblyInformationalVersion}";
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(_clsFullName, ex);
            }
        }

        /// <summary>
        /// Creates the transaction scope.
        /// </summary>
        /// <param name="scopeOption">The scope option.</param>
        /// <param name="isolationLevel">The isolation level.</param>
        /// <returns></returns>
        public static TransactionScope CreateTransactionScope(TransactionScopeOption scopeOption = TransactionScopeOption.Required, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            try
            {
                return new TransactionScope(scopeOption, new TransactionOptions() { IsolationLevel = isolationLevel });
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(_clsFullName, ex);
            }
        }

        /// <summary>
        /// Froms the unix time stamp.
        /// </summary>
        /// <param name="unixTimeStamp">The unix time stamp.</param>
        /// <returns></returns>
        public static DateTime FromUnixTimeStamp(double unixTimeStamp)
        {
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            return dtDateTime.AddSeconds(unixTimeStamp);
        }

        /// <summary>
        /// Converts to unixtimestamp.
        /// </summary>
        /// <param name="dtDateTime">The dt date time.</param>
        /// <returns></returns>
        public static double ToUnixTimeStamp(DateTime dtDateTime)
        {
            var date = dtDateTime.ToUniversalTime().Subtract(new DateTime(1970, 1, 1));
            return Math.Round(date.TotalSeconds, 1);
        }

        /// <summary>
        /// Deserializes the object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <param name="formatting">The formatting.</param>
        /// <param name="isCamelCase">if set to <c>true</c> [is camel case].</param>
        /// <returns></returns>
        public static T DeserializeObject<T>(string value, Newtonsoft.Json.Formatting formatting = Newtonsoft.Json.Formatting.Indented, bool isCamelCase = false)
        {
            var setting = new JsonSerializerSettings
            {
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                Formatting = formatting
            };

            if (isCamelCase)
                setting.ContractResolver = new CamelCasePropertyNamesContractResolver();

            return JsonConvert.DeserializeObject<T>(value, setting);
        }

        /// <summary>
        /// Serializes the object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="formatting">The formatting.</param>
        /// <param name="isCamelCase">if set to <c>true</c> [is camel case].</param>
        /// <returns></returns>
        public static string SerializeObject(object obj, Newtonsoft.Json.Formatting formatting = Newtonsoft.Json.Formatting.Indented, bool isCamelCase = false)
        {
            var setting = new JsonSerializerSettings
            {
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                Formatting = formatting
            };

            if (isCamelCase)
                setting.ContractResolver = new CamelCasePropertyNamesContractResolver();

            return JsonConvert.SerializeObject(obj, setting);
        }
        #endregion

        #region Join Split Methods
        /// <summary>
        /// Splits the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="split">The split.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        public static List<string> Split(string value, string split = ",", StringSplitOptions options = StringSplitOptions.RemoveEmptyEntries)
        {
            if (string.IsNullOrEmpty(value))
                return new List<string>();

            var arrTemp = value.Split(new string[] { split }, options);
            return arrTemp.ToList();
        }

        /// <summary>
        /// Joins the specified list.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="seperator">The seperator.</param>
        /// <returns></returns>
        public static string Join(List<int> list, string seperator)
        {
            try
            {
                if (list == null)
                    return string.Empty;

                if (list.Count == 0)
                    return string.Empty;

                return string.Join(seperator, list);
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(_clsFullName, ex);
            }
        }

        /// <summary>
        /// Joins the specified list.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="seperator">The seperator.</param>
        /// <returns></returns>
        public static string Join(List<string> list, string seperator)
        {
            try
            {
                if (list == null)
                    return string.Empty;

                if (list.Count == 0)
                    return string.Empty;

                return string.Join(seperator, list);
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(_clsFullName, ex);
            }
        }

        /// <summary>
        /// Combines the emails.
        /// </summary>
        /// <param name="email1">The email1.</param>
        /// <param name="email2">The email2.</param>
        /// <returns></returns>
        public static string CombineEmails(string email1, string email2)
        {
            try
            {
                var emailList = new List<string>();
                var arrEmail1 = BaseHelper.Split(email1, ";");
                var arrEmail2 = BaseHelper.Split(email2, ";");
                foreach (var item in arrEmail1)
                {
                    if (BaseHelper.ContainIgnoreCase(emailList, item) == false)
                        emailList.Add(item);
                }

                foreach (var item in arrEmail2)
                {
                    if (BaseHelper.ContainIgnoreCase(emailList, item) == false)
                        emailList.Add(item);
                }
                return BaseHelper.Join(emailList, ";");
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(_clsFullName, ex);
            }
        }

        #endregion

        #region File Name Methods
        /// <summary>
        /// Gets the file extension.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        public static string GetFileExtension(string fileName)
        {
            try
            {
                var index = fileName.LastIndexOf(".");
                if (index >= 0)
                    return fileName.Substring(fileName.LastIndexOf("."));

                return string.Empty;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(_clsFullName, ex);
            }
        }

        /// <summary>
        /// Determines whether [has file extension] [the specified file path].
        /// </summary>
        /// <param name="fileName">The file name.</param>
        /// <param name="extList">The ext list.</param>
        /// <returns>
        ///   <c>true</c> if [has file extension] [the specified file path]; otherwise, <c>false</c>.
        /// </returns>
        public static bool HasFileExtension(string fileName, List<string> extList)
        {
            try
            {
                foreach (var ext in extList)
                {
                    if (fileName.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(_clsFullName, ex);
            }
        }

        /// <summary>
        /// Splits the name of the file. 
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="name">The name.</param>
        /// <param name="ext">The ext.</param>
        public static void SplitFileName(string fileName, out string name, out string ext)
        {
            try
            {
                var index = fileName.LastIndexOf(".");
                if (index > 0)
                {
                    ext = fileName.Substring(index);
                    name = fileName.Substring(0, index);
                }
                else if (index < 0)
                {
                    //none ext
                    name = fileName;
                    ext = string.Empty;
                }
                else
                {
                    //none name
                    name = string.Empty;
                    ext = fileName;
                }
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(_clsFullName, ex);
            }
        }

        /// <summary>
        /// Gets the name of the file.
        /// </summary>
        /// <param name="fileFullName">Full name of the file.</param>
        /// <returns></returns>
        public static string GetFileName(string fileFullName)
        {
            try
            {
                var fileName = fileFullName;
                var index = fileName.LastIndexOf("/");
                if (index > 0)
                    fileName = fileName.Substring(index + 1);

                index = fileName.LastIndexOf("\\");
                if (index > 0)
                    fileName = fileName.Substring(index + 1);

                return fileName;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(_clsFullName, ex);
            }
        }

        /// <summary>
        /// Gets the name of the BLOB folder.
        /// </summary>
        /// <param name="blobName">Name of the BLOB. //runtime-files/Logs/BeefSolutions/</param>
        /// <returns></returns>
        public static string GetBlobFolderName(string blobName)
        {
            try
            {
                var temp = BaseHelper.RemoveFirstChar(blobName);
                var index = temp.LastIndexOf("/");
                if (index > 0)
                    return temp.Substring(index + 1);

                return temp;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(_clsFullName, ex);
            }
        }

        /// <summary>
        /// Gets the content type by extension.
        /// </summary>
        /// <param name="extension">The extension.</param>
        /// <returns></returns>
        public static string GetContentTypeByExtension(string extension)
        {
            switch (extension.ToLower())
            {
                case ".323": return "text/h323";
                case ".3g2": return "video/3gpp2";
                case ".3gp2": return "video/3gpp2";
                case ".3gp": return "video/3gpp";
                case ".3gpp": return "video/3gpp";
                case ".aac": return "audio/aac";
                case ".aaf": return "application/octet-stream";
                case ".aca": return "application/octet-stream";
                case ".accdb": return "application/msaccess";
                case ".accde": return "application/msaccess";
                case ".accdt": return "application/msaccess";
                case ".acx": return "application/internet-property-stream";
                case ".adt": return "audio/vnd.dlna.adts";
                case ".adts": return "audio/vnd.dlna.adts";
                case ".afm": return "application/octet-stream";
                case ".ai": return "application/postscript";
                case ".aif": return "audio/x-aiff";
                case ".aifc": return "audio/aiff";
                case ".aiff": return "audio/aiff";
                case ".appcache": return "text/cache-manifest";
                case ".application": return "application/x-ms-application";
                case ".art": return "image/x-jg";
                case ".asd": return "application/octet-stream";
                case ".asf": return "video/x-ms-asf";
                case ".asi": return "application/octet-stream";
                case ".asm": return "text/plain";
                case ".asr": return "video/x-ms-asf";
                case ".asx": return "video/x-ms-asf";
                case ".atom": return "application/atom+xml";
                case ".au": return "audio/basic";
                case ".avi": return "video/x-msvideo";
                case ".axs": return "application/olescript";
                case ".bas": return "text/plain";
                case ".bcpio": return "application/x-bcpio";
                case ".bin": return "application/octet-stream";
                case ".bmp": return "image/bmp";
                case ".c": return "text/plain";
                case ".cab": return "application/vnd.ms-cab-compressed";
                case ".calx": return "application/vnd.ms-office.calx";
                case ".cat": return "application/vnd.ms-pki.seccat";
                case ".cdf": return "application/x-cdf";
                case ".chm": return "application/octet-stream";
                case ".class": return "application/x-java-applet";
                case ".clp": return "application/x-msclip";
                case ".cmx": return "image/x-cmx";
                case ".cnf": return "text/plain";
                case ".config": return "application/xml";
                case ".cod": return "image/cis-cod";
                case ".cpio": return "application/x-cpio";
                case ".cpp": return "text/plain";
                case ".crd": return "application/x-mscardfile";
                case ".crl": return "application/pkix-crl";
                case ".crt": return "application/x-x509-ca-cert";
                case ".csh": return "application/x-csh";
                case ".css": return "text/css";
                case ".csv": return "application/octet-stream";
                case ".cur": return "application/octet-stream";
                case ".dcr": return "application/x-director";
                case ".deploy": return "application/octet-stream";
                case ".der": return "application/x-x509-ca-cert";
                case ".dib": return "image/bmp";
                case ".dir": return "application/x-director";
                case ".disco": return "text/xml";
                case ".dlm": return "text/dlm";
                case ".doc": return "application/msword";
                case ".docm": return "application/vnd.ms-word.document.macroEnabled.12";
                case ".docx": return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                case ".dot": return "application/msword";
                case ".dotm": return "application/vnd.ms-word.template.macroEnabled.12";
                case ".dotx": return "application/vnd.openxmlformats-officedocument.wordprocessingml.template";
                case ".dsp": return "application/octet-stream";
                case ".dtd": return "text/xml";
                case ".dvi": return "application/x-dvi";
                case ".dvr-ms": return "video/x-ms-dvr";
                case ".dwf": return "drawing/x-dwf";
                case ".dwp": return "application/octet-stream";
                case ".dxr": return "application/x-director";
                case ".eml": return "message/rfc822";
                case ".emz": return "application/octet-stream";
                case ".eot": return "application/vnd.ms-fontobject";
                case ".eps": return "application/postscript";
                case ".etx": return "text/x-setext";
                case ".evy": return "application/envoy";
                case ".fdf": return "application/vnd.fdf";
                case ".fif": return "application/fractals";
                case ".fla": return "application/octet-stream";
                case ".flr": return "x-world/x-vrml";
                case ".flv": return "video/x-flv";
                case ".gif": return "image/gif";
                case ".gtar": return "application/x-gtar";
                case ".gz": return "application/x-gzip";
                case ".h": return "text/plain";
                case ".hdf": return "application/x-hdf";
                case ".hdml": return "text/x-hdml";
                case ".hhc": return "application/x-oleobject";
                case ".hhk": return "application/octet-stream";
                case ".hhp": return "application/octet-stream";
                case ".hlp": return "application/winhlp";
                case ".hqx": return "application/mac-binhex40";
                case ".hta": return "application/hta";
                case ".htc": return "text/x-component";
                case ".htm": return "text/html";
                case ".html": return "text/html";
                case ".htt": return "text/webviewhtml";
                case ".hxt": return "text/html";
                case ".ical": return "text/calendar";
                case ".icalendar": return "text/calendar";
                case ".ico": return "image/x-icon";
                case ".ics": return "text/calendar";
                case ".ief": return "image/ief";
                case ".ifb": return "text/calendar";
                case ".iii": return "application/x-iphone";
                case ".inf": return "application/octet-stream";
                case ".ins": return "application/x-internet-signup";
                case ".isp": return "application/x-internet-signup";
                case ".IVF": return "video/x-ivf";
                case ".jar": return "application/java-archive";
                case ".java": return "application/octet-stream";
                case ".jck": return "application/liquidmotion";
                case ".jcz": return "application/liquidmotion";
                case ".jfif": return "image/pjpeg";
                case ".jpb": return "application/octet-stream";
                case ".jpe": return "image/jpeg";
                case ".jpeg": return "image/jpeg";
                case ".jpg": return "image/jpeg";
                case ".js": return "application/javascript";
                case ".json": return "application/json";
                case ".jsx": return "text/jscript";
                case ".latex": return "application/x-latex";
                case ".lit": return "application/x-ms-reader";
                case ".lpk": return "application/octet-stream";
                case ".lsf": return "video/x-la-asf";
                case ".lsx": return "video/x-la-asf";
                case ".lzh": return "application/octet-stream";
                case ".m13": return "application/x-msmediaview";
                case ".m14": return "application/x-msmediaview";
                case ".m1v": return "video/mpeg";
                case ".m2ts": return "video/vnd.dlna.mpeg-tts";
                case ".m3u": return "audio/x-mpegurl";
                case ".m4a": return "audio/mp4";
                case ".m4v": return "video/mp4";
                case ".man": return "application/x-troff-man";
                case ".manifest": return "application/x-ms-manifest";
                case ".map": return "text/plain";
                case ".markdown": return "text/markdown";
                case ".md": return "text/markdown";
                case ".mdb": return "application/x-msaccess";
                case ".mdp": return "application/octet-stream";
                case ".me": return "application/x-troff-me";
                case ".mht": return "message/rfc822";
                case ".mhtml": return "message/rfc822";
                case ".mid": return "audio/mid";
                case ".midi": return "audio/mid";
                case ".mix": return "application/octet-stream";
                case ".mmf": return "application/x-smaf";
                case ".mno": return "text/xml";
                case ".mny": return "application/x-msmoney";
                case ".mov": return "video/quicktime";
                case ".movie": return "video/x-sgi-movie";
                case ".mp2": return "video/mpeg";
                case ".mp3": return "audio/mpeg";
                case ".mp4": return "video/mp4";
                case ".mp4v": return "video/mp4";
                case ".mpa": return "video/mpeg";
                case ".mpe": return "video/mpeg";
                case ".mpeg": return "video/mpeg";
                case ".mpg": return "video/mpeg";
                case ".mpp": return "application/vnd.ms-project";
                case ".mpv2": return "video/mpeg";
                case ".ms": return "application/x-troff-ms";
                case ".msi": return "application/octet-stream";
                case ".mso": return "application/octet-stream";
                case ".mvb": return "application/x-msmediaview";
                case ".mvc": return "application/x-miva-compiled";
                case ".nc": return "application/x-netcdf";
                case ".nsc": return "video/x-ms-asf";
                case ".nws": return "message/rfc822";
                case ".ocx": return "application/octet-stream";
                case ".oda": return "application/oda";
                case ".odc": return "text/x-ms-odc";
                case ".ods": return "application/oleobject";
                case ".oga": return "audio/ogg";
                case ".ogg": return "video/ogg";
                case ".ogv": return "video/ogg";
                case ".ogx": return "application/ogg";
                case ".one": return "application/onenote";
                case ".onea": return "application/onenote";
                case ".onetoc": return "application/onenote";
                case ".onetoc2": return "application/onenote";
                case ".onetmp": return "application/onenote";
                case ".onepkg": return "application/onenote";
                case ".osdx": return "application/opensearchdescription+xml";
                case ".otf": return "font/otf";
                case ".p10": return "application/pkcs10";
                case ".p12": return "application/x-pkcs12";
                case ".p7b": return "application/x-pkcs7-certificates";
                case ".p7c": return "application/pkcs7-mime";
                case ".p7m": return "application/pkcs7-mime";
                case ".p7r": return "application/x-pkcs7-certreqresp";
                case ".p7s": return "application/pkcs7-signature";
                case ".pbm": return "image/x-portable-bitmap";
                case ".pcx": return "application/octet-stream";
                case ".pcz": return "application/octet-stream";
                case ".pdf": return "application/pdf";
                case ".pfb": return "application/octet-stream";
                case ".pfm": return "application/octet-stream";
                case ".pfx": return "application/x-pkcs12";
                case ".pgm": return "image/x-portable-graymap";
                case ".pko": return "application/vnd.ms-pki.pko";
                case ".pma": return "application/x-perfmon";
                case ".pmc": return "application/x-perfmon";
                case ".pml": return "application/x-perfmon";
                case ".pmr": return "application/x-perfmon";
                case ".pmw": return "application/x-perfmon";
                case ".png": return "image/png";
                case ".pnm": return "image/x-portable-anymap";
                case ".pnz": return "image/png";
                case ".pot": return "application/vnd.ms-powerpoint";
                case ".potm": return "application/vnd.ms-powerpoint.template.macroEnabled.12";
                case ".potx": return "application/vnd.openxmlformats-officedocument.presentationml.template";
                case ".ppam": return "application/vnd.ms-powerpoint.addin.macroEnabled.12";
                case ".ppm": return "image/x-portable-pixmap";
                case ".pps": return "application/vnd.ms-powerpoint";
                case ".ppsm": return "application/vnd.ms-powerpoint.slideshow.macroEnabled.12";
                case ".ppsx": return "application/vnd.openxmlformats-officedocument.presentationml.slideshow";
                case ".ppt": return "application/vnd.ms-powerpoint";
                case ".pptm": return "application/vnd.ms-powerpoint.presentation.macroEnabled.12";
                case ".pptx": return "application/vnd.openxmlformats-officedocument.presentationml.presentation";
                case ".prf": return "application/pics-rules";
                case ".prm": return "application/octet-stream";
                case ".prx": return "application/octet-stream";
                case ".ps": return "application/postscript";
                case ".psd": return "application/octet-stream";
                case ".psm": return "application/octet-stream";
                case ".psp": return "application/octet-stream";
                case ".pub": return "application/x-mspublisher";
                case ".qt": return "video/quicktime";
                case ".qtl": return "application/x-quicktimeplayer";
                case ".qxd": return "application/octet-stream";
                case ".ra": return "audio/x-pn-realaudio";
                case ".ram": return "audio/x-pn-realaudio";
                case ".rar": return "application/octet-stream";
                case ".ras": return "image/x-cmu-raster";
                case ".rf": return "image/vnd.rn-realflash";
                case ".rgb": return "image/x-rgb";
                case ".rm": return "application/vnd.rn-realmedia";
                case ".rmi": return "audio/mid";
                case ".roff": return "application/x-troff";
                case ".rpm": return "audio/x-pn-realaudio-plugin";
                case ".rtf": return "application/rtf";
                case ".rtx": return "text/richtext";
                case ".scd": return "application/x-msschedule";
                case ".sct": return "text/scriptlet";
                case ".sea": return "application/octet-stream";
                case ".setpay": return "application/set-payment-initiation";
                case ".setreg": return "application/set-registration-initiation";
                case ".sgml": return "text/sgml";
                case ".sh": return "application/x-sh";
                case ".shar": return "application/x-shar";
                case ".sit": return "application/x-stuffit";
                case ".sldm": return "application/vnd.ms-powerpoint.slide.macroEnabled.12";
                case ".sldx": return "application/vnd.openxmlformats-officedocument.presentationml.slide";
                case ".smd": return "audio/x-smd";
                case ".smi": return "application/octet-stream";
                case ".smx": return "audio/x-smd";
                case ".smz": return "audio/x-smd";
                case ".snd": return "audio/basic";
                case ".snp": return "application/octet-stream";
                case ".spc": return "application/x-pkcs7-certificates";
                case ".spl": return "application/futuresplash";
                case ".spx": return "audio/ogg";
                case ".src": return "application/x-wais-source";
                case ".ssm": return "application/streamingmedia";
                case ".sst": return "application/vnd.ms-pki.certstore";
                case ".stl": return "application/vnd.ms-pki.stl";
                case ".sv4cpio": return "application/x-sv4cpio";
                case ".sv4crc": return "application/x-sv4crc";
                case ".svg": return "image/svg+xml";
                case ".svgz": return "image/svg+xml";
                case ".swf": return "application/x-shockwave-flash";
                case ".t": return "application/x-troff";
                case ".tar": return "application/x-tar";
                case ".tcl": return "application/x-tcl";
                case ".tex": return "application/x-tex";
                case ".texi": return "application/x-texinfo";
                case ".texinfo": return "application/x-texinfo";
                case ".tgz": return "application/x-compressed";
                case ".thmx": return "application/vnd.ms-officetheme";
                case ".thn": return "application/octet-stream";
                case ".tif": return "image/tiff";
                case ".tiff": return "image/tiff";
                case ".toc": return "application/octet-stream";
                case ".tr": return "application/x-troff";
                case ".trm": return "application/x-msterminal";
                case ".ts": return "video/vnd.dlna.mpeg-tts";
                case ".tsv": return "text/tab-separated-values";
                case ".ttc": return "application/x-font-ttf";
                case ".ttf": return "application/x-font-ttf";
                case ".tts": return "video/vnd.dlna.mpeg-tts";
                case ".txt": return "text/plain";
                case ".u32": return "application/octet-stream";
                case ".uls": return "text/iuls";
                case ".ustar": return "application/x-ustar";
                case ".vbs": return "text/vbscript";
                case ".vcf": return "text/x-vcard";
                case ".vcs": return "text/plain";
                case ".vdx": return "application/vnd.ms-visio.viewer";
                case ".vml": return "text/xml";
                case ".vsd": return "application/vnd.visio";
                case ".vss": return "application/vnd.visio";
                case ".vst": return "application/vnd.visio";
                case ".vsto": return "application/x-ms-vsto";
                case ".vsw": return "application/vnd.visio";
                case ".vsx": return "application/vnd.visio";
                case ".vtx": return "application/vnd.visio";
                case ".wasm": return "application/wasm";
                case ".wav": return "audio/wav";
                case ".wax": return "audio/x-ms-wax";
                case ".wbmp": return "image/vnd.wap.wbmp";
                case ".wcm": return "application/vnd.ms-works";
                case ".wdb": return "application/vnd.ms-works";
                case ".webm": return "video/webm";
                case ".webp": return "image/webp";
                case ".wks": return "application/vnd.ms-works";
                case ".wm": return "video/x-ms-wm";
                case ".wma": return "audio/x-ms-wma";
                case ".wmd": return "application/x-ms-wmd";
                case ".wmf": return "application/x-msmetafile";
                case ".wml": return "text/vnd.wap.wml";
                case ".wmlc": return "application/vnd.wap.wmlc";
                case ".wmls": return "text/vnd.wap.wmlscript";
                case ".wmlsc": return "application/vnd.wap.wmlscriptc";
                case ".wmp": return "video/x-ms-wmp";
                case ".wmv": return "video/x-ms-wmv";
                case ".wmx": return "video/x-ms-wmx";
                case ".wmz": return "application/x-ms-wmz";
                case ".woff": return "application/font-woff";
                case ".woff2": return "font/woff2";
                case ".wps": return "application/vnd.ms-works";
                case ".wri": return "application/x-mswrite";
                case ".wrl": return "x-world/x-vrml";
                case ".wrz": return "x-world/x-vrml";
                case ".wsdl": return "text/xml";
                case ".wtv": return "video/x-ms-wtv";
                case ".wvx": return "video/x-ms-wvx";
                case ".x": return "application/directx";
                case ".xaf": return "x-world/x-vrml";
                case ".xaml": return "application/xaml+xml";
                case ".xap": return "application/x-silverlight-app";
                case ".xbap": return "application/x-ms-xbap";
                case ".xbm": return "image/x-xbitmap";
                case ".xdr": return "text/plain";
                case ".xht": return "application/xhtml+xml";
                case ".xhtml": return "application/xhtml+xml";
                case ".xla": return "application/vnd.ms-excel";
                case ".xlam": return "application/vnd.ms-excel.addin.macroEnabled.12";
                case ".xlc": return "application/vnd.ms-excel";
                case ".xlm": return "application/vnd.ms-excel";
                case ".xls": return "application/vnd.ms-excel";
                case ".xlsb": return "application/vnd.ms-excel.sheet.binary.macroEnabled.12";
                case ".xlsm": return "application/vnd.ms-excel.sheet.macroEnabled.12";
                case ".xlsx": return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                case ".xlt": return "application/vnd.ms-excel";
                case ".xltm": return "application/vnd.ms-excel.template.macroEnabled.12";
                case ".xltx": return "application/vnd.openxmlformats-officedocument.spreadsheetml.template";
                case ".xlw": return "application/vnd.ms-excel";
                case ".xml": return "text/xml";
                case ".xof": return "x-world/x-vrml";
                case ".xpm": return "image/x-xpixmap";
                case ".xps": return "application/vnd.ms-xpsdocument";
                case ".xsd": return "text/xml";
                case ".xsf": return "text/xml";
                case ".xsl": return "text/xml";
                case ".xslt": return "text/xml";
                case ".xsn": return "application/octet-stream";
                case ".xtp": return "application/octet-stream";
                case ".xwd": return "image/x-xwindowdump";
                case ".z": return "application/x-compress";
                case ".zip": return "application/x-zip-compressed";
                default: return "application/octet-stream";
            }
        }

        /// <summary>
        /// Formats the name of the file.
        /// Remove /\:*?"..
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        public static string FormatFileName(string fileName)
        {
            try
            {
                fileName = fileName.Replace(@"/", string.Empty);
                fileName = fileName.Replace(@"\", string.Empty);
                fileName = fileName.Replace(@":", string.Empty);
                fileName = fileName.Replace(@"*", string.Empty);
                fileName = fileName.Replace(@"?", string.Empty);
                fileName = fileName.Replace("\"", string.Empty);
                fileName = fileName.Replace(@"<", string.Empty);
                fileName = fileName.Replace(@">", string.Empty);
                fileName = fileName.Replace(@"|", string.Empty);
                return fileName;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(_clsFullName, ex);
            }
        }
        #endregion

        #region Path Methods

        /// <summary>
        /// Splits the azure storage full path.
        /// container/xx.zip =&gt; container,xx.zip \r\n
        /// /container/xx.zip =&gt; container,xx.zip \r\n
        /// container/yy/xx.zip =&gt; container,yy/xx.zip \r\n
        /// </summary>
        /// <param name="storageFullPath">The storage full path.</param>
        /// <param name="storageName">Name of the storage.</param>
        /// <param name="storageRelativePath">The storage path.</param>
        public static void SplitStorageFullPath(string storageFullPath, out string storageName, out string storageRelativePath)
        {
            try
            {
                storageFullPath = BaseHelper.RemoveFirstChar(storageFullPath);
                var index = storageFullPath.IndexOf("/");
                storageName = index == -1 ? storageFullPath : storageFullPath.Substring(0, index);
                storageRelativePath = index == -1 ? string.Empty : storageFullPath.Substring(storageName.Length + 1);
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(_clsFullName, ex);
            }
        }

        /// <summary>
        /// Splits the azure storage relative path.
        /// xx.zip=> dirName=empty, fileName=xx.zip \r\n
        /// yy/xx.zip ,dirName=yy, fileNmae=xx.zip \r\n
        /// /yy/xx.zip, dirName=yy, fileNmae=xx.zip \r\n
        /// </summary>
        /// <param name="shareRelativePath">The relative path. {LegacyFTP/LocalUser/xx.zip}</param>
        /// <param name="dirName">The dir path.  {LegacyFTP/LocalUser}</param>
        /// <param name="fileName">Name of the file.{xx.zip}</param>
        public static void SplitStorageRelativePath(string shareRelativePath, out string dirName, out string fileName)
        {
            try
            {
                shareRelativePath = BaseHelper.RemoveFirstChar(shareRelativePath);
                var index = shareRelativePath.LastIndexOf("/");
                dirName = shareRelativePath.Substring(0, index == -1 ? 0 : index);
                fileName = shareRelativePath.Substring(index == -1 ? 0 : index + 1);
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(_clsFullName, ex);
            }
        }

        /// <summary>
        /// Splits the file full path.
        /// d:\xx\zz.zip => dir=d:\xx  file=zz.zip
        /// </summary>
        /// <param name="fullPath">The full path.</param>
        /// <param name="dirPath">The dir path.</param>
        /// <param name="fileName">Name of the file.</param>
        public static void SplitFileFullPath(string fullPath, out string dirPath, out string fileName)
        {
            try
            {
                var index = fullPath.LastIndexOf(@"\");
                dirPath = fullPath.Substring(0, index == -1 ? 0 : index);
                fileName = fullPath.Substring(index == -1 ? 0 : index + 1);
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(_clsFullName, ex);
            }
        }

        /// <summary>
        /// Removes the before after character.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="removeLeft">if set to <c>true</c> [remove left].</param>
        /// <param name="removeRight">if set to <c>true</c> [remove right].</param>
        /// <param name="removeChar">The remove character.</param>
        /// <returns></returns>
        private static string RemoveFirstChar(string str, bool removeLeft = true, bool removeRight = true, string removeChar = "/")
        {
            var ln = removeChar.Length;
            if (string.IsNullOrEmpty(str))
                return string.Empty;

            if (removeLeft && str.Length >= ln)
            {
                if (str.Substring(0, ln) == removeChar)
                    str = str.Remove(0, ln);
            }

            if (removeRight && str.Length >= ln)
            {
                if (str.Substring(str.Length - ln, ln) == removeChar)
                    str = str.Remove(str.Length - ln);
            }

            return str;
        }

        /// <summary>
        /// Combines the storage path.
        /// [xx,yy] => xx/yy
        /// [/xx,/yy] ==> xx/yy
        /// [/xx/zz,/yy] ==> xx/zz/yy
        /// [/xx/zz/,/yy/] ==> xx/zz/yy
        /// </summary>
        /// <param name="path1">The path1.</param>
        /// <param name="path2">The path2.</param>
        /// <returns></returns>
        public static string CombineStoragePath(string path1, string path2)
        {
            try
            {
                path1 = BaseHelper.RemoveFirstChar(path1);
                path2 = BaseHelper.RemoveFirstChar(path2);
                var strPath = string.Empty;
                if (string.IsNullOrEmpty(path1) == false)
                    strPath += $"/{path1}";

                if (string.IsNullOrEmpty(path2) == false)
                    strPath += $"/{path2}";

                return BaseHelper.RemoveFirstChar(strPath, true, false);
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(_clsFullName, ex);
            }
        }

        /// <summary>
        /// Combines the storage path.
        /// [xx,yy,zz] => xx/yy/zz
        /// [/xx,/yy,/zz] => xx/yy/zz
        /// [/xx/,/yy/,/zz/] => xx/yy/zz
        /// </summary>
        /// <param name="path1">The path1.</param>
        /// <param name="path2">The path2.</param>
        /// <param name="path3">The path3.</param>
        /// <returns></returns>
        public static string CombineStoragePath(string path1, string path2, string path3)
        {
            try
            {
                path1 = BaseHelper.RemoveFirstChar(path1);
                path2 = BaseHelper.RemoveFirstChar(path2);
                path3 = BaseHelper.RemoveFirstChar(path3);
                var strPath = string.Empty;
                if (string.IsNullOrEmpty(path1) == false)
                    strPath += $"/{path1}";

                if (string.IsNullOrEmpty(path2) == false)
                    strPath += $"/{path2}";

                if (string.IsNullOrEmpty(path3) == false)
                    strPath += $"/{path3}";

                return BaseHelper.RemoveFirstChar(strPath, true, false);
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(_clsFullName, ex);
            }
        }
        #endregion

        #region Convert Methods

        /// <summary>
        /// Converts the string to base64.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        public static string ConvertStringToBase64(string str)
        {
            try
            {
                if (string.IsNullOrEmpty(str))
                    return string.Empty;

                byte[] byteArray = Encoding.UTF8.GetBytes(str);
                return Convert.ToBase64String(byteArray);
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(_clsFullName, ex);
            }
        }

        /// <summary>
        /// Converts the base64 to string.
        /// </summary>
        /// <param name="base64Str">The base64 string.</param>
        /// <returns></returns>
        public static string ConvertBase64ToString(string base64Str)
        {
            try
            {
                if (string.IsNullOrEmpty(base64Str))
                    return string.Empty;

                byte[] bytes = Convert.FromBase64String(base64Str);
                return Encoding.UTF8.GetString(bytes, 0, bytes.Length);
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(_clsFullName, ex);
            }
        }

        /// <summary>
        /// URLs the encode.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string UrlEncode(string value)
        {
            try
            {
                if (string.IsNullOrEmpty(value))
                    return string.Empty;

                return HttpUtility.UrlEncode(value.Trim());
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(_clsFullName, ex);
            }
        }

        /// <summary>
        /// URLs the decode.
        /// </summary>
        /// <param name="encodedValue">The encoded value.</param>
        /// <returns></returns>
        public static string UrlDecode(string encodedValue)
        {
            try
            {
                if (string.IsNullOrEmpty(encodedValue))
                    return string.Empty;

                return HttpUtility.UrlDecode(encodedValue.Trim());
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(_clsFullName, ex);
            }
        }

        /// <summary>
        /// URLs the path encode.
        /// </summary>
        /// <param name="url">The string.</param>
        /// <returns></returns>
        public static string UrlPathEncode(string url)
        {
            try
            {
                if (string.IsNullOrEmpty(url))
                    return string.Empty;

                return HttpUtility.UrlPathEncode(url);
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(_clsFullName, ex);
            }
        }

        /// <summary>
        /// HTMLs the encode.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string HtmlEncode(string value)
        {
            try
            {
                return HttpUtility.HtmlEncode(value.Trim());
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(_clsFullName, ex);
            }
        }

        /// <summary>
        /// HTMLs the decode.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string HtmlDecode(string value)
        {
            try
            {
                return HttpUtility.HtmlDecode(value.Trim());
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(_clsFullName, ex);
            }
        }

        /// <summary>
        /// Dates to string.
        /// </summary>
        /// <param name="dt">The dt.</param>
        /// <param name="format">The format.</param>
        /// <returns></returns>
        public static string DateToString(DateTime? dt, string format = "yyyy-MM-dd HH:mm:ss")
        {
            try
            {
                if (dt.HasValue)
                    return dt.Value.ToString(format);

                return string.Empty;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(_clsFullName, ex);
            }
        }

        /// <summary>
        /// Strings to date.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static DateTime? StringToDate(string value)
        {
            try
            {
                if (string.IsNullOrEmpty(value))
                    return null;

                return Convert.ToDateTime(value);
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(_clsFullName, ex);
            }
        }

        /// <summary>
        /// Gets the enum by value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumValue">The enum value.</param>
        /// <returns></returns>
        public static T GetEnumByValue<T>(string enumValue)
        {
            try
            {
                return (T)Enum.Parse(typeof(T), enumValue, true);
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(_clsFullName, ex);
            }
        }

        /// <summary>
        /// Converts the list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataList">The data list.</param>
        /// <param name="convertCols">The convert cols.</param>
        public static void ConvertList<T>(ref List<object[]> dataList, int[] convertCols)
        {
            try
            {
                foreach (var item in dataList)
                {
                    foreach (var col in convertCols)
                    {
                        item[col] = (T)Convert.ChangeType(item[col], typeof(T));
                    }
                }
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(_clsFullName, ex);
            }
        }
        #endregion

        #region Convert stream to Datatable
        public static async Task<System.Data.DataTable> ConvertStreamToDataTableAsync(Stream stream, dynamic schema, string format, bool convertType = false)
        {
            switch (format.ToUpper())
            {
                case "ARROW_STREAM":
                    {
                        return await ConvertArrowStreamToDataTableAsync(stream, schema, convertType);
                    }
                case "JSON":
                    {
                        return ConvertJsonStreamToDataTable(stream, schema, convertType);
                    }
                default:
                    throw BaseHelper.CreateException(_clsFullName, new NotSupportedException($"Format '{format}' is not supported."));
            }
        }

        public static async Task<System.Data.DataTable> ConvertArrowStreamToDataTableAsync(Stream arrowStream, dynamic schema, bool convertType = false)
        {
            var dt = new System.Data.DataTable();

            foreach (var col in schema.columns)
            {
                string colName = col.name;
                string typeName = col.type_name;
                Type colType = convertType ? GetDotNetType(typeName) : typeof(string);
                dt.Columns.Add(colName, colType);
            }

            using (var reader = new Apache.Arrow.Ipc.ArrowStreamReader(arrowStream))
            {
                Apache.Arrow.RecordBatch batch;
                while ((batch = await reader.ReadNextRecordBatchAsync().ConfigureAwait(false)) != null)
                {
                    AppendRecordBatchToDataTable(dt, batch, convertType);
                    batch.Dispose();
                    batch = null;
                }
                reader.Dispose();
            }

            return dt;
        }

        private static void AppendRecordBatchToDataTable(System.Data.DataTable dt, Apache.Arrow.RecordBatch batch, bool convertType)
        {
            var arrays = batch.Arrays.ToList(); // IList<IArrowArray>
            int colCount = dt.Columns.Count;
            int rowCount = batch.Length;

            for (int r = 0; r < rowCount; r++)
            {
                var values = new object[colCount];

                for (int c = 0; c < colCount; c++)
                {
                    var array = arrays[c];
                    if (array.IsNull(r))
                    {
                        values[c] = DBNull.Value;
                        continue;
                    }

                    object val = null;

                    if (!convertType)
                    {
                        val = GetStringValue(array, r);
                    }
                    else
                    {
                        val = GetTypedValue(array, r);
                    }

                    if (dt.Columns[c].DataType == typeof(string) && val != null && val != DBNull.Value)
                    {
                        val = val.ToString();
                    }

                    values[c] = val ?? DBNull.Value;
                }

                dt.Rows.Add(values);
            }
        }

        private static object GetStringValue(Apache.Arrow.IArrowArray array, int index)
        {
            switch (array)
            {
                case Apache.Arrow.StringArray sa: return sa.GetString(index);
                case Apache.Arrow.Int32Array i32a: return i32a.GetValue(index).ToString();
                case Apache.Arrow.Int64Array i64a: return i64a.GetValue(index).ToString();
                case Apache.Arrow.FloatArray fa: return fa.GetValue(index).ToString();
                case Apache.Arrow.DoubleArray da: return da.GetValue(index).ToString();
                case Apache.Arrow.BooleanArray ba: return ba.GetValue(index).ToString();
                case Apache.Arrow.TimestampArray ta: return ta.GetTimestamp(index)?.DateTime.ToString("o");
                default: return array.ToString();
            }
        }

        private static object GetTypedValue(Apache.Arrow.IArrowArray array, int index)
        {
            switch (array)
            {
                case Apache.Arrow.StringArray sa: return sa.GetString(index);
                case Apache.Arrow.Int32Array i32a: return i32a.GetValue(index);
                case Apache.Arrow.Int64Array i64a: return i64a.GetValue(index);
                case Apache.Arrow.FloatArray fa: return fa.GetValue(index);
                case Apache.Arrow.DoubleArray da: return da.GetValue(index);
                case Apache.Arrow.BooleanArray ba: return ba.GetValue(index);
                case Apache.Arrow.TimestampArray ta: return ta.GetTimestamp(index)?.DateTime;
                default: return array.ToString();
            }
        }


        public static System.Data.DataTable ConvertJsonStreamToDataTable(Stream jsonStream, dynamic schema, bool convertType = false)
        {
            var dt = new System.Data.DataTable();

            foreach (var col in schema.columns)
            {
                string colName = col.name;
                string typeName = col.type_name;
                Type colType = convertType ? GetDotNetType(typeName) : typeof(string);
                dt.Columns.Add(colName, colType);
            }

            using (var reader = new JsonTextReader(new StreamReader(jsonStream)))
            {
                while (reader.Read())
                {
                    if (reader.TokenType == JsonToken.StartArray)
                    {
                        while (reader.Read())
                        {
                            if (reader.TokenType == JsonToken.StartArray)
                            {
                                var rowToken = JArray.Load(reader);
                                var values = new object[dt.Columns.Count];

                                for (int c = 0; c < dt.Columns.Count; c++)
                                {
                                    if (rowToken.Count > c)
                                    {
                                        if (rowToken[c] != null)
                                        {
                                            values[c] = rowToken[c].ToString();
                                        }
                                        else
                                        {
                                            values[c] = DBNull.Value;
                                        }
                                    }
                                    else
                                    {
                                        values[c] = DBNull.Value;
                                    }
                                }

                                dt.Rows.Add(values);
                            }
                        }
                    }
                }
            }

            return dt;
        }

        private static Type GetDotNetType(string type)
        {
            if (string.IsNullOrEmpty(type))
                return typeof(string);

            switch (type.ToUpperInvariant())
            {
                case "STRING":
                    return typeof(string);
                case "INTEGER":
                    return typeof(int);
                case "BIGINT":
                    return typeof(long);
                case "DOUBLE":
                    return typeof(double);
                case "FLOAT":
                    return typeof(float);
                case "DECIMAL":
                    return typeof(decimal);
                case "BOOLEAN":
                    return typeof(bool);
                case "TIMESTAMP":
                    return typeof(DateTime);
                default:
                    return typeof(string);
            }
        }
        #endregion

        #region Cache Methods
        private static Dictionary<string, CacheInfo> DicCacheData = new Dictionary<string, CacheInfo>();
        /// <summary>
        /// Cleans the cache.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="isDispose">if set to <c>true</c> [is dispose].</param>
        public static void CleanCache(string key, bool isDispose = true)
        {
            if (DicCacheData.ContainsKey(key))
            {
                if (isDispose)
                {
                    var data = DicCacheData[key].Data;
                    if (data is IDisposable)
                    {
                        var disposeData = data as IDisposable;
                        disposeData.Dispose();
                    }
                }

                DicCacheData.Remove(key);
            }
        }

        /// <summary>
        /// Adds the cache.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="expireHour">The expire hour.</param>
        /// <returns></returns>
        public static string AddCache(object value, int expireHour = 1)
        {
            //clean
            var cleanKeys = new List<string>();
            foreach (var item in DicCacheData)
            {
                if (item.Value.ExpireDate < DateTime.Now)
                {
                    cleanKeys.Add(item.Key);
                }
            }

            foreach (var item in cleanKeys)
            {
                BaseHelper.CleanCache(item);
            }
            //add to cache
            var key = Guid.NewGuid().ToString();
            var cacheInfo = new CacheInfo();
            cacheInfo.Data = value;
            cacheInfo.ExpireDate = DateTime.Now.AddHours(expireHour);
            DicCacheData.Add(key, cacheInfo);
            return key;
        }

        /// <summary>
        /// Gets the cache.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public static object GetCache(string key)
        {
            if (DicCacheData.ContainsKey(key))
                return DicCacheData[key].Data;

            return null;
        }
        #endregion

        #region Cryptography
        /// <summary>
        /// Decrypts the specified encrypted string.
        /// </summary>
        /// <param name="encryptedString">The encrypted string.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public static string Decrypt(string encryptedString, string key)
        {
            try
            {
                if (string.IsNullOrEmpty(encryptedString) || string.IsNullOrEmpty(key))
                    return string.Empty;

                var inputBytes = Convert.FromBase64String(encryptedString);
                var hashmd5 = new MD5CryptoServiceProvider();
                var pwdhash = hashmd5.ComputeHash(ASCIIEncoding.ASCII.GetBytes(key));

                // Create a new TripleDES service provider 
                var tdesProvider = new TripleDESCryptoServiceProvider();
                tdesProvider.Key = pwdhash;
                tdesProvider.Mode = CipherMode.ECB;
                return ASCIIEncoding.ASCII.GetString(tdesProvider.CreateDecryptor().TransformFinalBlock(inputBytes, 0, inputBytes.Length));
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(_clsFullName, ex);
            }
        }

        /// <summary>
        /// Encrypts the specified plain text.
        /// </summary>
        /// <param name="plainText">The plain text.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public static string Encrypt(string plainText, string key)
        {
            try
            {
                if (string.IsNullOrEmpty(plainText))
                    return string.Empty;

                var inputBytes = ASCIIEncoding.ASCII.GetBytes(plainText);
                var hashmd5 = new MD5CryptoServiceProvider();
                var pwdhash = hashmd5.ComputeHash(ASCIIEncoding.ASCII.GetBytes(key));

                // Create a new TripleDES service provider 
                var tdesProvider = new TripleDESCryptoServiceProvider();
                tdesProvider.Key = pwdhash;
                tdesProvider.Mode = CipherMode.ECB;
                return Convert.ToBase64String(tdesProvider.CreateEncryptor().TransformFinalBlock(inputBytes, 0, inputBytes.Length));
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(_clsFullName, ex);
            }
        }

        /// <summary>
        /// Encrypts the password.
        /// </summary>
        /// <param name="pwd">The password.</param>
        /// <returns></returns>
        public static byte[] EncryptPWD(string pwd)
        {
            try
            {
                var len = 32;
                var encoder = new ASCIIEncoding();
                var md5Hasher = new MD5CryptoServiceProvider();
                var hashedDataBytes = md5Hasher.ComputeHash(encoder.GetBytes(pwd));
                if (hashedDataBytes.Length > len)
                {
                    var result = new byte[len];
                    Array.Copy(hashedDataBytes, result, len);
                    return result;
                }
                return hashedDataBytes;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(_clsFullName, ex);
            }
        }

        /// <summary>
        /// EncryptMD5
        /// </summary>
        /// <param name="plainText"></param>
        /// <returns></returns>
        public static string EncryptMD5(string plainText)
        {
            try
            {
                if (string.IsNullOrEmpty(plainText))
                    return string.Empty;

                using (var md5 = MD5.Create())
                {
                    var hashedPwd = md5.ComputeHash(Encoding.UTF8.GetBytes(plainText));
                    var md5Pwd = BitConverter.ToString(hashedPwd);
                    return md5Pwd.ToLower().Replace("-", "");
                }
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(_clsFullName, ex);
            }
        }

        /// <summary>
        /// Gets the regular word.
        /// </summary>
        /// <returns></returns>
        private static string GetRegularWord()
        {
            var rnd = new Random();
            var uppercaseWord = new char[26];
            //'A' to 'Z'
            for (int i = 65, j = 0; i <= 90; i++)
            {
                uppercaseWord[j] = (char)i;
                j++;
            }

            //'a' to 'z'
            var lowercaseWord = new char[26];
            for (int i = 97, j = 0; i <= 122; i++)
            {
                lowercaseWord[j] = (char)i;
                j++;
            }

            //'0' to '9'
            var numericWord = new int[10];
            for (int i = 0; i <= 9; i++)
            {
                numericWord[i] = i;
            }

            //special character
            //only use following characters when system generating random password, otherwise it might cause email send out with some issues
            var specialCharWord = new char[] { '!', '@', '#', '$', '%', '*', '(', ')', '_', '+', '-', '=' };

            var u = uppercaseWord[rnd.Next(0, 25)].ToString();
            var l = lowercaseWord[rnd.Next(0, 25)].ToString();
            var n = numericWord[rnd.Next(0, 9)].ToString();
            var s = specialCharWord[rnd.Next(0, specialCharWord.Length)].ToString();
            return u + l + n + s;
        }

        /// <summary>
        /// Gets the random word.
        /// </summary>
        /// <param name="minLen">The minimum length.</param>
        /// <param name="maxLen">The maximum length.</param>
        /// <returns></returns>
        private static string GetRandomWord(int minLen, int maxLen)
        {
            var rnd = new Random();
            var i = rnd.Next(minLen, maxLen);
            var m = 0;
            var n = 0;
            var random = new byte[i];
            for (int j = 0; j < i; j++)
            {
                m = rnd.Next(1, 6);
                if (m == 1)//digit
                {
                    n = rnd.Next(0, 9);
                    random[j] = (byte)(n + (int)'0');
                }
                else
                {
                    n = rnd.Next(0, 25);
                    m = rnd.Next(0, 1);
                    if (m == 0)
                        random[j] = (byte)(n + (int)'a');
                    else
                        random[j] = (byte)(n + (int)'A');
                }
            }
            return (new ASCIIEncoding()).GetString(random);
        }

        /// <summary>
        /// Gets the random password.
        /// </summary>
        /// <returns></returns>
        public static string GetRandomPassword()
        {
            try
            {
                return GetRegularWord() + GetRandomWord(4, 4);
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(_clsFullName, ex);
            }
        }
        #endregion

        #region Exception
        /// <summary>
        /// Retries the execute.
        /// </summary>
        /// <param name="doFun">The do fun.</param>
        /// <param name="retryWait">The retry wait seconds</param>
        /// <param name="retryCoun">The retry coun.</param>
        public static Exception ExecuteRetryThrowEx(Func<Exception> doFun, int retryWait = 5, int retryCoun = 5)
        {
            var count = 0;
            var exLast = (Exception)null;
            while (count < retryCoun)
            {
                try
                {
                    exLast = doFun();
                    if (exLast == null)
                        return null;
                }
                catch (Exception ex)
                {
                    exLast = ex;
                }
                finally
                {
                    System.Threading.Thread.Sleep(retryWait * 1000);
                    count++;
                }
            }
            return exLast;
        }

        /// <summary>
        /// Retries the execute.
        /// </summary>
        /// <param name="doAction">The do fun.</param>
        /// <param name="retryWait">The retry wait seconds.</param>
        /// <param name="retryCoun">The retry coun.</param>
        public static Exception ExecuteRetryThrowEx(Action doAction, int retryWait = 5, int retryCoun = 5)
        {
            var count = 0;
            var exLast = (Exception)null;
            while (count < retryCoun)
            {
                try
                {
                    doAction();
                    return null;
                }
                catch (Exception ex)
                {
                    exLast = ex;
                }
                finally
                {
                    System.Threading.Thread.Sleep(retryWait * 1000);
                    count++;
                }
            }
            return exLast;
        }

        /// <summary>
        /// Gets the ex message body.
        /// </summary>
        /// <param name="ex">The ex.</param>
        /// <param name="customMsg">The custom MSG.</param>
        /// <returns></returns>
        public static string GetExMessageBody(Exception ex, string customMsg = "")
        {
            var sbMsg = new StringBuilder();
            var tempEx = ex;
            while (tempEx != null)
            {
                sbMsg.AppendLine();
                sbMsg.Append(tempEx.Message);

                if (string.IsNullOrEmpty(tempEx.StackTrace) == false)
                {
                    var statckTraceList = BaseHelper.Split(ex.StackTrace, "\r\n");
                    for (var i = 0; i < statckTraceList.Count; i++)
                    {
                        sbMsg.AppendLine();
                        sbMsg.Append(statckTraceList[i]);
                    }
                }

                tempEx = tempEx.InnerException;

            }

            if (string.IsNullOrEmpty(customMsg) == false)
            {
                sbMsg.AppendLine();
                sbMsg.Append($"custom message:{customMsg}");
            }
            return sbMsg.ToString();
        }

        /// <summary>
        /// Creates the exception.
        /// </summary>
        /// <param name="className">Name of the class.</param>
        /// <param name="ex">The ex.</param>
        /// <param name="customMsg">The custom MSG.</param>
        /// <param name="memberName">Name of the member.</param>
        /// <returns></returns>
        public static Exception CreateException(string className, Exception ex, string customMsg = "", [CallerMemberName] string memberName = "")
        {
            var exBody = BaseHelper.GetExMessageBody(ex, customMsg);
            return new Exception($"{className}.{memberName} Exception:{exBody}");
        }

        #endregion

        #region Config Methods

        /// <summary>
        /// Downloads the BLOB configuration.
        /// </summary>
        /// <param name="blobConnectionString">The BLOB connection string.</param>
        /// <param name="docConfigPath">The document configuration path.</param>
        /// <param name="defKeyValutKey">The definition key valut key.</param>
        /// <param name="defKeyValutURL">The definition key valut URL.</param>
        /// <returns></returns>
        public async static Task<Dictionary<string, string>> DownloadBlobConfigAsync(string blobConnectionString, string docConfigPath, string defKeyValutKey = "", string defKeyValutURL = "")
        {
            try
            {
                //download config file
                await TrackingLogHelper.WriteTextLogAsync($"Download config file start by {docConfigPath}");
                var dicValues = new Dictionary<string, string>();
                var storageHelper = new AzureStorageHelper(blobConnectionString);
                using (var stream = await storageHelper.DownloadBlobAsync(docConfigPath))
                {
                    var configHelper = new ConfigHelper(stream);
                    configHelper.LoadAllValues(ref dicValues);
                }
                await TrackingLogHelper.WriteTextLogAsync($"Download config file end");

                //read keyvault value
                var keyVaultURL = dicValues.ContainsKey(defKeyValutKey) ? dicValues[defKeyValutKey] : defKeyValutURL;
                if (string.IsNullOrEmpty(keyVaultURL))
                    return dicValues;

                var client = new SecretClient(new Uri(keyVaultURL), new DefaultAzureCredential());
                for (var i = 0; i < dicValues.Count; i++)
                {
                    var item = dicValues.ElementAt(i);
                    if (item.Value.StartsWith("KeyVault:") == false)
                        continue;

                    var secretKey = item.Value.Replace("KeyVault:", string.Empty);
                    var secretValue = await client.GetSecretAsync(secretKey);
                    dicValues[item.Key] = secretValue.Value.Value;
                    await TrackingLogHelper.WriteTextLogAsync($"Read KeyVault Value by {secretKey}");
                }
                return dicValues;
            }
            catch (Exception ex)
            {
                TrackingLogHelper.WriteTextLog($"{MethodInfo.GetCurrentMethod().Name} ex:{ex.Message},{ex.InnerException?.Message}");
                throw BaseHelper.CreateException(_clsFullName, ex);
            }
        }
        /// <summary>
        /// Gets the key vault value.
        /// </summary>
        /// <param name="keyVaultURL">The key vault URL.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public static string GetKeyVaultValue(string keyVaultURL, string key)
        {
            try
            {
                var client = new SecretClient(new Uri(keyVaultURL), new DefaultAzureCredential());
                return client.GetSecret(key).Value.Value;
            }
            catch (Exception ex)
            {
                TrackingLogHelper.WriteTextLog($"ex:{ex.Message},{ex.InnerException?.Message}");
                throw BaseHelper.CreateException(_clsFullName, ex);
            }
            finally
            {
                TrackingLogHelper.WriteTextLog($"Read key vaule by {key}");
            }
        }

        /// <summary>
        /// Sets the key vault value.
        /// </summary>
        /// <param name="keyVaultURL">The key vault URL.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static void SetKeyVaultValue(string keyVaultURL, string key, string value)
        {
            try
            {
                var client = new SecretClient(new Uri(keyVaultURL), new DefaultAzureCredential());
                client.SetSecret(key, value);
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(_clsFullName, ex);
            }
            finally
            {
                TrackingLogHelper.WriteTextLog($"Set key vaule by {key}");
            }
        }

        /// <summary>
        /// Gets the configuration sub value list.
        /// </summary>
        /// <param name="dicConfig">The dic configuration values.</param>
        /// <param name="key">The key.</param>
        /// <param name="requiredYN">required field</param>
        /// <returns></returns>
        public static List<string> GetConfigSubValueList(Dictionary<string, string> dicConfig, string key, bool requiredYN = true)
        {
            try
            {
                if (dicConfig.ContainsKey(key) == false)
                {
                    if (requiredYN)
                    {
                        throw new Exception($"The key={key} not exist");
                    }
                    return new List<string>();
                }

                if (string.IsNullOrEmpty(dicConfig[key]))
                    return new List<string>();

                var subValueList = new List<string>();
                var keys = BaseHelper.Split(dicConfig[key]);
                foreach (string subKey in keys)
                {
                    if (dicConfig.ContainsKey(subKey) == false || string.IsNullOrEmpty(dicConfig[subKey]))
                        continue;

                    var keyValueList = BaseHelper.Split(dicConfig[subKey]);
                    foreach (string value in keyValueList)
                    {
                        if (BaseHelper.ContainIgnoreCase(subValueList, value) == false)
                            subValueList.Add(value);
                    }
                }
                return subValueList;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(_clsFullName, ex, $"Get Config Value ({key})");
            }
        }

        /// <summary>
        /// Gets the configuration value list.
        /// </summary>
        /// <param name="dicConfig">The dic configuration values.</param>
        /// <param name="key">The key.</param>
        /// <param name="split">The split.</param>
        /// <param name="requiredYN">required field</param>
        /// <returns></returns>
        public static List<string> GetConfigValueList(Dictionary<string, string> dicConfig, string key, string split = ",", bool requiredYN = true)
        {
            try
            {
                if (dicConfig.ContainsKey(key) == false)
                {
                    if (requiredYN)
                    {
                        throw new Exception($"The key={key} not exist");
                    }
                    return new List<string>();
                }

                if (string.IsNullOrEmpty(dicConfig[key]))
                    return new List<string>();

                return BaseHelper.Split(dicConfig[key], split);
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(_clsFullName, ex, $"Get Config Value ({key})");
            }
        }

        /// <summary>
        /// Gets the configuration value.
        /// </summary>
        /// <param name="dicConfig">The dic configuration.</param>
        /// <param name="key">The key.</param>
        /// <param name="requiredYN">required field</param>
        /// <returns></returns>
        public static string GetConfigValue(Dictionary<string, string> dicConfig, string key, bool requiredYN = true)
        {
            try
            {
                if (dicConfig.ContainsKey(key) == false)
                {
                    if (requiredYN)
                    {
                        throw new Exception($"The key={key} not exist");
                    }

                    return string.Empty;
                }

                return dicConfig[key];
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(_clsFullName, ex, $"Get Config Value ({key})");
            }
        }

        /// <summary>
        /// Gets the configuration value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dicConfig">The dic configuration.</param>
        /// <param name="key">The key.</param>
        /// <param name="requiredYN">required field</param>
        /// <returns></returns>
        public static T GetConfigValue<T>(Dictionary<string, string> dicConfig, string key, bool requiredYN = true)
        {
            try
            {
                if (dicConfig.ContainsKey(key) == false)
                {
                    if (requiredYN)
                    {
                        throw new Exception($"The key={key} not exist");
                    }
                    return default(T);
                }

                return (T)Convert.ChangeType(dicConfig[key], typeof(T));
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(_clsFullName, ex);
            }
        }

        /// <summary>
        /// Initializes the library schedule information.
        /// </summary>
        /// <param name="xmlDoc">The XML document.</param>
        public static void CreateEmptyConfigFile(XmlDocument xmlDoc)
        {
            try
            {
                var xmldecl = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", null);
                xmlDoc.AppendChild(xmldecl);
                xmlDoc.AppendChild(xmlDoc.CreateElement("ConfigEntities"));
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(_clsFullName, ex);
            }
        }

        /// <summary>
        /// Sets the common library schedule information.
        /// </summary>
        /// <param name="xmlDoc">The XML document.</param>
        /// <param name="toolAppName">Name of the tool application.</param>
        /// <param name="version">The version.</param>
        public static void SetConfigLibScheduleInfo(XmlDocument xmlDoc, string toolAppName, string version)
        {
            try
            {
                var node = xmlDoc.SelectSingleNode($"ConfigEntities/Entity[@appname='{toolAppName}']") as XmlElement;
                var lastDate = BaseHelper.DateToString(DateTime.Now);
                if (node == null)
                {
                    node = xmlDoc.CreateElement("Entity");
                    node.SetAttribute("appname", toolAppName);
                    node.SetAttribute("version", version);
                    node.SetAttribute("lastdate", lastDate);
                    xmlDoc.SelectSingleNode("ConfigEntities").AppendChild(node);
                }
                else
                {
                    node.SetAttribute("version", version);
                    node.SetAttribute("lastdate", lastDate);
                }
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(_clsFullName, ex);
            }
        }

        /// <summary>
        /// Sets the configuration obsolete information.
        /// </summary>
        /// <param name="xmlDoc">The XML document.</param>
        /// <param name="key">The key.</param>
        /// <param name="callerName">Name of the caller.</param>
        public static void SetConfigObsoleteInfo(XmlDocument xmlDoc, string key, string callerName)
        {
            try
            {
                var lastDate = BaseHelper.DateToString(DateTime.Now);
                var entityNode = xmlDoc.SelectSingleNode($"ConfigEntities/Entity[@key='{key}']") as XmlElement;
                var callerNode = entityNode.SelectSingleNode($"Caller[@name='{callerName}']") as XmlElement;
                if (callerNode == null)
                {
                    callerNode = xmlDoc.CreateElement("Caller");
                    callerNode.SetAttribute("name", callerName);
                    callerNode.SetAttribute("lastdate", lastDate);
                    entityNode.AppendChild(callerNode);
                }
                else
                {
                    callerNode.SetAttribute("lastdate", lastDate);
                }
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(_clsFullName, ex);
            }
        }

        #endregion

        #region Normal Distribution
        /// <summary>
        /// Given a probability, a mean, and a standard deviation, an x value can be calculated.
        /// </summary>
        /// <param name="probability">The probability.</param>
        /// <param name="mean">The mean.</param>
        /// <param name="sigma">The sigma.</param>
        /// <returns></returns>
        public static double NormInv(double probability, double mean, double sigma)
        {
            try
            {
                const double a1 = -39.6968302866538;
                const double a2 = 220.946098424521;
                const double a3 = -275.928510446969;
                const double a4 = 138.357751867269;
                const double a5 = -30.6647980661472;
                const double a6 = 2.50662827745924;

                const double b1 = -54.4760987982241;
                const double b2 = 161.585836858041;
                const double b3 = -155.698979859887;
                const double b4 = 66.8013118877197;
                const double b5 = -13.2806815528857;

                const double c1 = -7.78489400243029E-03;
                const double c2 = -0.322396458041136;
                const double c3 = -2.40075827716184;
                const double c4 = -2.54973253934373;
                const double c5 = 4.37466414146497;
                const double c6 = 2.93816398269878;

                const double d1 = 7.78469570904146E-03;
                const double d2 = 0.32246712907004;
                const double d3 = 2.445134137143;
                const double d4 = 3.75440866190742;

                //Define break-points
                // using Epsilon is wrong; see link above for reference to 0.02425 value
                //const double pLow = double.Epsilon;
                const double pLow = 0.02425;

                const double pHigh = 1 - pLow;

                //Define work variables
                double q;
                double result = 0;

                // if argument out of bounds.
                // set it to a value within desired precision.
                if (probability <= 0)
                    probability = pLow;

                if (probability >= 1)
                    probability = pHigh;

                if (probability < pLow)
                {
                    //Rational approximation for lower region
                    q = Math.Sqrt(-2 * Math.Log(probability));
                    result = (((((c1 * q + c2) * q + c3) * q + c4) * q + c5) * q + c6) / ((((d1 * q + d2) * q + d3) * q + d4) * q + 1);
                }
                else if (probability <= pHigh)
                {
                    //Rational approximation for lower region
                    q = probability - 0.5;
                    double r = q * q;
                    result = (((((a1 * r + a2) * r + a3) * r + a4) * r + a5) * r + a6) * q /
                             (((((b1 * r + b2) * r + b3) * r + b4) * r + b5) * r + 1);
                }
                else if (probability < 1)
                {
                    //Rational approximation for upper region
                    q = Math.Sqrt(-2 * Math.Log(1 - probability));
                    result = -(((((c1 * q + c2) * q + c3) * q + c4) * q + c5) * q + c6) / ((((d1 * q + d2) * q + d3) * q + d4) * q + 1);
                }

                return sigma * result + mean;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(_clsFullName, ex);
            }
        }
        #endregion

        #region HttpClient Get/Post

        /// <summary>
        /// Posts the request asynchronous.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="requestURL">The request URL.</param>
        /// <param name="requestBody">The request body.</param>
        /// <param name="timeoutSeconds">The timeout seconds.</param>
        /// <param name="dicHeader">The dic header.</param>
        /// <returns></returns>
        public static async Task<string> PostRequestAsync<T>(string requestURL, T requestBody, int timeoutSeconds = 0, Dictionary<string, string> dicHeader = null)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    if (timeoutSeconds != 0)
                        client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);

                    if (dicHeader != null)
                    {
                        foreach (var item in dicHeader)
                        {
                            client.DefaultRequestHeaders.Add(item.Key, item.Value);
                        }
                    }
                    var content = new StringContent(BaseHelper.SerializeObject(requestBody), Encoding.UTF8, "application/json");
                    var response = await client.PostAsync(requestURL, content);
                    return await response.Content.ReadAsStringAsync();
                }
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(_clsFullName, ex);
            }
        }

        /// <summary>
        /// Posts the request.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="requestURL">The request URL.</param>
        /// <param name="requestBody">The request body.</param>
        /// <param name="timeoutSeconds">The timeout seconds.</param>
        /// <param name="dicHeader">The dic header.</param>
        /// <returns></returns>
        public static string PostRequest<T>(string requestURL, T requestBody, int timeoutSeconds = 0, Dictionary<string, string> dicHeader = null)
        {
            try
            {
                var task = Task.Run(async () =>
                {
                    return await BaseHelper.PostRequestAsync<T>(requestURL, requestBody, timeoutSeconds, dicHeader);
                });
                return task.Result;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(_clsFullName, ex.InnerException ?? ex);
            }
        }

        /// <summary>
        /// Gets the request asynchronous.
        /// </summary>
        /// <param name="requestURL">The request URL.</param>
        /// <param name="timeoutSeconds">The timeout seconds.</param>
        /// <param name="dicHeader">The dic header.</param>
        /// <returns></returns>
        public static async Task<string> GetRequestAsync(string requestURL, int timeoutSeconds = 0, Dictionary<string, string> dicHeader = null)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    if (timeoutSeconds != 0)
                        client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);

                    if (dicHeader != null)
                    {
                        foreach (var item in dicHeader)
                        {
                            client.DefaultRequestHeaders.Add(item.Key, item.Value);
                        }
                    }

                    var response = await client.GetAsync(requestURL);
                    return await response.Content.ReadAsStringAsync();
                }
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(_clsFullName, ex);
            }
        }

        /// <summary>
        /// Gets the request.
        /// </summary>
        /// <param name="requestURL">The request URL.</param>
        /// <param name="timeoutSeconds">The timeout seconds.</param>
        /// <param name="dicHeader">The dic header.</param>
        /// <returns></returns>
        public static string GetRequest(string requestURL, int timeoutSeconds = 0, Dictionary<string, string> dicHeader = null)
        {
            try
            {
                var task = Task.Run(async () =>
                {
                    return await BaseHelper.GetRequestAsync(requestURL, timeoutSeconds, dicHeader);
                });
                return task.Result;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(_clsFullName, ex.InnerException ?? ex);
            }
        }
        #endregion
    }
}