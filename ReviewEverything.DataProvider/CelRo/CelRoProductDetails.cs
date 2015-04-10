using System;
using System.Collections.Generic;
using System.Linq;
using ReviewEverything.Model;
using HtmlAgilityPack;
using System.Globalization;

namespace ReviewEverything.DataProvider.CelRo
{
    public class CelRoProductDetails : WebsiteProductDetails
    {
        public CelRoProductDetails(string url) : base(url) { }

        protected override ReviewItem Parse(string content)
        {
            HtmlDocument htmlDoc = new HtmlDocument();

            Log.Trace("Parsing CEL.ro product details from {0}", productDetailsUrl);

            htmlDoc.LoadHtml(content);

            var prodInfoNode = htmlDoc.DocumentNode.Descendants("div").WithClass("prod_info").Single();

            var name = prodInfoNode.Descendants("h2").WithAttribute("itemprop", "name").Single().InnerText;
            var descriptionNode = prodInfoNode.Descendants("div").WithClass("descriere").SingleOrDefault();

            var imagesTable = htmlDoc.GetElementbyId("pzx");
            var mainImageNode = imagesTable.Descendants("td").First().Descendants("img").WithAttribute("itemprop", "image").SingleOrDefault();
            var mainImageUrl = mainImageNode != null ? mainImageNode.GetAttributeValue("src", null) : null;

            var otherimagesNode = imagesTable.Descendants("div").WithClass("poze_secundare").SingleOrDefault();
            var otherImages = otherimagesNode != null ?
                otherimagesNode.Descendants("img").Select(n => n.Attributes["src"].Value).ToArray() :
                new string[0];

            var pricingInfoNode = htmlDoc.GetElementbyId("pret_tabela");
            var priceString = pricingInfoNode.Descendants("div").WithClass("pret_info").Single()
                .Descendants("b").WithAttribute("itemprop", "price").Single().InnerText.Trim();
            var price = decimal.Parse(priceString);
            var currency = pricingInfoNode.Descendants("div").WithClass("pret_info").Single()
                .Descendants("meta").WithAttribute("itemprop", "priceCurrency").Single().Attributes["content"].Value.Trim();

            var specsTableNode = prodInfoNode.Element("table");

            Log.Trace("Successfully parsed CEL.ro product details from {0}", productDetailsUrl);

            return new ReviewItem(new Uri(this.productDetailsUrl, UriKind.Absolute))
            {
                Name = name,
                Description = descriptionNode != null ? new ReviewItem.RichContent { Html = descriptionNode.InnerHtml, Text = descriptionNode.InnerText } : null,
                MainImageUrl = mainImageUrl,
                ImagesUrls = otherImages,
                Price = price,
                OldPrice = this.OldPrice,
                Currency = currency,
                Specifications = ParseSpecs(specsTableNode),
                Impressions = ParseImpressions(prodInfoNode).ToArray()
            };
        }

        private IEnumerable<ReviewItem.Impression> ParseImpressions(HtmlNode prodInfoNode)
        {
            string dateTimeFormat = "'a scris pe' d MMM yyyy 'la' H:mm";
            var nameNodes = prodInfoNode.Elements("div").WithClass("review_nume").ToArray();
            var commentNodes = prodInfoNode.Elements("div").WithClass("review_coment").ToArray();

            for (var i = 0; i < nameNodes.Length; i++)
            {
                var name = nameNodes[i].Descendants("b").First().InnerText;
                var timestamp = DateTime.ParseExact(nameNodes[i].Descendants("span").First().InnerText, dateTimeFormat, CultureInfo.InvariantCulture);
                var ratingImg = nameNodes[i].Descendants("img").FirstOrDefault();
                var ratingString = ratingImg != null ? nameNodes[i].Descendants("img").First().GetAttributeValue("title", null) : null;

                byte? rating = null;
                if (ratingString != null)
                {
                    decimal celRating = decimal.Parse(ratingString.Replace(" din 5 Stele", string.Empty).Trim());
                    rating = (byte)(celRating / 5 * 100);
                }

                yield return new ReviewItem.Impression
                {
                    By = name,
                    Comment = new ReviewItem.RichContent { Text = commentNodes[i].InnerText, Html = commentNodes[i].InnerHtml },
                    On = timestamp,
                    Rating = rating
                };
            }

        }

        private ReviewItem.Specification[] ParseSpecs(HtmlNode specsTableNode)
        {
            if (specsTableNode == null)
            {
                return new ReviewItem.Specification[0];
            }

            return specsTableNode
                .Descendants("tr")
                .Select(row => new ReviewItem.Specification
                {
                    Name = CleanSpecName(row.Descendants("td").WithClass("c3").Single().InnerText),
                    Value = row.Descendants("td").WithClass("c4").Single().InnerText
                })
                .ToArray();
        }
    }
}
