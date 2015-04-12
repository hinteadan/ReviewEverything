using ReviewEverything.Model;
using System;
using System.Threading.Tasks;
using System.Linq;
using HtmlAgilityPack;
using System.Globalization;
using System.Collections.Generic;

namespace ReviewEverything.DataProvider.AmazonCom
{
    public class AmazonProduct : WebsiteProductDetails
    {
        private const string reviewDateFormat = "MMMM d, yyyy";

        public AmazonProduct(string url) : base(url) { }

        protected override ReviewItem Parse(string content)
        {
            HtmlDocument htmlDoc = new HtmlDocument();

            Log.Trace("Parsing Amazon.com product details from {0}", productDetailsUrl);

            htmlDoc.LoadHtml(content);

            HtmlNode descriptionNode = null;
            var productDescriptionNode = htmlDoc.GetElementbyId("productDescription");
            if (productDescriptionNode != null)
            {
                descriptionNode = productDescriptionNode.Descendants("div").WithClass("productDescriptionWrapper").LastOrDefault();
            }

            return new ReviewItem(new Uri(this.productDetailsUrl, UriKind.Absolute))
            {
                Name = (htmlDoc.GetElementbyId("btAsinTitle") ?? htmlDoc.GetElementbyId("productTitle")).InnerText.Trim(),
                Description = descriptionNode != null ? new ReviewItem.RichContent { Html = descriptionNode.InnerHtml, Text = descriptionNode.InnerText } : null,
                MainImageUrl = (htmlDoc.GetElementbyId("main-image") ?? htmlDoc.GetElementbyId("prodImage") ?? htmlDoc.GetElementbyId("landingImage")).GetAttributeValue("src", null),
                ImagesUrls = ParseOtherImages(htmlDoc.GetElementbyId("thumbs-image") ?? htmlDoc.GetElementbyId("thumb_strip") ?? htmlDoc.GetElementbyId("altImages")),
                Price = ParsePrice(htmlDoc.GetElementbyId("actualPriceValue") ?? htmlDoc.GetElementbyId("priceblock_ourprice")),
                OldPrice = this.OldPrice,
                Currency = "$",
                Specifications = ParseSpecs(htmlDoc.GetElementbyId("prodDetails")),
                Impressions = TryParseImpressions(htmlDoc)
            };
        }

        private ReviewItem.Impression[] TryParseImpressions(HtmlDocument htmlDoc)
        {
            var ratingArea = htmlDoc.DocumentNode.Descendants("span").WithClass("asinReviewsSummary").FirstOrDefault();

            if (ratingArea == null)
            {
                return new ReviewItem.Impression[0];
            }

            var reviewAnchor = ratingArea.Descendants("a").FirstOrDefault();

            if (reviewAnchor == null)
            {
                return new ReviewItem.Impression[0];
            }

            return ParseImpressions(reviewAnchor.GetAttributeValue("href", null));
        }

        private ReviewItem.Impression[] ParseImpressions(string reviewsUrl)
        {
            if (reviewsUrl == null)
            {
                return new ReviewItem.Impression[0];
            }

            string content = HttpOperations.Get(reviewsUrl).Result;

            if(content == null)
            {
                Log.Warn("Error parsing impressions for Amazon.com product at {0}. No content at {1}.", productDetailsUrl, reviewsUrl);
                return new ReviewItem.Impression[0];
            }

            var htmlDoc = new HtmlDocument();

            Log.Trace("Parsing Amazon.com product impressions from {0}", reviewsUrl);

            htmlDoc.LoadHtml(content);

            var reviewSummaryRows = htmlDoc.GetElementbyId("histogramTable").Descendants("tr").ToArray();

            List<ReviewItem.Impression> impressions = new List<ReviewItem.Impression>();
            for (var rowIndex = 0; rowIndex < reviewSummaryRows.Length; rowIndex++)
            {
                var amazonRating = 5 - rowIndex;
                var rating = (byte)Math.Round((decimal)amazonRating / 5 * 100, 0);
                var count = int.Parse(reviewSummaryRows[rowIndex].Descendants("td").Last().InnerText.Trim().Replace(",", string.Empty));
                for (var i = 0; i < count; i++)
                {
                    impressions.Add(new ReviewItem.Impression { Rating = rating });
                }
            }

            PopulateImpressions(impressions, htmlDoc);

            return impressions.ToArray();
        }

        private decimal ParsePrice(HtmlNode priceNode)
        {
            if (priceNode == null)
            {
                return 0;
            }

            decimal price = 0;
            if (!decimal.TryParse(priceNode.InnerText.Replace("$", string.Empty).Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out price))
            {
                return 0;
            }

            return price;
        }

        private ReviewItem.Specification[] ParseSpecs(HtmlNode detailsNode)
        {
            if (detailsNode == null)
            {
                return new ReviewItem.Specification[0];
            }

            var techDetailsNode = detailsNode
                .Descendants("div")
                .WithClass("techD")
                .FirstOrDefault();

            if (techDetailsNode == null)
            {
                return new ReviewItem.Specification[0];
            }

            return techDetailsNode
                .Descendants("div")
                .WithClass("content")
                .SelectMany(x => x.Descendants("tr"))
                .Select(row =>
                {
                    if (row.Elements("td").WithClass("label").SingleOrDefault() == null)
                    {
                        return null;
                    }

                    return new ReviewItem.Specification
                    {
                        Name = CleanSpecName(row.Elements("td").WithClass("label").Single().InnerText),
                        Value = row.Elements("td").WithClass("value").Single().InnerText.Trim()
                    };
                }
                )
                .Where(x => x != null)
                .ToArray();
        }

        private string[] ParseOtherImages(HtmlNode altImagesNode)
        {
            if (altImagesNode == null)
            {
                return new string[0];
            }

            return altImagesNode
                .Descendants("img")
                .Select(x =>
                        x.GetAttributeValue("src", string.Empty)
                        .Replace("._SX38_SY50_CR,0,0,38,50_.", "._SX600_SY600_CR,0,0,600,600_.")
                ).ToArray();

        }

        private void PopulateImpressions(List<ReviewItem.Impression> impressions, HtmlDocument htmlDoc)
        {
            var reviewListArea = htmlDoc.GetElementbyId("cm_cr-review_list");

            foreach (var reviewNode in reviewListArea.Elements("div"))
            {
                TryPopulateImpressions(reviewNode, impressions);
            }
        }

        private void TryPopulateImpressions(HtmlNode reviewNode, List<ReviewItem.Impression> impressions)
        {
            try
            {
                PopulateImpression(reviewNode, impressions);
            }
            catch(Exception x)
            {
                Log.Error(string.Format("Error populating impressions for Amazon product {0}, review node HTML: {1}", this.productDetailsUrl, reviewNode != null ? reviewNode.InnerHtml : "NULL"), x);
            }
        }

        private void PopulateImpression(HtmlNode reviewNode, List<ReviewItem.Impression> impressions)
        {
            var ratingAndTitleNode = reviewNode.Elements("div").ElementAt(1);
            var amazonRating = decimal.Parse(ratingAndTitleNode.Descendants("span").WithClass("a-icon-alt").Single().InnerText.Trim());
            var rating = (byte)Math.Round(amazonRating / 5 * 100, 0);

            var impression = impressions
                .Where(i => i.On == null || i.By == null || i.Comment == null)
                .Where(i => i.Rating == rating)
                .First();

            var title = ratingAndTitleNode.Elements("a").Last().InnerText.Trim();

            var nameAndDateNode = reviewNode.Elements("div").ElementAt(2);

            var name = nameAndDateNode.Elements("span").First().Elements("a").Last().InnerText.Trim();
            var dateString = nameAndDateNode.Elements("span").WithClass("review-date").Single().InnerText.Replace("on", string.Empty).Trim();
            DateTime? timestamp = ParseReviewTimestamp(dateString);

            var commentHtml = reviewNode.Elements("div").WithClass("review-data").Last().Element("span").InnerHtml;
            commentHtml = string.Format("{0}{1}{1}{1}{2}", title, Environment.NewLine, commentHtml);

            impression.By = name;
            impression.Comment = new ReviewItem.RichContent { Html = commentHtml, Text = HtmlToText(commentHtml) };
            impression.On = timestamp;
        }

        private DateTime? ParseReviewTimestamp(string dateString)
        {
            DateTime timestamp;
            if (DateTime.TryParseExact(dateString, reviewDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out timestamp))
            {
                return timestamp;
            }
            return null;
        }
    }
}
