using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReviewEverything.DataProvider.CelRo;
using ReviewEverything.DataProvider.eMag;
using ReviewEverything.Model;
using ReviewEverything.DataProvider.LocalStore;

namespace Playground
{
    class Program
    {
        static void Main(string[] args)
        {
            var criteria = new SearchCriteria("nokia lumia 930");
            var result = new EMagSearch().SearchFor(criteria);

            var item = result.First().Parse();

            //var store = new LocalStore();
            //var item = store.SearchFor(criteria);

            //store.Persist(criteria, new ReviewItem[]{ item } );
        }
    }
}
