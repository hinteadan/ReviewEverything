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
        private readonly TimeSpan resultsExpireIn = TimeSpan.FromDays(180);

        public IEnumerable<ReviewItem> Crawl(SearchCriteria criteria)
        {
            var existingCriteria = localStore
                .Find<SearchCriteria>(f => string.Equals(f["Value"], criteria.RawValue, StringComparison.InvariantCultureIgnoreCase))
                .SingleOrDefault();

            if(existingCriteria != null && !IsCriteriaExpired(existingCriteria))
            {
                return localStore.SearchFor(existingCriteria).Select(r => r.Parse());
            }
            else if(IsCriteriaExpired(existingCriteria))
            {
                CleanupExpiredResultsForCriteria(existingCriteria);
            }

            var result = CrawlAsync(criteria).Result;
            localStore.Persist(criteria, result);
            return result;
        }

        private void CleanupExpiredResultsForCriteria(SearchCriteria existingCriteria)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<ReviewItem>> CrawlAsync(SearchCriteria criteria)
        {
            var searchResuts = await Task.WhenAll(dataSources.Select(s => s.SearchForAsync(criteria)));

            var reviewItems = await Task.WhenAll(searchResuts.SelectMany(x => x).Select(i => i.ParseAsync()));

            return reviewItems;
        }

        private bool IsCriteriaExpired(SearchCriteria criteria)
        {
            return DateTime.Now - criteria.CreatedOn > resultsExpireIn;
        }
    }
}
