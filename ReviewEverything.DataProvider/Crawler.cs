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

        public IEnumerable<ReviewItem> Crawl(SearchCriteria criteria)
        {
            return CrawlAsync(criteria).Result;
        }

        public async Task<IEnumerable<ReviewItem>> CrawlAsync(SearchCriteria criteria)
        {
            var searchResuts = await Task.WhenAll(dataSources.Select(s => s.SearchForAsync(criteria)));

            var reviewItems = await Task.WhenAll(searchResuts.SelectMany(x => x).Select(i => i.ParseAsync()));

            return reviewItems;
        }

    }
}
