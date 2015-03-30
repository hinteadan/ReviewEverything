using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReviewEverything.DataProvider.CelRo;
using ReviewEverything.Model;

namespace Playground
{
    class Program
    {
        static void Main(string[] args)
        {
            var result = new CelRoSearch().SearchFor(new SearchCriteria("Nokia Lumia 930"));

            var item = result.First().Parse();
        }
    }
}
