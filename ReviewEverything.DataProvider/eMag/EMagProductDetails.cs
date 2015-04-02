using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Recognos.Core;
using ReviewEverything.Model;
using HtmlAgilityPack;

namespace ReviewEverything.DataProvider.eMag
{
    public class EMagProductDetails : WebsiteProductDetails
    {
        public EMagProductDetails(string url) : base(url) { }

        protected override ReviewItem Parse(string content)
        {
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(content);

            var name = htmlDoc.GetElementbyId("offer-title").Element("h1").InnerText;
            var detailsNode = htmlDoc.GetElementbyId("description_section");
            var descriptionNode = detailsNode.Descendants("div").WithClass("preview_desc").Single();

            var mainImageNode = htmlDoc.GetElementbyId("zoom-image").Element("img");
            string mainImagePath = string.Format("http:{0}", mainImageNode.GetAttributeValue("src", null));

            var otherImagesNodes = htmlDoc.GetElementbyId("product-pictures-carousel").Descendants("a").WithClass("gallery-image");
            var otherImgPaths = otherImagesNodes
                .Select(n => string.Format("http:{0}", n.GetAttributeValue("href", null)))
                .Where(p => !string.Equals(p, mainImagePath, StringComparison.InvariantCultureIgnoreCase))
                .ToArray();

            var specsNode = htmlDoc.GetElementbyId("box-specificatii-produs");
            var reviewsNode = htmlDoc.GetElementbyId("new_reviews");

            return new ReviewItem(new Uri(this.productDetailsUrl, UriKind.Absolute))
            {
                Name = name,
                Description = new ReviewItem.RichContent { Html = descriptionNode.InnerHtml, Text = descriptionNode.InnerText },
                MainImageUrl = mainImagePath,
                ImagesUrls = otherImgPaths,
                Price = this.Price,
                OldPrice = this.OldPrice,
                Currency = this.Currency,
                Specifications = ParseSpecs(specsNode),
                Impressions = ParseReviews(reviewsNode)
            };
        }

        private ReviewItem.Specification[] ParseSpecs(HtmlNode specsNode)
        {
            var specGridNodes = specsNode.Elements("div")
                .WithClass("holder-specificatii")
                .SelectMany(n =>
                    n.Elements("div")
                    .WithClass("box-specificatie")
                    );

            return specGridNodes.SelectMany(s =>
            {
                var names = s.Elements("p").WithClass("ch_title").ToArray();
                var values = s.Elements("p").WithClass("ch_spec").ToArray();

                ReviewItem.Specification[] specs = new ReviewItem.Specification[names.Length];

                for (int i = 0; i < specs.Length; i++)
                {
                    specs[i] = new ReviewItem.Specification
                    {
                        Name = CleanSpecName(names[i].InnerText),
                        Value = HtmlToText(values[i].InnerHtml).Trim()
                    };
                }

                return specs;

            }).ToArray();
        }

        private ReviewItem.Impression[] ParseReviews(HtmlNode reviewsNode)
        {
            return new ReviewItem.Impression[0];
        }
    }
}
