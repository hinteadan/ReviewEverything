using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;

namespace ReviewEverything.Model.Reports
{
    public abstract class ReportBase : ICanReport
    {
        private const string configKeyReportsBasePath = "Reports.Path";

        private readonly string defaultReportsBasePath = string.Format(@"{0}\Reports", Directory.GetCurrentDirectory());

        public abstract void Generate(SearchCriteria criteria, IEnumerable<ReviewItem> results);

        protected string ReportPath(string reportFileNameFormat, params object[] args)
        {
            return string.Format(@"{0}\{1}", GetReportsBasePath(), string.Format(reportFileNameFormat, args));
        }

        protected string GetReportsBasePath()
        {
            string configEntry = ConfigurationManager.AppSettings[configKeyReportsBasePath];
            if(!string.IsNullOrWhiteSpace(configEntry))
            {
                return configEntry;
            }

            EnsureDefaultPath();

            return defaultReportsBasePath;
        }

        private void EnsureDefaultPath()
        {
            if (Directory.Exists(defaultReportsBasePath))
            {
                EnsureAccessToDirectoryPath();
                return;
            }

            Directory.CreateDirectory(defaultReportsBasePath);
        }

        private void EnsureAccessToDirectoryPath()
        {
            Directory.GetAccessControl(defaultReportsBasePath);
        }
    }
}
