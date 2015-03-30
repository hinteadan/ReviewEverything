using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Recognos.Core;
using ReviewEverything.Model;
using HtmlAgilityPack;
using ReviewEverything.DataProvider;

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

            return new ReviewItem(new Uri(this.productDetailsUrl, UriKind.Absolute))
                {
                    Name = name,
                    Description = descriptionHtml,
                    MainImageUrl = mainImageUrl,
                    ImagesUrls = otherImages,
                    Price = price,
                    OldPrice = this.OldPrice,
                    Currency = currency
                };
        }
    }
}
