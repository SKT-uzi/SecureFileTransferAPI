using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace ResumableFileTransfer.Common
{
    /// <summary>
    /// ZipHelper
    /// </summary>
    public class ZipHelper : IDisposable
    {
        private ZipArchive ZipArchive { get; set; }
        public Stream Stream { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZipHelper"/> class.
        /// </summary>
        public ZipHelper()
        {
            try
            {
                this.Stream = new MemoryStream();
                this.Stream.Seek(0, SeekOrigin.Begin);
                this.ZipArchive = new ZipArchive(this.Stream, ZipArchiveMode.Update, true);
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZipHelper"/> class.
        /// </summary>
        /// <param name="stream">The stream.</param>
        public ZipHelper(Stream stream)
        {
            try
            {
                this.Stream = stream;
                this.Stream.Seek(0, SeekOrigin.Begin);
                this.ZipArchive = new ZipArchive(stream, ZipArchiveMode.Update, true);
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZipHelper"/> class.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="zipArchiveMode">The zip archive mode.</param>
        /// <param name="leaveOpen">if set to <c>true</c> [leave open].</param>
        public ZipHelper(Stream stream, ZipArchiveMode zipArchiveMode, bool leaveOpen = true)
        {
            try
            {
                this.Stream = stream;
                this.Stream.Seek(0, SeekOrigin.Begin);
                this.ZipArchive = new ZipArchive(stream, zipArchiveMode, leaveOpen);
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Gets the zip items.
        /// </summary>
        /// <param name="ignoreFolder">if set to <c>true</c> [ignore folder].</param>
        /// <returns></returns>
        public List<string> GetZipItems(bool ignoreFolder = true)
        {
            try
            {
                var nameList = new List<string>();
                foreach (var entry in this.ZipArchive.Entries)
                {
                    if (ignoreFolder)
                    {
                        //folder
                        if (string.IsNullOrEmpty(entry.Name))
                            continue;
                    }

                    nameList.Add(entry.FullName);
                }
                return nameList;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Gets the zip files.
        /// </summary>
        /// <param name="searchExts">The search exts.</param>
        /// <returns></returns>
        public List<string> GetZipFiles(string[] searchExts = null)
        {
            try
            {
                var nameList = new List<string>();
                foreach (var entry in this.ZipArchive.Entries)
                {
                    //folder
                    if (string.IsNullOrEmpty(entry.Name))
                        continue;

                    if (searchExts == null)
                    {
                        nameList.Add(entry.FullName);
                        continue;
                    }

                    foreach (var ext in searchExts)
                    {
                        if (entry.Name.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
                        {
                            nameList.Add(entry.FullName);
                            break;
                        }
                    }
                }
                return nameList;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Gets the zip files.
        /// </summary>
        /// <param name="ext">The ext.</param>
        /// <returns></returns>
        public List<string> GetZipFiles(string ext)
        {
            try
            {
                var nameList = new List<string>();
                foreach (var entry in this.ZipArchive.Entries)
                {
                    //folder
                    if (string.IsNullOrEmpty(entry.Name))
                        continue;

                    if (entry.Name.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
                        nameList.Add(entry.FullName);
                }
                return nameList;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Gets the first file.
        /// </summary>
        /// <param name="searchExt">The search ext.</param>
        /// <param name="fullName">The full name.</param>
        /// <param name="fileName">Name of the file.</param>
        public void SearchFirstFile(string searchExt, out string fullName, out string fileName)
        {
            try
            {
                fullName = string.Empty;
                fileName = string.Empty;
                foreach (var entry in this.ZipArchive.Entries)
                {
                    //folder
                    if (string.IsNullOrEmpty(entry.Name))
                        continue;

                    if (entry.Name.EndsWith(searchExt, StringComparison.OrdinalIgnoreCase))
                    {
                        fullName = entry.FullName;
                        fileName = entry.Name;
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Searches the first file.
        /// </summary>
        /// <param name="searchExts">The search exts.</param>
        /// <param name="fullName">The full name.</param>
        /// <param name="fileName">Name of the file.</param>
        public void SearchFirstFile(string[] searchExts, out string fullName, out string fileName)
        {
            try
            {
                fullName = string.Empty;
                fileName = string.Empty;
                foreach (var entry in this.ZipArchive.Entries)
                {
                    //folder
                    if (string.IsNullOrEmpty(entry.Name))
                        continue;

                    foreach (var searchExt in searchExts)
                    {
                        if (entry.Name.EndsWith(searchExt, StringComparison.OrdinalIgnoreCase))
                        {
                            fullName = entry.FullName;
                            fileName = entry.Name;
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Valids the require files.
        ///  COWFILE*.DAT=>  ^(COWFILE(.*?)\\.DAT)$
        /// .CSV => ^(\\.CSV)$
        /// </summary>
        /// <param name="regRequireFiles">The reg require files.</param>
        /// <returns></returns>
        public bool ValidRequireFiles(List<string> regRequireFiles)
        {
            try
            {
                foreach (var requiredFile in regRequireFiles)
                {
                    var temp = requiredFile.Replace(".", "\\.").Replace("*", "(.*?)");
                    var regex = new Regex($"^({temp})$", RegexOptions.IgnoreCase);
                    var isExists = false;
                    foreach (var entry in this.ZipArchive.Entries)
                    {
                        if (string.IsNullOrEmpty(entry.Name))
                            continue;

                        if (regex.Match(entry.Name).Success)
                            isExists = true;
                    }

                    if (isExists == false)
                        return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Moves the sub folder fileto root.
        /// </summary>
        public void MoveSubFolderFiletoRoot()
        {
            try
            {
                var deleteList = new List<string>();
                for (var i = this.ZipArchive.Entries.Count - 1; i > 0; i--)
                {
                    var entry = this.ZipArchive.Entries[i];
                    //folder
                    if (string.IsNullOrEmpty(entry.Name))
                    {
                        deleteList.Add(entry.FullName);
                        continue;
                    }

                    //sub file
                    if (entry.FullName.IndexOf("/") > 0)
                    {
                        var newFileName = DateTime.Now.ToString("MMddyyyyhhmmss") + "_" + entry.Name;
                        using (var stream = entry.Open())
                        {
                            this.AddContent(newFileName, stream);
                            deleteList.Add(entry.FullName);
                        }
                    }
                }
                foreach (var item in deleteList)
                {
                    this.DeleteFile(item);
                }
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Appends the text.
        /// </summary>
        /// <param name="relativePath">The file path.</param>
        /// <param name="content">The content.</param>
        public void AppendText(string relativePath, string content)
        {
            try
            {
                var entry = this.ZipArchive.GetEntry(relativePath);
                if (entry == null)
                {
                    entry = this.ZipArchive.CreateEntry(relativePath);
                }
                using (var writer = new StreamWriter(entry.Open()))
                {
                    writer.WriteLine(content);
                }
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Adds the folder.
        /// </summary>
        /// <param name="relativePath">The relative path.</param>
        public void AddFolder(string relativePath)
        {
            try
            {
                var lastChar = relativePath.Substring(relativePath.Length - 1);
                this.ZipArchive.CreateEntry(lastChar == "/" ? relativePath : $"{relativePath}/");
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Adds the content.
        /// </summary>
        /// <param name="relativePath">The relative path.</param>
        /// <param name="stream">The stream.</param>
        public void AddContent(string relativePath, Stream stream)
        {
            try
            {
                var currentArchiveEntry = this.ZipArchive.CreateEntry(relativePath);
                var buffer = new byte[BaseHelper.BufferSize_1MB];
                int read;
                using (var streamEntry = currentArchiveEntry.Open())
                {
                    while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        streamEntry.Write(buffer, 0, read);
                    }
                }
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex, ex.StackTrace);
            }
        }

        /// <summary>
        /// Reads the file.
        /// </summary>
        /// <param name="relativePath">The relative path.</param>
        /// <returns></returns>
        public Stream ReadFile(string relativePath)
        {
            try
            {
                var entry = this.ZipArchive.GetEntry(relativePath);
                return entry.Open();
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Deletes the file.
        /// </summary>
        /// <param name="relativePath">The relative path.</param>
        /// <returns></returns>
        public void DeleteFile(string relativePath)
        {
            try
            {
                var entry = this.ZipArchive.GetEntry(relativePath);
                entry.Delete();
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Flushes this instance.
        /// </summary>
        public void Flush()
        {
            try
            {
                this.ZipArchive.Dispose();
                this.Stream.Seek(0, SeekOrigin.Begin);
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Flushes this instance.
        /// </summary>
        public void Dispose()
        {
            try
            {
                this.ZipArchive.Dispose();
                this.ZipArchive = null;
                this.Stream.Seek(0, SeekOrigin.Begin);
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }
    }
}
