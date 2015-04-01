using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReviewEverything.Model;

namespace ReviewEverything.DataProvider.eMag
{
    public class EMagSearch : ICanBeSearched
    {

        public IEnumerable<ICanBeParsed> SearchFor(SearchCriteria criteria)
        {
            throw new NotImplementedException();
        }
    }
}
