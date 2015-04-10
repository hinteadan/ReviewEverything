using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;

namespace ReviewEverything.Model.Reports.Excel
{
    public class ExcelReport : ReportBase
    {
        private static readonly string[] columns = new string[] { "Name", "URL", "Price", "Currency", "Rating", "Impressions" };
        private static readonly int[] columnWidth = new int[] { 50, 50, 10, 10, 15, 15 };

        public override void Generate(SearchCriteria criteria, IEnumerable<ReviewItem> results)
        {
            var workbook = new XSSFWorkbook();
            var sheet = workbook.CreateSheet(SheetSafeName(criteria));

            GenerateHeader(sheet, criteria, results);

            GenerateSummary(sheet, criteria, results);

            GenerateBody(sheet, criteria, results);

            using (var f = File.OpenWrite(ReportPath("{0}.xlsx", criteria.FileNameFriendly())))
            {
                workbook.Write(f);
            }
        }

        private void GenerateHeader(ISheet sheet, SearchCriteria criteria, IEnumerable<ReviewItem> results)
        {
            string value = string.Format("Search results for \"{0}\"", criteria.RawValue);
            int criteriaIndex = value.IndexOf(criteria.RawValue);
            int criteriaEndIndex = criteriaIndex + criteria.RawValue.Length;

            XSSFRichTextString headerText = new XSSFRichTextString(value);

            IFont font = sheet.Workbook.CreateFont();
            font.FontHeightInPoints = 18;

            headerText.ApplyFont(font);

            IFont fontForhighlight = sheet.Workbook.CreateFont();
            fontForhighlight.FontHeightInPoints = font.FontHeightInPoints;
            fontForhighlight.IsItalic = true;
            fontForhighlight.Boldweight = (short)FontBoldWeight.Bold;

            headerText.ApplyFont(criteriaIndex, criteriaEndIndex, fontForhighlight);

            var cell = sheet.CreateRow(0).CreateCell(0);
            cell.SetCellValue(headerText);

            var cellStyle = sheet.Workbook.CreateCellStyle();
            cellStyle.Alignment = HorizontalAlignment.Left;
            cellStyle.VerticalAlignment = VerticalAlignment.Center;
            cellStyle.Indention = 2;

            cell.CellStyle = cellStyle;
            cell.Row.HeightInPoints = 50;

            sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 16383));
        }

        private void GenerateSummary(ISheet sheet, SearchCriteria criteria, IEnumerable<ReviewItem> results)
        {
            string prices = string.Join(", ",
                results.GroupBy(r => r.Currency)
                .OrderBy(g => g.Key)
                .Select(g => string.Format("{0}-{1}{2}", g.Min(x => x.Price).ToString(CultureInfo.InvariantCulture), g.Max(x => x.Price).ToString(CultureInfo.InvariantCulture), g.Key))
                );

            string value = string.Format("Price ranges: {0}", prices);
            int valueIndex = value.IndexOf(prices);
            int valueEndIndex = valueIndex + prices.Length;
            GenerateSummaryRow(sheet, 1, value, valueIndex, valueEndIndex);



            double rating = results.Rating() ?? -1;
            string ratingString = rating >= 0 ? string.Format("{0}%", rating.ToString(CultureInfo.InvariantCulture)) : "N/A";

            value = string.Format("Overall rating: {0}", ratingString);
            valueIndex = value.IndexOf(ratingString);
            valueEndIndex = valueIndex + ratingString.Length;
            GenerateSummaryRow(sheet, 2, value, valueIndex, valueEndIndex);



            string numberOfImpressions = results.Sum(r => r.Impressions.Length).ToString(CultureInfo.InvariantCulture);

            value = string.Format("Based on {0} impression(s)", numberOfImpressions);
            valueIndex = value.IndexOf(numberOfImpressions);
            valueEndIndex = valueIndex + numberOfImpressions.Length;
            GenerateSummaryRow(sheet, 3, value, valueIndex, valueEndIndex);
        }

        private void GenerateSummaryRow(ISheet sheet, int rowIndex, string summary, int highlightIndex, int highlightEndIndex)
        {
            XSSFRichTextString summaryText = new XSSFRichTextString(summary);

            IFont font = sheet.Workbook.CreateFont();
            font.FontHeightInPoints = 12;

            summaryText.ApplyFont(font);

            IFont fontForhighlight = sheet.Workbook.CreateFont();
            fontForhighlight.FontHeightInPoints = font.FontHeightInPoints;
            fontForhighlight.Boldweight = (short)FontBoldWeight.Bold;

            summaryText.ApplyFont(highlightIndex, highlightEndIndex, fontForhighlight);

            var cell = sheet.CreateRow(rowIndex).CreateCell(0);
            cell.SetCellValue(summaryText);

            var cellStyle = sheet.Workbook.CreateCellStyle();
            cellStyle.Alignment = HorizontalAlignment.Left;
            cellStyle.VerticalAlignment = VerticalAlignment.Center;
            cellStyle.Indention = 2;

            cell.CellStyle = cellStyle;
            cell.Row.HeightInPoints = 25;

            sheet.AddMergedRegion(new CellRangeAddress(rowIndex, rowIndex, 0, 16383));
        }

        private void GenerateBody(ISheet sheet, SearchCriteria criteria, IEnumerable<ReviewItem> results)
        {
            GenerateBodyColumns(sheet);

            GenerateBodyEntries(sheet, criteria, results);
        }

        private void GenerateBodyColumns(ISheet sheet)
        {
            var row = sheet.CreateRow(sheet.PhysicalNumberOfRows + 1);

            IFont font = sheet.Workbook.CreateFont();
            font.FontHeightInPoints = 11;
            font.Boldweight = (short)FontBoldWeight.Bold;

            var cellStyle = sheet.Workbook.CreateCellStyle();
            cellStyle.Alignment = HorizontalAlignment.Center;
            cellStyle.VerticalAlignment = VerticalAlignment.Center;
            cellStyle.BorderBottom = cellStyle.BorderRight = cellStyle.BorderTop = BorderStyle.Thin;
            cellStyle.SetFont(font);

            for (var i = 0; i < columns.Length; i++)
            {
                var cell = row.CreateCell(i);

                cell.SetCellValue(columns[i]);
                cell.CellStyle = cellStyle;
                cell.Row.HeightInPoints = 20;
                sheet.SetColumnWidth(i, 256 * columnWidth[i]);
            }
        }

        private void GenerateBodyEntries(ISheet sheet, SearchCriteria criteria, IEnumerable<ReviewItem> results)
        {
            int rowIndex = sheet.PhysicalNumberOfRows + 1;
            foreach(var result in results)
            {
                GenerateBodyEntry(rowIndex, sheet, criteria, result);
                rowIndex++;
            }
        }

        private void GenerateBodyEntry(int rowIndex, ISheet sheet, SearchCriteria criteria, ReviewItem result)
        {
            var row = sheet.CreateRow(rowIndex);
            double rating = result.Rating() ?? -1;
            string[] values = new string[] { 
                result.Name,
                result.Reference.ToString(),
                result.Price.ToString(CultureInfo.InvariantCulture),
                result.Currency,
                rating >=0 ? string.Format("{0}%", rating.ToString(CultureInfo.InvariantCulture)) : "N/A",
                result.Impressions.Length.ToString(CultureInfo.InvariantCulture)
            };

            for (var i = 0; i < values.Length; i++)
            {
                var cell = row.CreateCell(i);

                cell.SetCellValue(values[i]);

                if(columns[i] == "URL")
                {
                    XSSFHyperlink link = new XSSFHyperlink(HyperlinkType.Url);
                    link.Address = values[i];
                    cell.Hyperlink = link;
                }
            }
        }

        private static string SheetSafeName(SearchCriteria criteria)
        {
            return criteria.RawValue.Length > 31 ? criteria.RawValue.Substring(0, 31) : criteria.RawValue;
        }
    }
}
