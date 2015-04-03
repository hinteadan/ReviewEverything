using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Recognos.Core;
using ReviewEverything.Model;

namespace ReviewEverything.DataProvider
{
    public abstract class WebsiteSearch : ICanBeSearched
    {
        protected abstract string UrlPattern { get; }

        public IEnumerable<ICanBeParsed> SearchFor(SearchCriteria criteria)
        {
            return ParseSearchResult(HttpOperations.Get(SearchEndpointFor(criteria)));
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
