using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReviewEverything.Model.Reports
{
    public interface ICanReport<TProjection>
    {
        TProjection Generate(SearchCriteria criteria, IEnumerable<ReviewItem> results);
    }
}
