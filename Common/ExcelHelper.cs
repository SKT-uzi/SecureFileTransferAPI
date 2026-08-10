using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Drawing = DocumentFormat.OpenXml.Drawing.Spreadsheet;

namespace ResumableFileTransfer.Common
{
    /// <summary>
    /// OpenXmlExcelHelper
    /// </summary>
    public class ExcelHelper
    {
        #region Private Varaibles

        private Regex _regexColKey = new Regex(@"[A-Z]+");
        private Regex _regexRowIndex = new Regex(@"[0-9]+");
        private Dictionary<string, uint> _dicFontStylesIndex = null;
        private Dictionary<string, uint> _dicBorderStylesIndex = null;
        private Dictionary<string, uint> _dicFillStylesIndex = null;
        private Dictionary<string, uint> _dicNumFmtStylesIndex = null;
        private Dictionary<string, uint> _dicStyleIndex = null;

        private WorkbookPart _workBookPart = null;
        private WorksheetPart _worksheetPart = null;
        private SpreadsheetDocument _doc = null;
        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ExcelHelper" /> class.
        /// </summary>
        public ExcelHelper()
        {
            this._dicFontStylesIndex = new Dictionary<string, uint>();
            this._dicBorderStylesIndex = new Dictionary<string, uint>();
            this._dicFillStylesIndex = new Dictionary<string, uint>();
            this._dicNumFmtStylesIndex = new Dictionary<string, uint>();
            this._dicStyleIndex = new Dictionary<string, uint>();
            this._dicNumFmtStylesIndex.Add(ExcelFormat.Percentage1, 9U);
            this._dicNumFmtStylesIndex.Add(ExcelFormat.Percentage2, 10U);
            this._dicNumFmtStylesIndex.Add(ExcelFormat.Number1, 1U);
            this._dicNumFmtStylesIndex.Add(ExcelFormat.Number2, 2U);
            this._dicNumFmtStylesIndex.Add(ExcelFormat.Number3, 3U);
            this._dicNumFmtStylesIndex.Add(ExcelFormat.Number4, 37U);
            this._dicNumFmtStylesIndex.Add(ExcelFormat.Number5, 38U);
            this._dicNumFmtStylesIndex.Add(ExcelFormat.Number6, 39U);
            this._dicNumFmtStylesIndex.Add(ExcelFormat.Number7, 40U);
            this._dicNumFmtStylesIndex.Add(ExcelFormat.Date1, 14U);
            this._dicNumFmtStylesIndex.Add(ExcelFormat.Date2, 15U);
            this._dicNumFmtStylesIndex.Add(ExcelFormat.Date3, 16U);
            this._dicNumFmtStylesIndex.Add(ExcelFormat.Date4, 17U);
            this._dicNumFmtStylesIndex.Add(ExcelFormat.Date5, 18U);
            this._dicNumFmtStylesIndex.Add(ExcelFormat.Date6, 19U);
            this._dicNumFmtStylesIndex.Add(ExcelFormat.Date7, 20U);
            this._dicNumFmtStylesIndex.Add(ExcelFormat.Date8, 21U);
            this._dicNumFmtStylesIndex.Add(ExcelFormat.Date9, 22U);
            this._dicNumFmtStylesIndex.Add(ExcelFormat.Date10, 45U);
        }

        #endregion

        #region Public Methods

        #region Style Sheet Methods

        /// <summary>
        /// Load the font style.
        /// </summary>
        /// <param name="dicFontStyle">The dic font style.</param>
        public void LoadFontStyleSheet(Dictionary<string, XFontStyle> dicFontStyle)
        {
            try
            {
                var styleSheet = this._workBookPart.WorkbookStylesPart.Stylesheet;
                if (styleSheet.Fonts == null)
                    styleSheet.Fonts = new Fonts();

                foreach (var item in dicFontStyle)
                {
                    var font = new Font();
                    if (string.IsNullOrEmpty(item.Value.Color) == false)
                        font.Append(new Color() { Rgb = item.Value.Color });

                    if (item.Value.Bold)
                        font.Bold = new Bold();

                    if (string.IsNullOrEmpty(item.Value.Name) == false)
                        font.FontName = new FontName() { Val = new StringValue(item.Value.Name) };

                    if (item.Value.Size.HasValue)
                        font.FontSize = new FontSize() { Val = new DoubleValue(item.Value.Size) };
                    
                    if (item.Value.Italic)
                        font.Italic = new Italic();

                    if (item.Value.Underline)
                        font.Underline = new Underline();

                    styleSheet.Fonts.Append(font);
                    this._dicFontStylesIndex.Add(item.Key, (uint)styleSheet.Fonts.Count() - 1);
                }
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Loads the border style sheet.
        /// </summary>
        /// <param name="dicBorderStyle">The dic border style.</param>
        public void LoadBorderStyleSheet(Dictionary<string, ExcelBorderStyle> dicBorderStyle)
        {
            try
            {
                var styleSheet = this._workBookPart.WorkbookStylesPart.Stylesheet;
                if (styleSheet.Borders == null)
                    styleSheet.Borders = new Borders();

                foreach (var item in dicBorderStyle)
                {
                    var color = (Color)null;
                    if (string.IsNullOrEmpty(item.Value.Color) == false)
                    {
                        color = new Color() { Rgb = item.Value.Color };
                    }
                    else
                    {
                        color = new Color() { Indexed = (UInt32Value)64U };
                    }

                    var leftBorder = new LeftBorder() { Style = item.Value.Left ? (BorderStyleValues)item.Value.Border.GetHashCode() : BorderStyleValues.None };
                    var rightBorder = new RightBorder() { Style = item.Value.Right ? (BorderStyleValues)item.Value.Border.GetHashCode() : BorderStyleValues.None };
                    var topBorder = new TopBorder() { Style = item.Value.Top ? (BorderStyleValues)item.Value.Border.GetHashCode() : BorderStyleValues.None };
                    var bottomBorder = new BottomBorder() { Style = item.Value.Bottom ? (BorderStyleValues)item.Value.Border.GetHashCode() : BorderStyleValues.None };
                    leftBorder.Append(color.CloneNode(false));
                    rightBorder.Append(color.CloneNode(false));
                    topBorder.Append(color.CloneNode(false));
                    bottomBorder.Append(color.CloneNode(false));

                    var border = new Border();
                    border.Append(leftBorder);
                    border.Append(rightBorder);
                    border.Append(topBorder);
                    border.Append(bottomBorder);
                    border.Append(new DiagonalBorder());
                    styleSheet.Borders.Append(border);
                    this._dicBorderStylesIndex.Add(item.Key, (uint)styleSheet.Borders.Count() - 1);
                }
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Loads the number FMT style sheet.
        /// </summary>
        /// <param name="dicNumFmtStyle">The dic number FMT style.</param>
        public void LoadNumFmtStyleSheet(Dictionary<string, string> dicNumFmtStyle)
        {
            try
            {
                var styleSheet = this._workBookPart.WorkbookStylesPart.Stylesheet;
                var numFmtID = 201u;
                if (styleSheet.NumberingFormats == null)
                    styleSheet.NumberingFormats = new NumberingFormats();

                foreach (var item in dicNumFmtStyle)
                {
                    styleSheet.NumberingFormats.Append(new NumberingFormat() { NumberFormatId = numFmtID, FormatCode = item.Value });
                    this._dicNumFmtStylesIndex.Add(item.Key, numFmtID);
                    numFmtID++;
                }
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Gets the index of the style.
        /// </summary>
        /// <param name="fontKey">The font key.</param>
        /// <param name="bgColor">The fill key.</param>
        /// <param name="borderKey">The border key.</param>
        /// <param name="numFmtKey">The number FMT key.</param>
        /// <param name="alignStyle">The align style.</param>
        /// <returns></returns>
        public uint GetStyleIndex(string fontKey=null, string bgColor=null, string borderKey=null, string numFmtKey=null, ExcelAlignStyle alignStyle = null)
        {
            try
            {
                if (fontKey == null)
                    fontKey = string.Empty;
                if (bgColor == null)
                    bgColor = string.Empty;
                if (borderKey == null)
                    borderKey = string.Empty;
                if (numFmtKey == null)
                    numFmtKey = string.Empty;

                if (fontKey.Length == 0 && bgColor.Length == 0 && borderKey.Length == 0 && numFmtKey.Length == 0 && alignStyle == null)
                    return 0;

                var styleKey = string.Format("{0}_{1}_{2}_{3}", new object[] { fontKey, bgColor, borderKey, numFmtKey });
                if (alignStyle != null)
                    styleKey += string.Format("${0}-{1}-{2}", new object[] { alignStyle.WrapText, alignStyle.Horizontal, alignStyle.Vertical });

                if (this._dicStyleIndex.ContainsKey(styleKey))
                    return this._dicStyleIndex[styleKey];

                var workbookStylesPart = this._workBookPart.WorkbookStylesPart;
                var styleSheet = workbookStylesPart.Stylesheet;
                var cellFormat = new CellFormat();
                if (string.IsNullOrEmpty(fontKey) == false)
                    cellFormat.FontId = this._dicFontStylesIndex[fontKey];

                if (string.IsNullOrEmpty(bgColor) == false)
                    cellFormat.FillId = this.GetBGColorIndex(this._workBookPart, bgColor);

                if (string.IsNullOrEmpty(borderKey) == false)
                    cellFormat.BorderId = this._dicBorderStylesIndex[borderKey];

                if (string.IsNullOrEmpty(numFmtKey) == false)
                    cellFormat.NumberFormatId = this._dicNumFmtStylesIndex[numFmtKey];

                if (alignStyle != null)
                {
                    cellFormat.Alignment = new Alignment();
                    cellFormat.Alignment.WrapText = new BooleanValue(alignStyle.WrapText);
                    cellFormat.Alignment.Horizontal = new EnumValue<HorizontalAlignmentValues>() { Value = (HorizontalAlignmentValues)alignStyle.Horizontal.GetHashCode() };
                    cellFormat.Alignment.Vertical = new EnumValue<VerticalAlignmentValues>() { Value = (VerticalAlignmentValues)alignStyle.Vertical.GetHashCode() };
                }

                styleSheet.CellFormats.Append(cellFormat);
                var styleIndex = (uint)styleSheet.CellFormats.Count() - 1;
                this._dicStyleIndex.Add(styleKey, styleIndex);
                return styleIndex;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Gets the range cells.
        /// </summary>
        /// <param name="cellReference">The range. {A2:B18,C5,F6,J8:K3}</param>
        /// <param name="styleIndex">Index of the style.</param>
        public void SetCellStyle(string cellReference, uint? styleIndex)
        {
            try
            {
                //1. get all range cell
                var cellReferenceList = new List<string>();
                var arrSumReference = cellReference.Split(',');
                foreach (string sumReference in arrSumReference)
                {
                    var arrRangeReference = sumReference.Split(':');
                    //one cell B3
                    if (arrRangeReference.Length == 1)
                    {
                        cellReferenceList.Add(arrRangeReference[0]);
                        continue;
                    }

                    //range cell: {A1:B5}
                    var startRowIndex = this.MatchRowIndex(arrRangeReference[0]);
                    var endRowIndex = this.MatchRowIndex(arrRangeReference[1]);
                    var startColIndex = this.MatchColIndex(arrRangeReference[0]);
                    var endColIndex = this.MatchColIndex(arrRangeReference[1]);

                    var colKeyList = new List<string>();
                    for (var m = startColIndex; m <= endColIndex; m++)
                    {
                        colKeyList.Add(BaseHelper.GetExcelColKey(m));
                    }

                    for (var i = startRowIndex; i <= endRowIndex; i++)
                    {
                        for (var j = startColIndex; j <= endColIndex; j++)
                        {
                            var colKey = colKeyList[Convert.ToInt32(j - startColIndex)];
                            cellReferenceList.Add($"{colKey}{i}");
                        }
                    }
                }

                //2. set cell style index
                var cellList = this._worksheetPart.Worksheet.Descendants<Cell>().ToList();
                foreach (Cell item in cellList)
                {
                    if (cellReferenceList.Contains(item.CellReference) == false)
                        continue;

                    item.StyleIndex = styleIndex;
                }
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Sets the height of the row.
        /// </summary>
        /// <param name="rowIndex">Index of the row.</param>
        /// <param name="rowHeight">Height of the row.</param>
        public void SetRowHeight(int rowIndex, double rowHeight)
        {
            try
            {
                var sheetData = this._worksheetPart.Worksheet.GetFirstChild<SheetData>();
                var currentRow = this.GetRow(sheetData, (uint)rowIndex);
                currentRow.Height = DoubleValue.FromDouble(rowHeight);
                currentRow.CustomHeight = true;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Sets the width of the columns.
        /// </summary>
        /// <param name="cellReference">The cell reference.</param>
        /// <param name="columnWidth">Width of the column.</param>
        public void SetColumnsWidth(string cellReference, double columnWidth)
        {
            try
            {
                var startColIndex = 0u;
                var endColIndex = 0u;
                var columns = this._worksheetPart.Worksheet.GetFirstChild<Columns>();
                var sheetData = this._worksheetPart.Worksheet.GetFirstChild<SheetData>();

                var arrRangeReference = cellReference.Split(':');
                startColIndex = this.MatchColIndex(arrRangeReference[0]);
                //one cell B3
                if (arrRangeReference.Length == 1)
                {
                    endColIndex = startColIndex;
                }
                else
                {
                    //range cell: {A1:B5}
                    endColIndex = this.MatchColIndex(arrRangeReference[1]);
                }

                var newColumn = new Column()
                {
                    Min = startColIndex,
                    Max = endColIndex,
                    Width = DoubleValue.FromDouble(columnWidth)
                };

                if (columns == null)
                {
                    columns = new Columns();
                    columns.Append(newColumn);
                    this._worksheetPart.Worksheet.InsertBefore<Columns>(columns, sheetData);
                }
                else
                {
                    columns.Append(newColumn);
                }
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Sets the width of the columns.
        /// </summary>
        /// <param name="dicWidth">Width of the dic.</param>
        public void SetColumnsWidth(Dictionary<string, double> dicWidth)
        {
            try
            {
                
                var columns = this._worksheetPart.Worksheet.GetFirstChild<Columns>();
                if (columns == null)
                {
                    columns = new Columns();
                    var sheetData = this._worksheetPart.Worksheet.GetFirstChild<SheetData>();
                    this._worksheetPart.Worksheet.InsertBefore<Columns>(columns, sheetData);
                }

                foreach (var item in dicWidth)
                {
                    var colIndex = this.MatchColIndex(item.Key);
                    var column = columns.Descendants<Column>().FirstOrDefault(d => d.Min.Value == colIndex);
                    if (column == null)
                    {
                        column = new Column();
                        columns.Append(column);
                    }
                    column.Min = colIndex;
                    column.Max = colIndex;
                    column.Width = DoubleValue.FromDouble(item.Value);
                }
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }
        #endregion

        #region Fill Data Methods

        /// <summary>
        /// Fills the sheet data.
        /// </summary>
        /// <param name="startCell">The start cell.</param>
        /// <param name="dataList">The data list.</param>
        /// <param name="cellStyle">The cell style.</param>
        public void FillSheetTableData(string startCell, List<object[]> dataList, uint? cellStyle = null)
        {
            try
            {
                var sheetData = this._worksheetPart.Worksheet.GetFirstChild<SheetData>();
                var startRow = this.MatchRowIndex(startCell);
                var startCol = this.MatchColIndex(startCell);
                //Add Row Data
                foreach (var data in dataList)
                {
                    var row = this.GetRow(sheetData, startRow);
                    for (uint i = 0; i < data.Length; i++)
                    {
                        var cell = this.GetCell(row, startCol + i);
                        this.SetCellValue(cell, data[i], cellStyle);
                    }
                    startRow++;
                }
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Fills the sheet table data.
        /// </summary>
        /// <param name="startCell">The start cell.</param>
        /// <param name="dataList">The data list.</param>
        /// <param name="rowStyleList">The row style list.</param>
        public void FillSheetTableData(string startCell, List<object[]> dataList, List<uint[]> rowStyleList)
        {
            try
            {
                var sheetData = this._worksheetPart.Worksheet.GetFirstChild<SheetData>();
                var startRow = this.MatchRowIndex(startCell);
                var startCol = this.MatchColIndex(startCell);
                //Add Row Data
                for (uint i = 0; i < dataList.Count; i++)
                {
                    var data = dataList[(int)i];
                    var rowStyle = rowStyleList[(int)i];
                    var row = this.GetRow(sheetData, startRow + i);
                    for (uint j = 0; j < data.Length; j++)
                    {
                        var cell = this.GetCell(row, startCol + j);
                        this.SetCellValue(cell, data[j], rowStyle[j]);
                    }
                }
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Fills the sheet row data.
        /// </summary>
        /// <param name="startCell">The start cell.</param>
        /// <param name="valueList">The value list.</param>
        /// <param name="cellStyle">The cell style.</param>
        /// <returns></returns>
        public void FillSheetRowData(string startCell, object[] valueList, uint? cellStyle = null)
        {
            try
            {
                var sheetData = this._worksheetPart.Worksheet.GetFirstChild<SheetData>();
                var startRow = this.MatchRowIndex(startCell);
                var startCol = this.MatchColIndex(startCell);
                var row = this.GetRow(sheetData, startRow);
                for (var i = 0; i < valueList.Length; i++)
                {
                    var cell = this.GetCell(row, startCol + (uint)i);
                    this.SetCellValue(cell, valueList[i], cellStyle);
                }
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Fills the sheet row data.
        /// </summary>
        /// <param name="startCell">The start cell.</param>
        /// <param name="valueList">The value list.</param>
        /// <param name="cellStyleList">The cell style list.</param>
        /// <returns></returns>
        public void FillSheetRowData(string startCell, object[] valueList, uint[] cellStyleList)
        {
            try
            {
                var sheetData = this._worksheetPart.Worksheet.GetFirstChild<SheetData>();
                var startRow = this.MatchRowIndex(startCell);
                var startCol = this.MatchColIndex(startCell);
                var row = this.GetRow(sheetData, startRow);
                for (var i = 0; i < valueList.Length; i++)
                {
                    var cell = this.GetCell(row, startCol + (uint)i);
                    this.SetCellValue(cell, valueList[i], cellStyleList[i]);
                }
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Fills the sheet column data.
        /// </summary>
        /// <param name="startCell">The start cell.</param>
        /// <param name="valueList">The value list.</param>
        /// <param name="cellStyle">The cell style.</param>
        public void FillSheetColumnData(string startCell, object[] valueList, uint? cellStyle = null)
        {
            try
            {
                var sheetData = this._worksheetPart.Worksheet.GetFirstChild<SheetData>();
                var startRow = this.MatchRowIndex(startCell);
                var startCol = this.MatchColIndex(startCell);
                for (int i = 0; i < valueList.Length; i++)
                {
                    var row = this.GetRow(sheetData, startRow + (uint)i);
                    var cell = this.GetCell(row, startCol);
                    this.SetCellValue(cell, valueList[i], cellStyle);
                }
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Fills the sheet column data.
        /// </summary>
        /// <param name="startCell">The start cell.</param>
        /// <param name="valueList">The value list.</param>
        /// <param name="cellStyleList">The cell style list.</param>
        public void FillSheetColumnData(string startCell, object[] valueList, uint[] cellStyleList)
        {
            try
            {
                var sheetData = this._worksheetPart.Worksheet.GetFirstChild<SheetData>();
                var startRow = this.MatchRowIndex(startCell);
                var startCol = this.MatchColIndex(startCell);
                for (int i = 0; i < valueList.Length; i++)
                {
                    var row = this.GetRow(sheetData, startRow + (uint)i);
                    var cell = this.GetCell(row, startCol);
                    this.SetCellValue(cell, valueList[i], cellStyleList[i]);
                }
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Fills the sheet data.
        /// </summary>
        /// <param name="cellReference">The cell reference.{A2:B18,C5,F6,J8:K3}</param>
        /// <param name="value">The value.</param>
        /// <param name="styleIndex">Index of the style.</param>
        public void FillSheetCellReferenceData(string cellReference, object value, uint? styleIndex = null)
        {
            try
            {
                var startRowIndex = 0u;
                var endRowIndex = 0u;
                var startColIndex = 0u;
                var endColIndex = 0u;
                var arrSumReference = cellReference.Split(',');
                var sheetData = this._worksheetPart.Worksheet.GetFirstChild<SheetData>();
                foreach (string sumReference in arrSumReference)
                {
                    var arrRangeReference = sumReference.Split(':');
                    startRowIndex = this.MatchRowIndex(arrRangeReference[0]);
                    startColIndex = this.MatchColIndex(arrRangeReference[0]);
                    //one cell B3
                    if (arrRangeReference.Length == 1)
                    {
                        endColIndex = startColIndex;
                        endRowIndex = startRowIndex;
                    }
                    else
                    {
                        //range cell: {A1:B5}
                        endRowIndex = this.MatchRowIndex(arrRangeReference[1]);
                        endColIndex = this.MatchColIndex(arrRangeReference[1]);
                    }

                    for (var i = startRowIndex; i <= endRowIndex; i++)
                    {
                        var row = this.GetRow(sheetData, i);
                        for (var j = startColIndex; j <= endColIndex; j++)
                        {
                            var cell = this.GetCell(row, j);
                            this.SetCellValue(cell, value, styleIndex);
                            if (styleIndex.HasValue)
                                cell.StyleIndex = styleIndex.Value;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        #endregion

        #region Other Methods

        /// <summary>
        /// Opens the specified stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        public void Open(Stream stream)
        {
            try
            {
                this._doc = SpreadsheetDocument.Open(stream, true);
                this._workBookPart = this._doc.WorkbookPart;
                this._workBookPart.Workbook.CalculationProperties.ForceFullCalculation = true;
                this._workBookPart.Workbook.CalculationProperties.FullCalculationOnLoad = true;
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
                this._workBookPart.Workbook.Save();
                this._doc.Close();
                this._doc.Dispose();
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }


        /// <summary>
        /// Gets the worksheet part.
        /// </summary>
        /// <param name="sheetName">Name of the sheet.</param>
        /// <returns></returns>
        public void ActiveWorksheet(string sheetName)
        {
            try
            {
                var sheetList = this._workBookPart.Workbook.Sheets.Elements<Sheet>().ToList();
                var index = 0u;
                foreach (var item in sheetList)
                {
                    if (item.Name == sheetName)
                    {
                        this._worksheetPart = this._workBookPart.GetPartById(item.Id) as WorksheetPart;
                        break;
                    }
                    index++;
                }

                WorkbookView _workbookView = this._workBookPart.Workbook.BookViews.ChildElements.First<WorkbookView>();
                if (_workbookView != null)
                {
                    _workbookView.ActiveTab = new UInt32Value(index); 
                }
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Sets the merge cell.
        /// </summary>
        /// <param name="startCell">The start cell.</param>
        /// <param name="mergeCount">The merge count.</param>
        /// <param name="isVertical">if set to <c>true</c> [is vertical].</param>
        public void SetMergeCell(string startCell, int mergeCount, bool isVertical)
        {
            try
            {
                var mergeCells = this.GetMergeCells(this._worksheetPart.Worksheet);
                var startColKey = this.MatchColKey(startCell);
                var startColIndex = BaseHelper.GetExcelColIndex(startColKey);
                var startRowIndex = this.MatchRowIndex(startCell);

                var mergeReference = string.Empty;
                if (isVertical)
                {
                    mergeReference = startCell + ":" + startColKey + (startRowIndex + mergeCount - 1);
                }
                else
                {
                    mergeReference = startCell + ":" + BaseHelper.GetExcelColKey(startColIndex + (uint)mergeCount - 1) + startRowIndex;
                }
                mergeCells.Append(new MergeCell()
                {
                    Reference = new StringValue(mergeReference)
                });

                mergeCells.Count = (uint)mergeCells.Count();
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Sets the defined name value.
        /// </summary>
        /// <param name="dicDefinedNames">The dic defined names.</param>
        public void UpdateDefinedNameValue(Dictionary<string, string> dicDefinedNames)
        {
            try
            {
                var definedNameList = this._workBookPart.Workbook.Descendants<DefinedName>().ToList();
                foreach (var item in definedNameList)
                {
                    var nameKey = item.Name.Value;
                    if (dicDefinedNames.ContainsKey(nameKey))
                    {
                        item.Text = dicDefinedNames[nameKey];
                    }
                }
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Ignores the number as text.
        /// </summary>
        /// <param name="reference">The reference.</param>
        public void IgnoreNumberAsText(string reference)
        {
            try
            {
                var ignoredErrors = this._worksheetPart.Worksheet.GetFirstChild<IgnoredErrors>();
                if (ignoredErrors == null)
                {
                    ignoredErrors = new IgnoredErrors();
                    this._worksheetPart.Worksheet.InsertAfter(ignoredErrors, this._worksheetPart.Worksheet.GetFirstChild<PageSetup>());
                }
                ignoredErrors.Append(new IgnoredError() { SequenceOfReferences = new ListValue<StringValue>() { InnerText = reference }, NumberStoredAsText = true });
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Deletes the sheet.
        /// </summary>
        /// <param name="sheetName">Name of the sheet.</param>
        public void DeleteSheet(string sheetName)
        {
            try
            {
                var sheet = this._workBookPart.Workbook.Descendants<Sheet>().FirstOrDefault(s => s.Name == sheetName);
                if (sheet == null)
                {
                    return;
                }

                // Remove the sheet reference from the workbook.
                var workSheetPart = (WorksheetPart)(this._workBookPart.GetPartById(sheet.Id));
                sheet.Remove();

                // Delete the worksheet part.
                this._workBookPart.DeletePart(workSheetPart);
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Renames the sheet.
        /// </summary>
        /// <param name="originalSheetName">Name of the original sheet.</param>
        /// <param name="newSheetName">New name of the sheet.</param>
        public void RenameSheet(string originalSheetName, string newSheetName)
        {
            try
            {
                var sheet = this._workBookPart.Workbook.Descendants<Sheet>().FirstOrDefault(s => s.Name == originalSheetName);
                if (sheet == null)
                {
                    return;
                }

                sheet.Name = newSheetName;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Copies the sheet.
        /// </summary>
        /// <param name="templateSheetName">Name of the sheet.</param>
        /// <param name="sheetName">Name of the cloned sheet.</param>
        /// <param name="insertAt">The insert at.</param>
        public void CopySheet(string templateSheetName, string sheetName, int insertAt)
        {
            try
            {
                var templateSheetID = this._workBookPart.Workbook.Descendants<Sheet>().FirstOrDefault(s => s.Name == templateSheetName).Id.Value;
                var templateSheetPart = this._workBookPart.GetPartById(templateSheetID) as WorksheetPart;

                //Add cloned sheet and all associated parts to workbook
                var tempSheet = SpreadsheetDocument.Create(new MemoryStream(), SpreadsheetDocumentType.Workbook);
                var tempSheetPart = tempSheet.AddWorkbookPart().AddPart<WorksheetPart>(templateSheetPart);

                var clonedSheetPart = this._workBookPart.AddPart<WorksheetPart>(tempSheetPart);
                var views = clonedSheetPart.Worksheet.GetFirstChild<SheetViews>();
                foreach (SheetView view in views)
                {
                    view.WorkbookViewId = 0;
                    view.TabSelected = new BooleanValue(false);
                }

                views = templateSheetPart.Worksheet.GetFirstChild<SheetViews>();
                foreach (SheetView view in views)
                {
                    view.TabSelected = new BooleanValue(false);
                }

                //Add new sheet to main workbook part
                var sheetParts = this._workBookPart.Workbook.GetFirstChild<Sheets>();
                Sheet copiedSheet = new Sheet();
                copiedSheet.Name = sheetName;
                copiedSheet.Id = this._workBookPart.GetIdOfPart(clonedSheetPart);
                copiedSheet.SheetId = (uint)sheetParts.ChildElements.Count + 1;
                sheetParts.InsertAt(copiedSheet, insertAt);
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Inserts the row.
        /// </summary>
        /// <param name="insertAt">The insert at.</param>
        /// <param name="appendRow">The row count.</param>
        public void InsertRow(uint insertAt, uint appendRow)
        {
            try
            {
                //modify drawing refrence
                var changeRowID = 0u;
                var colKey = string.Empty;
                var drawingPart = this._worksheetPart.GetPartsOfType<DrawingsPart>().ToList().FirstOrDefault();
                if (drawingPart != null)
                {
                    var picList = drawingPart.WorksheetDrawing.Descendants<Drawing.TwoCellAnchor>().Where(d => Convert.ToInt32(d.FromMarker.RowId.Text) > insertAt);
                    foreach (var anchor in picList)
                    {
                        changeRowID = Convert.ToUInt32(anchor.FromMarker.RowId.Text) + appendRow;
                        anchor.FromMarker.RowId.Text = changeRowID.ToString();

                        changeRowID = Convert.ToUInt32(anchor.ToMarker.RowId.Text) + appendRow;
                        anchor.ToMarker.RowId.Text = changeRowID.ToString();
                    }
                }

                //modify sheetData refrence
                var sheetData = this._worksheetPart.Worksheet.GetFirstChild<SheetData>();
                var rowList = sheetData.Descendants<Row>().Where(d => d.RowIndex > insertAt).ToList();
                foreach (Row row in rowList)
                {
                    changeRowID = row.RowIndex.Value + appendRow;
                    row.RowIndex = new UInt32Value(changeRowID);
                    foreach (Cell cell in row.ChildElements)
                    {
                        colKey = this.MatchColKey(cell.CellReference.Value);
                        cell.CellReference = new StringValue($"{colKey}{changeRowID}");
                    }
                }

                //modify mergeCells
                var mergeCellList = this._worksheetPart.Worksheet.Descendants<MergeCell>().ToList();
                foreach (var mergeCell in mergeCellList)
                {
                    var arrReference = mergeCell.Reference.Value.Split(':');
                    var startRow = this.MatchRowIndex(arrReference[0]);
                    var startCol = this.MatchColKey(arrReference[0]);
                    var endRow = this.MatchRowIndex(arrReference[1]);
                    var endCol = this.MatchColKey(arrReference[1]);
                    if (startRow <= insertAt)
                        continue;

                    mergeCell.Reference = new StringValue($"{startCol}{startRow + appendRow}:{endCol}{ endRow + appendRow}");
                }
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Inserts the col.
        /// </summary>
        /// <param name="insertAt">The insert at.</param>
        /// <param name="appendCol">The append col.</param>
        public void InsertCol(uint insertAt, uint appendCol)
        {
            try
            {
                //modify drawing refrence
                var changeColID = 0u;
                var colKey = string.Empty;
                var drawingPart = this._worksheetPart.GetPartsOfType<DrawingsPart>().ToList().FirstOrDefault();
                if (drawingPart != null)
                {
                    var picList = drawingPart.WorksheetDrawing.Descendants<Drawing.TwoCellAnchor>().Where(d => Convert.ToInt32(d.FromMarker.ColumnId.Text) > insertAt);
                    foreach (var anchor in picList)
                    {
                        changeColID = Convert.ToUInt32(anchor.FromMarker.ColumnId.Text) + appendCol;
                        anchor.FromMarker.ColumnId.Text = changeColID.ToString();

                        changeColID = Convert.ToUInt32(anchor.ToMarker.ColumnId.Text) + appendCol;
                        anchor.ToMarker.ColumnId.Text = changeColID.ToString();
                    }
                }

                //modify sheetData refrence
                var sheetData = this._worksheetPart.Worksheet.GetFirstChild<SheetData>();
                var cellList = sheetData.Descendants<Cell>().Where(d => this.MatchColIndex(d.CellReference.Value) > insertAt).ToList();
                foreach (Cell cell in cellList)
                {
                    var rowId = this.MatchRowIndex(cell.CellReference.Value);
                    changeColID = this.MatchColIndex(cell.CellReference.Value) + appendCol;
                    cell.CellReference = new StringValue($"{BaseHelper.GetExcelColKey(changeColID)}{rowId}");
                }

                //modify mergeCells
                var mergeCellList = this._worksheetPart.Worksheet.Descendants<MergeCell>().ToList();
                foreach (var mergeCell in mergeCellList)
                {
                    var arrReference = mergeCell.Reference.Value.Split(':');
                    var startRow = this.MatchRowIndex(arrReference[0]);
                    var startCol = this.MatchColIndex(arrReference[0]);
                    var endRow = this.MatchRowIndex(arrReference[1]);
                    var endCol = this.MatchColIndex(arrReference[1]);
                    if (startCol <= insertAt)
                        continue;

                    mergeCell.Reference = new StringValue($"{BaseHelper.GetExcelColKey(startCol + appendCol)}{startRow}:{BaseHelper.GetExcelColKey(endCol + appendCol)}{ endRow}");
                }
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Adds the hyperlinks.
        /// </summary>
        /// <param name="cellReference">The cell reference.</param>
        /// <param name="url">The URL.</param>
        public void AddHyperlinks(string cellReference, string url)
        {
            try
            {
                var hyperlinks = this._worksheetPart.Worksheet.GetFirstChild<Hyperlinks>();
                if (hyperlinks == null)
                {
                    var pageMargins = this._worksheetPart.Worksheet.GetFirstChild<PageMargins>();
                    hyperlinks = new Hyperlinks();
                    this._worksheetPart.Worksheet.InsertBefore<Hyperlinks>(hyperlinks, pageMargins);
                }

                var id = $"lnk_{cellReference}";
                var hyperlink = hyperlinks.Descendants<Hyperlink>().FirstOrDefault(d => d.Id == id);
                if (hyperlink == null)
                {
                    hyperlink = new Hyperlink();
                    hyperlinks.Append(hyperlink);
                }
                hyperlink.Id = id;
                hyperlink.Reference = cellReference;

                var absoluteURL = new Uri(url, System.UriKind.Absolute);
                var hyperlinkRelationship = this._worksheetPart.HyperlinkRelationships.FirstOrDefault(d => d.Id == id);
                if (hyperlinkRelationship != null)
                {
                    if (hyperlinkRelationship.Uri != absoluteURL)
                    {
                        this._worksheetPart.DeleteReferenceRelationship(id);
                        this._worksheetPart.AddHyperlinkRelationship(absoluteURL, true, hyperlink.Id);
                    }
                }
                else
                {
                    this._worksheetPart.AddHyperlinkRelationship(absoluteURL, true, hyperlink.Id);
                }
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        #endregion

        #endregion

        #region Private Methods
        /// <summary>
        /// Matches the col key.
        /// </summary>
        /// <param name="cellReference">The cell reference.</param>
        /// <returns></returns>
        private string MatchColKey(string cellReference)
        {
            try
            {
                return this._regexColKey.Match(cellReference).Value;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Matches the index of the col.
        /// </summary>
        /// <param name="cellReference">The cell reference.</param>
        /// <returns></returns>
        private uint MatchColIndex(string cellReference)
        {
            try
            {
                var colKey = this._regexColKey.Match(cellReference).Value;
                return BaseHelper.GetExcelColIndex(colKey);
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Matches the index of the row.
        /// </summary>
        /// <param name="cellReference">The cell reference.</param>
        /// <returns></returns>
        private uint MatchRowIndex(string cellReference)
        {
            try
            {
                return Convert.ToUInt32(this._regexRowIndex.Match(cellReference).Value);
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Gets the row.
        /// </summary>
        /// <param name="sheetData">The sheet data.</param>
        /// <param name="rowIndex">Index of the row.</param>
        /// <returns></returns>
        private Row GetRow(SheetData sheetData, uint rowIndex)
        {
            try
            {
                var insertIndex = 0;
                var i = 0;
                var curRow = (Row)null;
                var rowList = sheetData.Elements<Row>().ToList();
                foreach (Row item in rowList)
                {
                    //search exist row
                    if (item.RowIndex == rowIndex)
                    {
                        curRow = item;
                        break;
                    }

                    //search insert after row index
                    i++;
                    if (item.RowIndex < rowIndex)
                        insertIndex = i;
                }

                //create new row
                if (curRow == null)
                {
                    curRow = new Row();
                    curRow.RowIndex = rowIndex;
                    sheetData.InsertAt(curRow, insertIndex);
                }
                return curRow;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Gets the cell.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="colIndex">Index of the col.</param>
        /// <returns></returns>
        private Cell GetCell(Row row, uint colIndex)
        {
            try
            {
                var insertIndex = 0;
                var i = 0;
                var curCell = (Cell)null;
                var cellList = row.Elements<Cell>().ToList();
                foreach (var item in cellList)
                {
                    var cellColIndex = this.MatchColIndex(item.CellReference);
                    //search exist col
                    if (cellColIndex == colIndex)
                    {
                        curCell = item;
                        break;
                    }

                    //search insert after row index
                    i++;
                    if (cellColIndex < colIndex)
                        insertIndex = i;
                }

                if (curCell == null)
                {
                    curCell = new Cell();
                    curCell.CellReference = new StringValue(BaseHelper.GetExcelColKey(colIndex) + row.RowIndex.ToString());
                    if (row.StyleIndex != null)
                        curCell.StyleIndex = row.StyleIndex;

                    row.InsertAt(curCell, insertIndex);
                }
                return curCell;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Gets the cell.
        /// </summary>
        /// <param name="cell">The cell.</param>
        /// <param name="value">The value.</param>
        /// <param name="styleIndex">Index of the style.</param>
        private void SetCellValue(Cell cell, object value, uint? styleIndex)
        {
            try
            {
                if (value == null)
                    cell.DataType = CellValues.String;
                else
                {
                    switch (value.GetType().Name)
                    {
                        case "Int32":
                        case "Double":
                            cell.DataType = CellValues.Number;
                            break;
                        default:
                            cell.DataType = CellValues.String;
                            break;
                    }
                }
                cell.CellValue = new CellValue(Convert.ToString(value));
                if (styleIndex.HasValue)
                    cell.StyleIndex = styleIndex.Value;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Gets the merge cells.
        /// </summary>
        /// <param name="worksheet">The worksheet.</param>
        /// <returns></returns>
        private MergeCells GetMergeCells(Worksheet worksheet)
        {
            try
            {
                var mergeCells = worksheet.GetFirstChild<MergeCells>();
                if (mergeCells != null)
                    return mergeCells;

                mergeCells = new MergeCells();
                var sheetView = worksheet.GetFirstChild<CustomSheetView>();
                if (sheetView != null)
                {
                    worksheet.InsertAfter(mergeCells, sheetView);
                    return mergeCells;
                }

                var consolidate = worksheet.GetFirstChild<DataConsolidate>();
                if (consolidate != null)
                {
                    worksheet.InsertAfter(mergeCells, consolidate);
                    return mergeCells;
                }

                var sortState = worksheet.GetFirstChild<SortState>();
                if (sortState != null)
                {
                    worksheet.InsertAfter(mergeCells, sortState);
                    return mergeCells;
                }

                var autoFilter = worksheet.GetFirstChild<AutoFilter>();
                if (autoFilter != null)
                {
                    worksheet.InsertAfter(mergeCells, autoFilter);
                    return mergeCells;
                }

                var scenarios = worksheet.GetFirstChild<Scenarios>();
                if (scenarios != null)
                {
                    worksheet.InsertAfter(mergeCells, scenarios);
                    return mergeCells;
                }

                var protectedRanges = worksheet.GetFirstChild<ProtectedRanges>();
                if (protectedRanges != null)
                {
                    worksheet.InsertAfter(mergeCells, protectedRanges);
                    return mergeCells;
                }

                var sheetProtection = worksheet.GetFirstChild<SheetProtection>();
                if (sheetProtection != null)
                {
                    worksheet.InsertAfter(mergeCells, sheetProtection);
                    return mergeCells;
                }

                var sheetCal = worksheet.GetFirstChild<SheetCalculationProperties>();
                if (sheetCal != null)
                {
                    worksheet.InsertAfter(mergeCells, sheetCal);
                    return mergeCells;
                }

                var sheetData = worksheet.GetFirstChild<SheetData>();
                worksheet.InsertAfter(mergeCells, sheetData);
                return mergeCells;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// Loads the fill style sheet.
        /// </summary>
        /// <param name="workBookPart">The work book part.</param>
        /// <param name="color">The color.</param>
        /// <returns></returns>
        private uint GetBGColorIndex(WorkbookPart workBookPart, string color)
        {
            try
            {
                if (this._dicFillStylesIndex.ContainsKey(color))
                    return this._dicFillStylesIndex[color];

                var styleSheet = workBookPart.WorkbookStylesPart.Stylesheet;
                if (styleSheet.Fills == null)
                    styleSheet.Fills = new Fills();

                var fill = new Fill();
                var patternFill = new PatternFill() { PatternType = PatternValues.Solid };
                var foregroundColor = new ForegroundColor() { Rgb = color };
                var backgroundColor = new BackgroundColor() { Indexed = (UInt32Value)64U };
                patternFill.Append(foregroundColor);
                patternFill.Append(backgroundColor);
                fill.Append(patternFill);

                styleSheet.Fills.Append(fill);
                var colorIndex = (uint)styleSheet.Fills.Count() - 1;
                this._dicFillStylesIndex.Add(color, colorIndex);
                return colorIndex;
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
    /// BorderStyle
    /// </summary>
    public enum XBorder
    {
        None = 0,
        Thin = 1,
        Medium = 2,
        Dashed = 3,
        Dotted = 4,
        Thick = 5,
        Double = 6,
        Hair = 7,
        MediumDashed = 8,
        DashDot = 9,
        MediumDashDot = 10,
        DashDotDot = 11,
        MediumDashDotDot = 12,
        SlantDashDot = 13
    }

    /// <summary>
    /// HorizontalAlignment
    /// </summary>
    public enum XHorizontal
    {
        General = 0,
        Left = 1,
        Center = 2,
        Right = 3,
        Fill = 4,
        Justify = 5,
        CenterContinuous = 6,
        Distributed = 7
    }

    /// <summary>
    /// VerticalAlignment
    /// </summary>
    public enum XVertical
    {
        Top = 0,
        Center = 1,
        Bottom = 2,
        Justify = 3,
        Distributed = 4
    }

    /// <summary>
    /// Excel Font Style
    /// </summary>
    public class XFontStyle
    {
        public string Name { get; set; }
        public double? Size { get; set; }
        public string Color { get; set; }
        public bool Bold { get; set; }
        public bool Italic { get; set; }
        public bool Underline { get; set; }
    }

    /// <summary>
    /// ExcelAlignStyle
    /// </summary>
    public class ExcelAlignStyle
    {
        public bool WrapText { get; set; }
        public XHorizontal Horizontal { get; set; }
        public XVertical Vertical { get; set; }
    }

    /// <summary>
    /// Excel Border Style
    /// </summary>
    public class ExcelBorderStyle
    {
        public string Color { get; set; }
        public bool Left { get; set; }
        public bool Right { get; set; }
        public bool Top { get; set; }
        public bool Bottom { get; set; }
        public XBorder Border { get; set; }
    }
    /// <summary>
    /// Excel Format
    /// </summary>
    public class ExcelFormat
    {
        /// <summary>
        /// ID=1 formatCode: 0
        /// </summary>
        public static readonly string Number1 = "SysNumber1";

        /// <summary>
        /// ID=2 formatCode: 0.00
        /// </summary>
        public static readonly string Number2 = "SysNumber2";

        /// <summary>
        /// ID=3 formatCode: #,##0
        /// </summary>
        public static readonly string Number3 = "SysNumber3";

        /// <summary>
        /// ID=37 formatCode: #,##0 ;(#,##0)
        /// </summary>
        public static readonly string Number4 = "SysNumber4";

        /// <summary>
        /// ID=38 formatCode: #,##0 ;[Red](#,##0)
        /// </summary>
        public static readonly string Number5 = "SysNumber5";

        /// <summary>
        /// ID=39 formatCode: #,##0.00;(#,##0.00)
        /// </summary>
        public static readonly string Number6 = "SysNumber6";

        /// <summary>
        /// ID=40 formatCode: #,##0.00;[Red](#,##0.00)
        /// </summary>
        public static readonly string Number7 = "SysNumber7";

        /// <summary>
        /// ID=9 formatCode: 0%
        /// </summary>
        public static readonly string Percentage1 = "SysPercentage1";

        /// <summary>
        /// ID=10 formatCode: 0.00%
        /// </summary>
        public static readonly string Percentage2 = "SysPercentage2";

        /// <summary>
        /// ID=14 formatCode: mm-dd-yy
        /// </summary>
        public static readonly string Date1 = "SysDate1";

        /// <summary>
        /// ID=15 formatCode: d-mmm-yy
        /// </summary>
        public static readonly string Date2 = "SysDate2";

        /// <summary>
        /// ID=16 formatCode: d-mmm
        /// </summary>
        public static readonly string Date3 = "SysDate3";

        /// <summary>
        /// ID=17 formatCode: mmm-yy
        /// </summary>
        public static readonly string Date4 = "SysDate4";

        /// <summary>
        /// ID=18 formatCode: h:mm AM/PM
        /// </summary>
        public static readonly string Date5 = "SysDate5";

        /// <summary>
        /// ID=19 formatCode: h:mm:ss AM/PM
        /// </summary>
        public static readonly string Date6 = "SysDate6";

        /// <summary>
        /// ID=20 formatCode: h:mm
        /// </summary>
        public static readonly string Date7 = "SysDate7";

        /// <summary>
        /// ID=21 formatCode: h:mm:ss
        /// </summary>
        public static readonly string Date8 = "SysDate8";

        /// <summary>
        /// ID=22 formatCode: m/d/yy h:mm
        /// </summary>
        public static readonly string Date9 = "SysDate9";

        /// <summary>
        /// ID=45 formatCode: mm:ss
        /// </summary>
        public static readonly string Date10 = "SysDate10";
    }
    #endregion
}
