using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReviewEverything.Model.Reports
{
    public interface ICanReport
    {
        void Generate(SearchCriteria criteria, IEnumerable<ReviewItem> results);
    }
}
