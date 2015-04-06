using ReviewEverything.Model;
using System;
using System.Threading.Tasks;
using System.Linq;
using HtmlAgilityPack;

namespace ReviewEverything.DataProvider.AmazonCom
{
    public class AmazonProduct : WebsiteProductDetails
    {
        public AmazonProduct(string url) : base(url) { }

        protected override ReviewItem Parse(string content)
        {
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(content);

            var descriptionNode = htmlDoc.GetElementbyId("productDescription").Descendants("div").WithClass("productDescriptionWrapper").SingleOrDefault();

            return new ReviewItem(new Uri(this.productDetailsUrl, UriKind.Absolute))
            {
                Name = htmlDoc.GetElementbyId("btAsinTitle").InnerText.Trim(),
                Description = descriptionNode != null ? new ReviewItem.RichContent { Html = descriptionNode.InnerHtml, Text = descriptionNode.InnerText } : null,
                MainImageUrl = htmlDoc.GetElementbyId("main-image").GetAttributeValue("src", null),
                ImagesUrls = ParseOtherImages(htmlDoc.GetElementbyId("thumbs-image")),
                Price = this.Price,
                OldPrice = this.OldPrice,
                Currency = this.Currency,
                Specifications = ParseSpecs(htmlDoc.GetElementbyId("prodDetails")),
                //Impressions = ParseImpressions(prodInfoNode).ToArray()
            };
        }

        private ReviewItem.Specification[] ParseSpecs(HtmlNode detailsNode)
        {
            if(detailsNode == null)
            {
                return new ReviewItem.Specification[0];
            }

            var techDetailsNode = detailsNode
                .Descendants("div")
                .WithClass("techD")
                .FirstOrDefault();

            if(techDetailsNode == null)
            {
                return new ReviewItem.Specification[0];
            }

            return techDetailsNode
                .Descendants("div")
                .WithClass("content")
                .SelectMany(x => x.Descendants("tr"))
                .Select(row => new ReviewItem.Specification{
                    Name = row.Elements("td").WithClass("label").Single().InnerText,
                    Value = row.Elements("td").WithClass("value").Single().InnerText
                }).ToArray();
        }

        private string[] ParseOtherImages(HtmlNode altImagesNode)
        {
            if (altImagesNode == null)
            {
                return new string[0];
            }

            return altImagesNode
                .Elements("img")
                .Select(x => 
                        x.GetAttributeValue("src", string.Empty)
                        .Replace("._SX38_SY50_CR,0,0,38,50_.", "._SX600_SY600_CR,0,0,600,600_.")
                ).ToArray();

        }
    }
}
