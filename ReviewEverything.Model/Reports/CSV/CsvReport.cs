﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReviewEverything.Model.Reports.CSV
{
    public class CsvReport : ICanReport<string>
    {
        private const string header = "Name,URL,Price,Currency,Rating,Impressions";

        public string Generate(SearchCriteria criteria, IEnumerable<ReviewItem> results)
        {
            return string.Format("{0}{1}{2}{1}{3}", header, Environment.NewLine, GenerateReportAggregates(criteria, results), GenerateReportContent(results));
        }

        private string GenerateReportAggregates(SearchCriteria criteria, IEnumerable<ReviewItem> results)
        {
            return string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\"",
                criteria.RawValue,
                string.Empty,
                string.Format("{0} - {1}", results.Min(r => r.Price.ToString(CultureInfo.InvariantCulture)), results.Max(r => r.Price.ToString(CultureInfo.InvariantCulture))),
                string.Join("|", results.Select(r => r.Currency).Distinct()),
                results.Rating(),
                results.SelectMany(r => r.Impressions).Count()
                );
        }

        private string GenerateReportContent(IEnumerable<ReviewItem> results)
        {
            return string.Join(Environment.NewLine, results.OrderByDescending(r => r.Rating()).Select(r => GenerateCsvLine(r)));
        }

        private string GenerateCsvLine(ReviewItem r)
        {
            return string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\"", 
                r.Name.Replace(",", " "),
                r.Reference,
                r.Price.ToString(CultureInfo.InvariantCulture),
                r.Currency,
                r.Rating(),
                r.Impressions.Length
                );
        }
    }
}
