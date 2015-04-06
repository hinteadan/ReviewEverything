using HtmlAgilityPack;
using ReviewEverything.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

namespace ReviewEverything.DataProvider.AmazonCom
{
    public class AmazonSearch : WebsiteSearch
    {
        protected override string UrlPattern
        {
            get { return "http://www.amazon.com/s/ref=nb_sb_noss_1?url=search-alias%3Daps&field-keywords={0}"; }
        }

        protected override IEnumerable<ICanBeParsed> ParseSearchResult(string content)
        {
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(content);

            Log.Trace("Parsing Amazon.com search results");

            if (htmlDoc.GetElementbyId("noResultsTitle") != null)
            {
                Log.Trace("Found no results, no review items to parse");
                return Enumerable.Empty<ICanBeParsed>();
            }

            return ParseSearchResultHtmlItems(htmlDoc.GetElementbyId("s-results-list-atf"));
        }

        private IEnumerable<ICanBeParsed> ParseSearchResultHtmlItems(HtmlNode resultsArea)
        {
            Log.Trace("Parsing search result items");
            return resultsArea
                .Elements("li")
                .Select(x => ParseResultHtmlItem(x))
                .Where(x => x != null)
                .ToArray();
        }

        private ICanBeParsed ParseResultHtmlItem(HtmlNode searchItemNode)
        {
            var itemNameAnchor = searchItemNode.Descendants("a").WithClass("s-access-detail-page").SingleOrDefault();

            if (itemNameAnchor == null)
            {
                return null;
            }

            string url = itemNameAnchor.GetAttributeValue("href", null);
            string name = itemNameAnchor.GetAttributeValue("title", null);

            var itemImageNode = searchItemNode.Descendants("img").WithClass("s-access-image").Single();
            string imageUrl = itemImageNode.GetAttributeValue("src", string.Empty).Replace("._AA160_.", "._AA400_.");

            HtmlNode priceNode = searchItemNode.Descendants("span").WithClass("s-price").SingleOrDefault();
            if (priceNode == null)
            {
                priceNode = searchItemNode.Descendants("span").WithClass("a-color-price").First();
            }
            else if(priceNode.InnerText.StartsWith("$0.00") || priceNode.InnerText.StartsWith("$0.01"))
            {
                priceNode = searchItemNode.Descendants("span").WithClass("a-color-price").Skip(1).First();
            }

            decimal price = ParsePrice(priceNode.InnerText);

            var oldPriceNode = searchItemNode.Descendants("span").WithClass("a-text-strike").FirstOrDefault();
            decimal oldPrice = -1;
            if (oldPriceNode != null)
            {
                oldPrice = ParsePrice(oldPriceNode.InnerText);
            }

            return new AmazonProduct(url)
            {
                Name = name,
                ThumbnailUrl = imageUrl,
                Price = price,
                Currency = "$",
                OldPrice = oldPrice != -1 ? (decimal?)oldPrice : null
            };
        }

        private decimal ParsePrice(string priceString)
        {
            string parseableString = priceString
                .Replace("$", string.Empty);
            if (parseableString.Contains(" - "))
            {
                parseableString = parseableString.Substring(0, parseableString.IndexOf(' '));
            }
            decimal price = -1;
            decimal.TryParse(parseableString, NumberStyles.Any, CultureInfo.InvariantCulture, out price);
            return price;
        }
    }
}
