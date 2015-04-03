using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using HtmlAgilityPack;
using ReviewEverything.Model;

namespace ReviewEverything.DataProvider.eMag
{
    public class EMagSearch : WebsiteSearch
    {

        protected override string UrlPattern
        {
            get { return "http://www.emag.ro/search/{0}"; }
        }

        protected override IEnumerable<ICanBeParsed> ParseSearchResult(string content)
        {
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(content);

            Log.Trace("Parsing eMag.ro search results");

            var resultsArea = htmlDoc.GetElementbyId("products-holder");

            if (resultsArea == null)
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
                .WithClass("product-holder-grid")
                .Select(x => ParseResultHtmlItem(x))
                .ToArray();
        }

        private ICanBeParsed ParseResultHtmlItem(HtmlNode itemNode)
        {
            Log.Trace("Parsing search result item");

            var productAnchor = itemNode.Descendants("a").WithClass("link_imagine").Single();
            string url = string.Format("http://www.emag.ro{0}", productAnchor.GetAttributeValue("href", null));
            string name = productAnchor.GetAttributeValue("title", null);
            string thumbUrl = string.Format("http:{0}", productAnchor.Descendants("img").Single().GetAttributeValue("src", null));

            var priceNode = itemNode.Descendants("div").WithClass("pret-produs-listing").Single();
            var currentPriceNode = priceNode.Descendants("span").WithClass("price-over").Single();
            string currency = currentPriceNode.Elements("span").WithClass("money-currency").Single().InnerText;

            decimal currentPrice = ParsePrice(currentPriceNode);

            decimal? oldPrice = null;
            var oldPriceNode = priceNode.Descendants("span").WithClass("initial_price").SingleOrDefault();
            if(oldPriceNode != null)
            {
                oldPrice = ParsePrice(oldPriceNode);
            }

            byte? rating = null;
            var ratingNode = itemNode.Descendants("div").WithClass("star-rating-small-progress").SingleOrDefault();
            if(ratingNode != null)
            {
                string ratingString = ratingNode.GetAttributeValue("style", string.Empty)
                    .ToLowerInvariant()
                    .Replace("width:", string.Empty)
                    .Replace("%", string.Empty)
                    .Trim();
                rating = (byte)Math.Round(float.Parse(ratingString, CultureInfo.InvariantCulture), 0);
            }

            Log.Trace("Successfully parsed item '{0}' ; Reference: {1}", name, url);

            return new EMagProductDetails(url)
                {
                    Name = name,
                    ThumbnailUrl = thumbUrl,
                    Price = currentPrice,
                    Currency = currency,
                    OldPrice = oldPrice,
                    Rating = rating
                };
        }

        private decimal ParsePrice(HtmlNode priceInfoNode)
        {
            string currentPriceStringInt = priceInfoNode.Elements("span").WithClass("money-int").Single().InnerText.Replace(".", string.Empty);
            string currentPriceStringDec = priceInfoNode.Elements("sup").WithClass("money-decimal").Single().InnerText;
            return decimal.Parse(string.Format("{0}.{1}", currentPriceStringInt, currentPriceStringDec), CultureInfo.InvariantCulture);
        }

    }
}
