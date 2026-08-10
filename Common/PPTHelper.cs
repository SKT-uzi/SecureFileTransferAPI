using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Drawing;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;
using Drawing=DocumentFormat.OpenXml.Drawing;

namespace ResumableFileTransfer.Common
{
    public class PPTHelper
    {
        #region Private Member Avaraibles
        private PresentationDocument _doc = null;
        private PresentationPart _part = null;
        #endregion

        #region Public Methods
        /// <summary>
        /// Opens the PPT.
        /// </summary>
        /// <param name="stream">The stream.</param>
        public void Open(Stream stream)
        {
            try
            {
                this._doc = PresentationDocument.Open(stream, true);
                this._part = this._doc.PresentationPart;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Saves this instance.
        /// </summary>
        public void Save()
        {
            try
            {
                this._doc.Save();
                this._doc.Close();
                this._doc.Dispose();
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Fills the text slider.
        /// </summary>
        /// <param name="slideIndex">Index of the slide.</param>
        /// <param name="dicReplaceValues">The dic replace values.</param>
        public void FillTextSlider(int slideIndex, Dictionary<string, string> dicReplaceValues)
        {
            try
            {
                var slideIDList = this._part.Presentation.SlideIdList;
                var slideID = slideIDList.ChildElements[slideIndex] as SlideId;
                var frontCoverSlide = this._part.GetPartById(slideID.RelationshipId) as SlidePart;
                foreach (var value in dicReplaceValues)
                {
                    var drawText = frontCoverSlide.Slide.Descendants<Drawing.Text>().First(s => s.InnerText.Equals(value.Key));
                    if (drawText != null)
                        drawText.Text = value.Value;
                }
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Fills the text layout.
        /// </summary>
        /// <param name="layoutName">Name of the layout.</param>
        /// <param name="dicReplaceValues">The dic replace values.</param>
        public void FillTextLayout(string layoutName, Dictionary<string, string> dicReplaceValues)
        {
            try
            {
                var slideLayoutPart = this._part.SlideMasterParts.First().SlideLayoutParts.Single(x => x.SlideLayout.CommonSlideData.Name == layoutName);
                var shapeTree = slideLayoutPart.SlideLayout.CommonSlideData.ShapeTree;
                foreach (var value in dicReplaceValues)
                {
                    var drawText = shapeTree.Descendants<Drawing.Text>().First(s => s.InnerText.Equals(value.Key));
                    if (drawText != null)
                    {
                        drawText.Text = value.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Adds the image slider.
        /// </summary>
        /// <param name="slideIndex">Index of the slide.</param>
        /// <param name="slideTitle">The slide title.</param>
        /// <param name="layoutName">Name of the layout.</param>
        /// <param name="imageBase64String">The image base64 string.</param>
        /// <param name="offsetY">The offset y.</param>
        public void AddImageSlider(int slideIndex, string slideTitle, string layoutName, string imageBase64String, double offsetY = 30d)
        {
            try
            {
                var newSlidePart = this.InsertNewSlidePart(this._part, slideIndex, layoutName, slideTitle);
                var shapeTree = newSlidePart.Slide.CommonSlideData.ShapeTree;
                var shapeTreeLayout = newSlidePart.SlideLayoutPart.SlideLayout.CommonSlideData.ShapeTree;
                var imageID = "img1";
                var imagePart = newSlidePart.AddImagePart(ImagePartType.Png, imageID);
                using (MemoryStream stream = BaseHelper.GetImageStream(imageBase64String))
                {
                    imagePart.FeedData(stream);
                    var imageSize = this.GetImageSize(stream, false, offsetY);
                    var pic = this.GeneratePicture(imageID, imageSize);
                    shapeTree.Append(pic);
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
        /// Inserts the new slide.
        /// </summary>
        /// <param name="presentationPart">The presentation part.</param>
        /// <param name="position">The position.</param>
        /// <param name="layoutName">Name of the layout.</param>
        /// <param name="slideTitle">The slide title.</param>
        /// <returns></returns>
        private SlidePart InsertNewSlidePart(PresentationPart presentationPart, int position, string layoutName, string slideTitle = "Slide Title")
        {
            try
            {
                var slide = new Slide(new CommonSlideData(new ShapeTree()));

                //1. create NonVisualGroupShapeProperties
                var nonVisualGroupShape = new NonVisualGroupShapeProperties();
                nonVisualGroupShape.NonVisualDrawingProperties = new NonVisualDrawingProperties() { Id = 1, Name = string.Empty };
                nonVisualGroupShape.NonVisualGroupShapeDrawingProperties = new NonVisualGroupShapeDrawingProperties();
                nonVisualGroupShape.ApplicationNonVisualDrawingProperties = new ApplicationNonVisualDrawingProperties();
                slide.CommonSlideData.ShapeTree.AppendChild(nonVisualGroupShape);

                // 2. Specify the group shape properties of the new slide.
                slide.CommonSlideData.ShapeTree.AppendChild(new GroupShapeProperties());

                //Declare and instantiate the title shape of the new slide.
                var nonVisualShape = new NonVisualShapeProperties();
                nonVisualShape.NonVisualDrawingProperties = new NonVisualDrawingProperties() { Id = 2, Name = "Title" };
                nonVisualShape.NonVisualShapeDrawingProperties = new NonVisualShapeDrawingProperties(new Drawing.ShapeLocks() { NoGrouping = true });
                nonVisualShape.ApplicationNonVisualDrawingProperties = new ApplicationNonVisualDrawingProperties(new PlaceholderShape() { Type = PlaceholderValues.Title });

                //Specify the required shape properties for the title shape. 
                var titleShape = new Shape() { NonVisualShapeProperties = nonVisualShape, ShapeProperties = new ShapeProperties() };

                //3. Specify the text of the title shape.
                titleShape.TextBody = new TextBody(new Drawing.BodyProperties(), new Drawing.ListStyle(), new Drawing.Paragraph(new Drawing.Run(new Drawing.Text() { Text = slideTitle })));
                slide.CommonSlideData.ShapeTree.AppendChild(titleShape);


                // Create the slide part for the new slide.
                SlidePart slidePart = presentationPart.AddNewPart<SlidePart>();

                // Save the new slide part.
                slide.Save(slidePart);

                // Modify the slide ID list in the presentation part.
                // The slide ID list should not be null.
                if (presentationPart.Presentation.SlideIdList == null)
                    presentationPart.Presentation.SlideIdList = new SlideIdList();

                SlideIdList slideIdList = presentationPart.Presentation.SlideIdList;

                // Find the highest slide ID in the current list.
                var maxSlideId = 1u;
                var prevSlideId = (SlideId)null;
                var newSlideId = (SlideId)null;
                foreach (SlideId slideId in slideIdList.ChildElements)
                {
                    if (slideId.Id > maxSlideId)
                    {
                        maxSlideId = slideId.Id;
                    }

                    position--;
                    if (position == 0)
                    {
                        prevSlideId = slideId;
                    }
                }
                maxSlideId++;

                // Get the ID of the previous slide.
                var lastSlidePart = (SlidePart)null;
                if (prevSlideId != null)
                {
                    lastSlidePart = (SlidePart)presentationPart.GetPartById(prevSlideId.RelationshipId);
                }
                else
                {
                    lastSlidePart = (SlidePart)presentationPart.GetPartById(((SlideId)slideIdList.ChildElements[0]).RelationshipId);
                }

                // Use the  slide layout "Title and Content" for the new slide 
                if (null != lastSlidePart.SlideLayoutPart)
                {
                    slidePart.AddPart(presentationPart.SlideMasterParts.First().SlideLayoutParts.Single(x => x.SlideLayout.CommonSlideData.Name == layoutName));
                }

                // Insert the new slide into the slide list after the previous slide.
                foreach (SlideId slideId in slideIdList.ChildElements)
                {
                    if (slideId.Id > maxSlideId)
                    {
                        maxSlideId = slideId.Id;
                        prevSlideId = slideId;
                    }
                }

                newSlideId = slideIdList.InsertAfter(new SlideId(), prevSlideId);
                newSlideId.Id = maxSlideId;
                newSlideId.RelationshipId = presentationPart.GetIdOfPart(slidePart);
                return slidePart;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Gets the size of the image.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="isFullScreen">if set to <c>true</c> [is full screen].</param>
        /// <param name="offsetY">The offset y.</param>
        /// <returns></returns>
        private PPTImageSize GetImageSize(MemoryStream stream, bool isFullScreen, double offsetY)
        {
            try
            {
                var xValue = 0d;
                var yValue = 0d;
                var maxWidth = isFullScreen ? 960d : 880d;
                var maxHeight = 760d;
                var slideWidth = 986d;
                var slideHeight = 722d;

                var bm = Image.FromStream(stream) as Bitmap;
                var zoomFactor = Math.Max(bm.Width / maxWidth, bm.Height / maxHeight);
                var newWidth = bm.Width / zoomFactor;
                var newHeight = bm.Height / zoomFactor;
                if (isFullScreen)
                {
                    if (bm.Height / maxHeight > bm.Width / maxWidth)
                    {
                        xValue = (slideWidth - newWidth) / 2;
                    }
                    else
                    {
                        xValue = 0;
                    }
                }
                else
                {
                    xValue = (slideWidth - newWidth) / 2;
                }

                yValue = (slideHeight - newHeight) / 2;


                var imageSize = new PPTImageSize();
                imageSize.ExtentsX = Convert.ToInt64(newWidth * 914400 / bm.HorizontalResolution);
                imageSize.ExtentsY = Convert.ToInt64(newHeight * 914400 / bm.VerticalResolution);
                imageSize.OffsetX = Convert.ToInt64(Math.Abs(xValue) * 914400 / bm.HorizontalResolution);
                imageSize.OffsetY = Convert.ToInt64((Math.Abs(yValue) + offsetY) * 914400 / bm.VerticalResolution);
                return imageSize;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Generates the picture.
        /// </summary>
        /// <param name="imageID">The image identifier.</param>
        /// <param name="imageSize">Size of the image.</param>
        /// <returns></returns>
        private Picture GeneratePicture(string imageID, PPTImageSize imageSize)
        {
            try
            {
                var nonVisualPictureDrawingProperties = new NonVisualPictureDrawingProperties();
                nonVisualPictureDrawingProperties.Append(new Drawing.PictureLocks() { NoChangeAspect = true });

                var nonVisualPictureProperties = new NonVisualPictureProperties();
                nonVisualPictureProperties.Append(new NonVisualDrawingProperties() { Id = (UInt32Value)4U, Name = "Picture 3" });
                nonVisualPictureProperties.Append(nonVisualPictureDrawingProperties);
                nonVisualPictureProperties.Append(new ApplicationNonVisualDrawingProperties());

                var transform2D = new Drawing.Transform2D();
                transform2D.Append(new Drawing.Offset() { X = imageSize.OffsetX, Y = imageSize.OffsetY });
                transform2D.Append(new Drawing.Extents() { Cx = imageSize.ExtentsX, Cy = imageSize.ExtentsY });

                var presetGeometry = new Drawing.PresetGeometry() { Preset = Drawing.ShapeTypeValues.Rectangle };
                presetGeometry.Append(new Drawing.AdjustValueList());

                var shapeProperties = new ShapeProperties();
                shapeProperties.Append(transform2D);
                shapeProperties.Append(presetGeometry);

                var stretch = new Drawing.Stretch();
                stretch.Append(new Drawing.FillRectangle());

                var blipFill = new BlipFill();
                blipFill.Append(new Drawing.Blip() { Embed = imageID });
                blipFill.Append(stretch);

                var picture = new Picture();
                picture.Append(nonVisualPictureProperties);
                picture.Append(blipFill);
                picture.Append(shapeProperties);
                return picture;
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
    /// PPTImageSize
    /// </summary>
    class PPTImageSize
    {
        public Int64 ExtentsX { get; set; }
        public Int64 ExtentsY { get; set; }
        public Int64 OffsetX { get; set; }
        public Int64 OffsetY { get; set; }
    }
    #endregion
}
