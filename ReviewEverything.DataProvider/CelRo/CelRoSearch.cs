using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;
using ReviewEverything.Model;

namespace ReviewEverything.DataProvider.CelRo
{
    public class CelRoSearch : WebsiteSearch
    {
        protected override string UrlPattern
        {
            get
            {
                return "http://www.cel.ro/cauta/{0}/1/1";
            }
        }

        protected override IEnumerable<ICanBeParsed> ParseSearchResult(string content)
        {
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(content);

            Log.Trace("Parsing CEL.ro search results");

            var resultsArea = htmlDoc.DocumentNode.Descendants().WithClass("productlisting").Single();

            if (resultsArea.Descendants("div").WithClass("fara_produse").SingleOrDefault() != null)
            {
                Log.Trace("Found no results, no review items to parse");
                return Enumerable.Empty<ICanBeParsed>();
            }

            return ParseSearchResultHtmlItems(resultsArea);
        }

        private IEnumerable<ICanBeParsed> ParseSearchResultHtmlItems(HtmlNode resultsArea)
        {
            Log.Trace("Parsing search result items");
            return resultsArea
                .Descendants("div")
                .WithClass("productListing-tot")
                .Select(x => ParseResultHtmlItem(x))
                .ToArray();
        }

        private ICanBeParsed ParseResultHtmlItem(HtmlNode item)
        {
            Log.Trace("Parsing search result item");

            var imageNode = item.Descendants("div").WithClass("productListing-poza").Single();
            var imageUrl = imageNode.Descendants("a").Single().FirstChild.Attributes["src"].Value;

            var detailsNode = item.Descendants("div").WithClass("productListing-nume").Single();
            var productAnchor = detailsNode.Descendants("h2").First().FirstChild;
            var priceNode = detailsNode.Descendants("div").WithClass("pret_n").Single();
            var price = decimal.Parse(priceNode.FirstChild.InnerText.Trim());
            var currency = priceNode.Descendants("b").Last().InnerText.Trim();

            var oldPriceString = detailsNode.Descendants("div").WithClass("pret_v").Single().InnerText;
            decimal? oldPrice = null;
            if(!string.IsNullOrWhiteSpace(oldPriceString))
            {
                oldPrice = decimal.Parse(oldPriceString.Substring(0, oldPriceString.IndexOf(' ')));
            }


            var productDetailsUrl = productAnchor.Attributes["href"].Value;
            var productName = productAnchor.InnerText.Trim();

            Log.Trace("Successfully parsed item '{0}' ; Reference: {1}", productName, productDetailsUrl);

            return new CelRoProductDetails(productDetailsUrl)
            {
                Name = productName,
                ThumbnailUrl = imageUrl,
                Price = price,
                Currency = currency,
                OldPrice = oldPrice
            };
        }
    }
}
