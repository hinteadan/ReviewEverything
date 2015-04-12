using System.Collections.Generic;
using System.Threading.Tasks;
using Recognos.Core;
using ReviewEverything.Model;
using NLog;

namespace ReviewEverything.DataProvider
{
    public abstract class WebsiteSearch : ICanBeSearched
    {
        protected abstract string UrlPattern { get; }

        protected Logger Log { get; private set; }

        protected WebsiteSearch()
        {
            this.Log = LogManager.GetLogger(this.GetType().FullName);
        }

        public IEnumerable<ICanBeParsed> SearchFor(SearchCriteria criteria)
        {
            Log.Trace("Search Website for: {0}", criteria.RawValue);

            var url = SearchEndpointFor(criteria);
            var content = HttpOperations.Get(url).Result;
            if (content == null)
            {
                Log.Warn("Error parsing website search results. No content can be load from: {0}", url);
                return null;
            }

            return ParseSearchResult(content);
        }

        protected virtual string SearchEndpointFor(SearchCriteria criteria)
        {
            Check.NotEmpty(UrlPattern, "UrlPattern");

            return string.Format(UrlPattern, criteria.UriFriendly());
        }

        protected abstract IEnumerable<ICanBeParsed> ParseSearchResult(string content);

        public Task<IEnumerable<ICanBeParsed>> SearchForAsync(SearchCriteria criteria)
        {
            return Task.Run<IEnumerable<ICanBeParsed>>(() => this.SearchFor(criteria));
        }
    }
}
