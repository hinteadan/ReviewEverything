using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ReviewEverything.DataProvider.Storage;
using ReviewEverything.Model;
using NLog;

namespace ReviewEverything.DataProvider
{
    public class Crawler
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        private readonly ICanBeSearched[] dataSources = new ICanBeSearched[] 
        { 
            new eMag.EMagSearch(),
            new CelRo.CelRoSearch(),
            new AmazonCom.AmazonSearch(),
        };
        private readonly ICanStore localStore = new LocalStore();
        private readonly TimeSpan resultsExpireIn = TimeSpan.FromDays(180);

        public async Task<IEnumerable<ReviewItem>> Crawl(SearchCriteria criteria)
        {
            log.Trace("Start crawling for '{0}'", criteria.RawValue);

            var existingCriteria = localStore
                .Find<SearchCriteria>(f => string.Equals(f["Value"], criteria.RawValue, StringComparison.InvariantCultureIgnoreCase))
                .SingleOrDefault();

            if(existingCriteria != null && !IsCriteriaExpired(existingCriteria))
            {
                log.Trace("We have local results for '{0}', no need to hit the WWW", criteria.RawValue);
                return localStore.SearchFor(existingCriteria).Select(r => r.Parse());
            }
            else if(existingCriteria != null && IsCriteriaExpired(existingCriteria))
            {
                log.Trace("We have some local results for '{0}', but expired!", criteria.RawValue);
                CleanupExpiredResultsForCriteria(existingCriteria);
            }

            var result = await CrawlAsync(criteria);
            localStore.Persist(criteria, result);
            return result;
        }

        private void CleanupExpiredResultsForCriteria(SearchCriteria existingCriteria)
        {
            log.Trace("Cleaning up local expired results for '{0}'", existingCriteria.RawValue);
            throw new NotImplementedException();
        }

        private async Task<IEnumerable<ReviewItem>> CrawlAsync(SearchCriteria criteria)
        {
            log.Trace("Crawling search results for '{0}'", criteria.RawValue);
            var searchResuts = await Task.WhenAll(dataSources.Select(s => s.SearchForAsync(criteria)));

            log.Trace("Crawling search result items for '{0}'", criteria.RawValue);
            var reviewItems = await Task.WhenAll(searchResuts.SelectMany(x => x).Select(i => i.ParseAsync()));

            return reviewItems;
        }

        private bool IsCriteriaExpired(SearchCriteria criteria)
        {
            return DateTime.Now - criteria.CreatedOn > resultsExpireIn;
        }
    }
}
