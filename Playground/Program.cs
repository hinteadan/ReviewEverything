using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playground
{
    class Program
    {
        static void Main(string[] args)
        {
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.Load(@"C:\Users\hinte_000\Downloads\Telefon mobil Nokia 930 Lumia, 32GB, Black - eMAG.ro.htm");

            var productInfoArea = htmlDoc.GetElementbyId("product-info");

            var name = productInfoArea.Descendants().Single(n => n.Id == "offer-title").Descendants("h1").Single().InnerText;

        }
    }
}
