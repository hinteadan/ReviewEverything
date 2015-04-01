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

            var otherImagesNodes = htmlDoc.GetElementbyId("gallery-popup").Descendants("img").WithClass("zoomImg");
            var otherImgPaths = otherImagesNodes
                .Select(n => string.Format("http:{0}", n.GetAttributeValue("src", null)))
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

        private ReviewItem.Impression[] ParseReviews(HtmlNode reviewsNode)
        {
            throw new NotImplementedException();
        }

        private ReviewItem.Specification[] ParseSpecs(HtmlNode specsNode)
        {
            throw new NotImplementedException();
        }
    }
}
