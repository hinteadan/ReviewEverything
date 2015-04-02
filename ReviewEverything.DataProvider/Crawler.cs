using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReviewEverything.DataProvider.Storage;
using ReviewEverything.Model;

namespace ReviewEverything.DataProvider
{
    public class Crawler
    {
        private readonly ICanBeSearched[] dataSources = new ICanBeSearched[] 
        { 
            new eMag.EMagSearch(),
            new CelRo.CelRoSearch()
        };
        private readonly ICanStore localStore = new LocalStore();

        private async Task<IEnumerable<ReviewItem>> CrawlAsync(SearchCriteria criteria)
        {
            var crawlTasks = dataSources.Select(s => new Task<IEnumerable<ICanBeParsed>>(() => s.SearchFor(criteria))).ToArray();

            var searchResults = await Task.WhenAll(crawlTasks);

            var parseTasks = searchResults.Select(r => Parse(r));

            throw new NotImplementedException();

            //return Task.WhenAll(parseTasks).ContinueWith<ReviewItem[]>(async x => new Task (await x).SelectMany(r => r).ToArray());
        }

        private async Task<ReviewItem[]> Parse(IEnumerable<ICanBeParsed> searchResults)
        {
            var parseTasks = searchResults.Select(x => new Task<ReviewItem>(() => x.Parse()));

            return await Task<IEnumerable<ReviewItem>>.WhenAll(parseTasks);
        }

    }
}
