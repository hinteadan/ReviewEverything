using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReviewEverything.Model
{
    public interface ICanStore : ICanBeSearched
    {
        void Persist(SearchCriteria criteria, IEnumerable<ReviewItem> items);
        ReviewItem Retrieve(Uri reference);
        IEnumerable<T> Find<T>(Predicate<Dictionary<string, string>> indexPredicate);

        Task PersistAsync(SearchCriteria criteria, IEnumerable<ReviewItem> items);
        Task<ReviewItem> RetrieveAsync(Uri reference);
    }
}
