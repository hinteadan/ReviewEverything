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

namespace Playground
{
    class Program
    {
        static void Main(string[] args)
        {
            ConfigureLogging();

            var crawler = new Crawler();

            var criterias = new SearchCriteria[] { 
                new SearchCriteria("Snickers")
            };

            var items = Task.WhenAll(criterias.Select(c => crawler.Crawl(c))).Result.SelectMany(x => x.ToArray());

            File.WriteAllText(string.Format(@"C:\Users\dan.hintea\Downloads\{0}.csv", criterias[0].FileNameFriendly()), new CsvReport().Generate(criterias[0], items));

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

            LogManager.Configuration = cfg;
        }
    }
}
