﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Recognos.Core;
using ReviewEverything.Model;
using HtmlAgilityPack;
using ReviewEverything.DataProvider;
using System.Globalization;

namespace ReviewEverything.DataProvider.CelRo
{
    public class CelRoProductDetails : ICanBeParsed
    {
        private readonly string productDetailsUrl;

        public CelRoProductDetails(string url)
        {
            Check.Condition(Uri.IsWellFormedUriString(url, UriKind.Absolute), "The given product details URL is invalid!");

            this.productDetailsUrl = url;
        }

        public string Name { get; set; }
        public string ThumbnailUrl { get; set; }
        public decimal Price { get; set; }
        public decimal? OldPrice { get; set; }
        public string Currency { get; set; }


        public ReviewItem Parse()
        {
            string content = HttpOperations.Get(this.productDetailsUrl);

            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(content);

            var prodInfoNode = htmlDoc.DocumentNode.Descendants("div").WithClass("prod_info").Single();

            var name = prodInfoNode.Descendants("h2").WithAttribute("itemprop", "name").Single().InnerText;
            var descriptionHtml = prodInfoNode.Descendants("div").WithClass("descriere").Single().InnerHtml;

            var imagesTable = htmlDoc.GetElementbyId("pzx");
            var mainImageUrl = imagesTable.Descendants("td").First().Descendants("img").WithAttribute("itemprop", "image").Single().Attributes["src"].Value;
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

            return new ReviewItem(new Uri(this.productDetailsUrl, UriKind.Absolute))
                {
                    Name = name,
                    Description = descriptionHtml,
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
            string dateTimeFormat = "'a scris pe' d MMM yyyy 'la' HH:mm";
            var nameNodes = prodInfoNode.Elements("div").WithClass("review_nume").ToArray();
            var commentNodes = prodInfoNode.Elements("div").WithClass("review_coment").ToArray();

            for(var i = 0; i < nameNodes.Length; i++)
            {
                var name = nameNodes[i].Descendants("b").First().InnerText;
                var comment = commentNodes[i].InnerText;
                var timestamp = DateTime.ParseExact(nameNodes[i].Descendants("span").First().InnerText, dateTimeFormat, CultureInfo.InvariantCulture);
                var ratingImg = nameNodes[i].Descendants("img").FirstOrDefault();
                var ratingString = ratingImg != null ? nameNodes[i].Descendants("img").First().GetAttributeValue("title", null) : null;

                byte? rating = null;
                if(ratingString != null)
                {
                    rating = byte.Parse(ratingString.Replace(" din 5 Stele", string.Empty).Trim());
                }

                yield return new ReviewItem.Impression
                {
                    By = name,
                    Comment = comment,
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

        private string CleanSpecName(string name)
        {
            var trimmed = name.Trim();
            if (trimmed[trimmed.Length - 1] == ':')
            {
                return trimmed.Substring(0, trimmed.Length - 1);
            }
            return trimmed;
        }
    }
}
