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
        public override void Generate(SearchCriteria criteria, IEnumerable<ReviewItem> results)
        {
            var workbook = new XSSFWorkbook();
            var sheet = workbook.CreateSheet(SheetSafeName(criteria));

            GenerateHeader(sheet, criteria, results);

            GenerateSummary(sheet, criteria, results);

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

            IFont fontForKeyword = sheet.Workbook.CreateFont();
            fontForKeyword.FontHeightInPoints = font.FontHeightInPoints;
            fontForKeyword.IsItalic = true;
            fontForKeyword.Boldweight = (short)FontBoldWeight.Bold;

            headerText.ApplyFont(criteriaIndex, criteriaEndIndex, fontForKeyword);

            var cell = sheet.CreateRow(0).CreateCell(0);
            cell.SetCellValue(headerText);

            var cellStyle = sheet.Workbook.CreateCellStyle();
            cellStyle.Alignment = HorizontalAlignment.Left;
            cellStyle.VerticalAlignment = VerticalAlignment.Center;
            cellStyle.Indention = 2;

            cell.CellStyle = cellStyle;
            cell.Row.HeightInPoints = 50;
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
            XSSFRichTextString summaryText = new XSSFRichTextString(value);

            IFont font = sheet.Workbook.CreateFont();
            font.FontHeightInPoints = 11;

            summaryText.ApplyFont(font);

            IFont fontForKeyword = sheet.Workbook.CreateFont();
            fontForKeyword.FontHeightInPoints = font.FontHeightInPoints;
            fontForKeyword.Boldweight = (short)FontBoldWeight.Bold;

            summaryText.ApplyFont(valueIndex, valueEndIndex, fontForKeyword);

            var cell = sheet.CreateRow(1).CreateCell(0);
            cell.SetCellValue(summaryText);

            var cellStyle = sheet.Workbook.CreateCellStyle();
            cellStyle.Alignment = HorizontalAlignment.Left;
            cellStyle.VerticalAlignment = VerticalAlignment.Center;
            cellStyle.Indention = 2;

            cell.CellStyle = cellStyle;
            cell.Row.HeightInPoints = 25;
        }

        private static string SheetSafeName(SearchCriteria criteria)
        {
            return criteria.RawValue.Length > 31 ? criteria.RawValue.Substring(0, 31) : criteria.RawValue;
        }
    }
}
