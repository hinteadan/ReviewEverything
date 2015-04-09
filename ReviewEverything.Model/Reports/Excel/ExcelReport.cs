using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
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

            using(var f = File.OpenWrite(ReportPath("{0}.xlsx", criteria.FileNameFriendly())))
            {
                workbook.Write(f);
            }
        }

        private void GenerateHeader(ISheet sheet, SearchCriteria criteria, IEnumerable<ReviewItem> results)
        {
            sheet.CreateRow(0).CreateCell(0).SetCellValue(string.Format("Summary for \"{1}\"", results.Count(), criteria.RawValue));
        }

        private void GenerateSummary(ISheet sheet, SearchCriteria criteria, IEnumerable<ReviewItem> results)
        {
            sheet.CreateRow(2).CreateCell(0).SetCellValue(string.Format("{0}% based on {1} impressions", results.Rating(), results.Count()));
        }

        private static string SheetSafeName(SearchCriteria criteria)
        {
            return criteria.RawValue.Length > 31 ? criteria.RawValue.Substring(0, 31) : criteria.RawValue;
        }
    }
}
