using System;
using System.Collections.Generic;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace ResumableFileTransfer.Common
{
    public class PDFHelper
    {
        #region Private member Avaraibles
        private Document _document = null;
        private PdfSmartCopy _pdfCopy = null;
        #endregion

        #region Public Methods

        /// <summary>
        /// Creates the PDF.
        /// </summary>
        /// <returns></returns>
        public MemoryStream CreatePDF()
        {
            try
            {
                var stream = new MemoryStream();
                this._document = new Document();
                this._pdfCopy = new PdfSmartCopy(this._document, stream);
                this._document.Open();
                return stream;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Closes this instance.
        /// </summary>
        public void Close()
        {
            try
            {
                this._pdfCopy.Close();
                this._document.Close();
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Fills the text page.
        /// </summary>
        /// <param name="template">The template.</param>
        /// <param name="title">The title.</param>
        /// <param name="pager">The pager.</param>
        public void FillPage(Stream template, string title = "", string pager = "")
        {
            try
            {
                var dicValues = new Dictionary<string, string>();
                if (string.IsNullOrEmpty(title))
                    dicValues.Add("Title", title);

                if (string.IsNullOrEmpty(pager) == false)
                    dicValues.Add("Pager", pager);

                using (var pageStream = this.GetTemplatePage(template, dicValues))
                {
                    this.AddPDFPage(pageStream);
                }
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Fills the page.
        /// </summary>
        /// <param name="template">The template.</param>
        /// <param name="pdfImg">The PDF img.</param>
        /// <param name="title">The title.</param>
        /// <param name="pager">The pager.</param>
        public void FillPage(Stream template, XPDFImage pdfImg, string title = "", string pager = "")
        {
            try
            {
                var dicValues = new Dictionary<string, string>();
                if (string.IsNullOrEmpty(title) == false)
                    dicValues.Add("Title", title);

                if (string.IsNullOrEmpty(pager) == false)
                    dicValues.Add("Pager", pager);

                using (var pageStream = this.GetTemplatePage(template, dicValues, pdfImg))
                {
                    this.AddPDFPage(pageStream);
                }
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Fills the page.
        /// </summary>
        /// <param name="template">The template.</param>
        /// <param name="dicFieldValues">The dic field values.</param>
        public void FillTextPage(Stream template, Dictionary<string, string> dicFieldValues = null)
        {
            try
            {
                var dicValues = new Dictionary<string, string>();
                if (dicFieldValues != null)
                {
                    foreach (var item in dicFieldValues)
                    {
                        if (dicValues.ContainsKey(item.Key) == false)
                            dicValues.Add(item.Key, item.Value);
                    }
                }

                using (var pageStream = this.GetTemplatePage(template, dicValues))
                {
                    this.AddPDFPage(pageStream);
                }
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Fills the page.
        /// </summary>
        /// <param name="template">The template.</param>
        /// <param name="pdfImg">The PDF img.</param>
        /// <param name="dicFieldValues">The dic field values.</param>
        public void FillImagePage(Stream template, XPDFImage pdfImg, Dictionary<string, string> dicFieldValues = null)
        {
            try
            {
                var dicValues = new Dictionary<string, string>();
                if (dicFieldValues != null)
                {
                    foreach (var item in dicFieldValues)
                    {
                        if (dicValues.ContainsKey(item.Key) == false)
                            dicValues.Add(item.Key, item.Value);
                    }
                }
                using (var pageStream = this.GetTemplatePage(template, dicValues, pdfImg))
                {
                    this.AddPDFPage(pageStream);
                }
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Creates the template page stream.
        /// </summary>
        /// <param name="template">The template.</param>
        /// <param name="dicFileValues">The dic file values.</param>
        /// <param name="pdfImg">The PDF img.</param>
        /// <returns></returns>
        /// <exception cref="Exception">imagedata format not data:image/png;base64,</exception>
        private Stream GetTemplatePage(Stream template, Dictionary<string, string> dicFileValues, XPDFImage pdfImg = null)
        {
            template.Seek(0, SeekOrigin.Begin);
            var stream = new MemoryStream();
            var pdfReader = new PdfReader(template);
            var pdfStamper = new PdfStamper(pdfReader, stream);
            try
            {
                //add text set not edit FormFlattening=true
                var pdfFormFields = pdfStamper.AcroFields;
                pdfStamper.FormFlattening = true;
                foreach (KeyValuePair<string, string> field in dicFileValues)
                {
                    if (pdfFormFields.Fields[field.Key] != null)
                    {
                        pdfFormFields.SetField(field.Key, field.Value);
                    }
                }

                //add images
                if (pdfImg != null)
                {
                    using (var streamImg = BaseHelper.GetImageStream(pdfImg.Img64String))
                    {
                        streamImg.Seek(0, SeekOrigin.Begin);
                        var img = Image.GetInstance(streamImg);
                        img.SetAbsolutePosition(pdfImg.ImgX, pdfImg.ImgY);
                        img.ScalePercent(pdfImg.ImgScale);
                        pdfStamper.GetOverContent(1).AddImage(img);
                    }
                }
                return stream;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
            finally
            {
                pdfStamper.Close();
                pdfReader.Close();
            }
        }

        /// <summary>
        /// Adds the PDF page.
        /// </summary>
        /// <param name="pageStream">The page stream.</param>
        private void AddPDFPage(Stream pageStream)
        {
            try
            {
                pageStream.Seek(0, SeekOrigin.Begin);
                var pdfReader = new PdfReader(pageStream);
                var pdfImportPage = this._pdfCopy.GetImportedPage(pdfReader, 1);
                this._pdfCopy.AddPage(pdfImportPage);
                this._pdfCopy.FreeReader(pdfReader);
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }
        #endregion
    }

    #region Depend Class

    /// <summary>
    /// XPDFImage
    /// </summary>
    public class XPDFImage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="XPDFImage" /> class.
        /// </summary>
        /// <param name="img64String">The img64 string.</param>
        /// <param name="imgX">The img x.</param>
        /// <param name="imgY">The img y.</param>
        /// <param name="imgScale">The img scale.</param>
        public XPDFImage(string img64String, float imgX, float imgY, float imgScale)
        {
            this.Img64String = img64String;
            this.ImgX = imgX;
            this.ImgY = imgY;
            this.ImgScale = imgScale;
        }

        public string Img64String { get; set; }
        public float ImgScale { get; set; }
        public float ImgX { get; set; }
        public float ImgY { get; set; }
    }
    #endregion
}
