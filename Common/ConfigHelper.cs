using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace ResumableFileTransfer.Common
{
    /// <summary>
    /// ConfigHelper
    /// </summary>
    public class ConfigHelper
    {
        #region Constructors
        private XmlDocument Document { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigHelper" /> class.
        /// </summary>
        /// <param name="stream">The stream.</param>
        public ConfigHelper(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            this.Document = new XmlDocument();
            this.Document.Load(stream);
        }

        #endregion

        #region Private Methods
        /// <summary>
        /// Gets the format node path.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        private string GetFormatNodePath(string key)
        {
            return $"ConfigEntities/Entity[@key='{key}']";
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Exists the key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public bool ExistKey(string key)
        {
            try
            {
                var nodePath = this.GetFormatNodePath(key);
                return this.Document.SelectSingleNode(nodePath) != null;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Get Value By Key
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>Key value</returns>
        public string GetValueByKey(string key)
        {
            try
            {
                var nodePath = this.GetFormatNodePath(key);
                var node = this.Document.SelectSingleNode(nodePath) as XmlElement;
                if (node == null)
                    return string.Empty;

                return node.GetAttribute("value").Trim();
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Gets all values.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> GetAllValues()
        {
            try
            {
                var dicValues = new Dictionary<string, string>();
                var nodeList = this.Document.SelectNodes("ConfigEntities/Entity");
                foreach (XmlElement node in nodeList)
                {
                    var key = node.GetAttribute("key");
                    var value = node.GetAttribute("value");
                    if (dicValues.ContainsKey(key) == false)
                        dicValues.Add(key, value);
                    else
                        dicValues[key] = value;
                }
                return dicValues;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }


        /// <summary>
        /// Loads all values.
        /// </summary>
        /// <param name="dicValues">The dic values.</param>
        public void LoadAllValues(ref ConcurrentDictionary<string, string> dicValues)
        {
            try
            {
                var nodeList = this.Document.SelectNodes("ConfigEntities/Entity");
                foreach (XmlElement node in nodeList)
                {
                    var key = node.GetAttribute("key");
                    var value = node.GetAttribute("value");
                    if (dicValues.ContainsKey(key) == false)
                        dicValues.TryAdd(key, value);
                    else
                        dicValues[key] = value;
                }
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Loads all values.
        /// </summary>
        /// <param name="dicValues">The dic values.</param>
        public void LoadAllValues(ref Dictionary<string, string> dicValues)
        {
            try
            {
                var nodeList = this.Document.SelectNodes("ConfigEntities/Entity");
                foreach (XmlElement node in nodeList)
                {
                    var key = node.GetAttribute("key");
                    var value = node.GetAttribute("value");
                    if (dicValues.ContainsKey(key) == false)
                        dicValues.Add(key, value);
                    else
                        dicValues[key] = value;
                }
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Set Value By Key
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void SetValueByKey(string key, string value)
        {
            try
            {
                var nodePath = this.GetFormatNodePath(key);
                var node = this.Document.SelectSingleNode(nodePath) as XmlElement;
                node.SetAttribute("value", value);
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Adds the value by key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void AddValueByKey(string key, string value)
        {
            try
            {
                var rootNode = this.Document.SelectSingleNode("ConfigEntities");
                var node = this.Document.CreateElement("Entity");
                node.SetAttribute("key", key);
                node.SetAttribute("value", value);
                rootNode.AppendChild(node);
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Gets the stream data.
        /// </summary>
        /// <returns></returns>
        public Stream GetStreamData()
        {
            try
            {
                var stream = new MemoryStream();
                this.Document.Save(stream);
                return stream;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }
        #endregion
    }
}