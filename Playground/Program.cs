using ReviewEverything.Model;
using ReviewEverything.DataProvider;
using NLog.Config;
using NLog.Targets;
using NLog;
using System;

namespace Playground
{
    class Program
    {
        static void Main(string[] args)
        {
            Configurelogging();

            var criteria = new SearchCriteria("nokia lumia 930");
            var crawler = new Crawler();

            var results = crawler.Crawl(criteria);


            //var result = new EMagSearch().SearchFor(criteria);

            //var item = result.First().Parse();

            //var store = new LocalStore();
            //var item = store.SearchFor(criteria);

            //store.Persist(criteria, new ReviewItem[]{ item } );

            Console.WriteLine("Done");
            Console.ReadLine();
        }

        private static void Configurelogging()
        {
            var cfg = new LoggingConfiguration();

            var consoleTarget = new ColoredConsoleTarget();
            cfg.AddTarget("Console", consoleTarget);

            cfg.LoggingRules.Add(new LoggingRule("*", LogLevel.Trace, consoleTarget));

            LogManager.Configuration = cfg;
        }
    }
}
