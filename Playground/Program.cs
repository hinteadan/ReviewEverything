using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReviewEverything.DataProvider.CelRo;
using ReviewEverything.Model;
using ReviewEverything.DataProvider.LocalStore;

namespace Playground
{
    class Program
    {
        static void Main(string[] args)
        {
            var criteria = new SearchCriteria("sony-xperia-z3-d6653");
            var result = new CelRoSearch().SearchFor(criteria);
            //var result = new CelRoSearch().SearchFor(new SearchCriteria("lumia 930"));

            var item = result.First().Parse();

            var store = new LocalStore();

            store.Persist(criteria, new ReviewItem[]{ item } );
        }
    }
}
