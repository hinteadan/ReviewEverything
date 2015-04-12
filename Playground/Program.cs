using ReviewEverything.Model;
using ReviewEverything.DataProvider;
using NLog.Config;
using NLog.Targets;
using NLog;
using System;
using System.Linq;
using System.Threading.Tasks;
using ReviewEverything.DataProvider.AmazonCom;
using ReviewEverything.Model.Reports.CSV;
using System.IO;
using ReviewEverything.Model.Reports.Excel;

namespace Playground
{
    class Program
    {
        static void Main(string[] args)
        {
            ConfigureLogging();

            var crawler = new Crawler();

            var criterias = new SearchCriteria[] { 
                new SearchCriteria("MacBook Pro")
            };

            var items = Task.WhenAll(criterias.Select(c => crawler.Crawl(c))).Result.SelectMany(x => x.ToArray());

            new ExcelReport().Generate(criterias[0], items);
            new CsvReport().Generate(criterias[0], items);

            Console.WriteLine("Done");
            Console.ReadLine();
        }

        private static void ConfigureLogging()
        {
            var cfg = new LoggingConfiguration();

            var consoleTarget = new ColoredConsoleTarget();
            cfg.AddTarget("Console", consoleTarget);

            cfg.LoggingRules.Add(new LoggingRule("*", LogLevel.Trace, consoleTarget));


            var fileTarget = new FileTarget();
            fileTarget.FileName = @"Logs\Log.txt";
            cfg.AddTarget("File", fileTarget);

            cfg.LoggingRules.Add(new LoggingRule("*", LogLevel.Trace, fileTarget));


            var errorOnlyFileTarget = new FileTarget();
            errorOnlyFileTarget.FileName = @"Logs\Errors.txt";
            cfg.AddTarget("ErrorFile", errorOnlyFileTarget);

            cfg.LoggingRules.Add(new LoggingRule("*", LogLevel.Warn, errorOnlyFileTarget));

            LogManager.Configuration = cfg;
        }
    }
}
