using ReviewEverything.Model;
using ReviewEverything.DataProvider;
using NLog.Config;
using NLog.Targets;
using NLog;
using System;
using System.Linq;
using System.Threading.Tasks;
using ReviewEverything.DataProvider.AmazonCom;

namespace Playground
{
    class Program
    {
        static void Main(string[] args)
        {
            ConfigureLogging();

            var crawler = new Crawler();

            var criterias = new SearchCriteria[] { 
                new SearchCriteria("Nokia Lumia 930")
            };

            var items = Task.WhenAll(criterias.Select(c => crawler.Crawl(c))).Result.SelectMany(x => x.ToArray());

            var rating = items.Rating();

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
