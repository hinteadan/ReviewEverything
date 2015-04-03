using System;
using System.Collections.Generic;
using System.Linq;
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

            Log.Trace("Parsing eMag.ro product details from {0}", productDetailsUrl);

            var name = htmlDoc.GetElementbyId("offer-title").Element("h1").InnerText;
            var detailsNode = htmlDoc.GetElementbyId("description_section");
            var descriptionNode = detailsNode.Descendants("div").WithClass("preview_desc").SingleOrDefault();

            var mainImageNode = htmlDoc.GetElementbyId("zoom-image").Element("img");
            string mainImagePath = string.Format("http:{0}", mainImageNode.GetAttributeValue("src", null));

            var otherImgPaths = new string[0];
            var pictureCarouselNode = htmlDoc.GetElementbyId("product-pictures-carousel");
            if (pictureCarouselNode != null)
            {
                var otherImagesNodes = htmlDoc.GetElementbyId("product-pictures-carousel").Descendants("a").WithClass("gallery-image");
                otherImgPaths = otherImagesNodes
                    .Select(n => string.Format("http:{0}", n.GetAttributeValue("href", null)))
                    .Where(p => !string.Equals(p, mainImagePath, StringComparison.InvariantCultureIgnoreCase))
                    .ToArray();
            }

            var specsNode = htmlDoc.GetElementbyId("box-specificatii-produs");
            var reviewsNode = htmlDoc.GetElementbyId("new_reviews");

            Log.Trace("Successfully parsed eMag.ro product details from {0}", productDetailsUrl);

            return new ReviewItem(new Uri(this.productDetailsUrl, UriKind.Absolute))
            {
                Name = name,
                Description = descriptionNode != null ? new ReviewItem.RichContent { Html = descriptionNode.InnerHtml, Text = descriptionNode.InnerText } : null,
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
            var reviewGridNode = reviewsNode.Descendants("div").WithAttribute("id", "grila_review").SingleOrDefault();

            if (reviewGridNode == null)
            {
                return new ReviewItem.Impression[0];
            }

            ReviewItem.Impression[] impressions = ParseRatingCountsFromReviewsHeader(
                reviewGridNode.Descendants("div").WithClass("rating-count").ToArray()
                );

            var reviewDetailsContainerNode = reviewsNode.Elements("div").WithAttribute("id", "rcontainer").SingleOrDefault();
            if (reviewDetailsContainerNode != null)
            {
                PopulateImpressionsWithDetails(impressions, reviewDetailsContainerNode.Elements("div").WithClass("review_row"));
            }

            return impressions;
        }

        private ReviewItem.Impression[] ParseRatingCountsFromReviewsHeader(HtmlNode[] ratingCountNodes)
        {
            List<ReviewItem.Impression> impressions = new List<ReviewItem.Impression>();

            for (int i = 0; i < ratingCountNodes.Length; i++)
            {
                decimal emagRatingValue = ratingCountNodes.Length - i;
                byte rating = (byte)Math.Round(emagRatingValue / 5 * 100, 0);
                int numberOfImpressions = int.Parse(ratingCountNodes[i].InnerText.Trim());
                impressions.AddRange(GenerateEmptyImpressions(numberOfImpressions, rating));
            }

            return impressions.ToArray();
        }

        private IEnumerable<ReviewItem.Impression> GenerateEmptyImpressions(int numberOfImpressions, byte rating)
        {
            for (var i = 0; i < numberOfImpressions; i++)
            {
                yield return new ReviewItem.Impression { Rating = rating };
            }
        }

        private void PopulateImpressionsWithDetails(ReviewItem.Impression[] impressions, IEnumerable<HtmlNode> reviewDetailNodes)
        {
            foreach (var reviewDetailNode in reviewDetailNodes)
            {
                var ratingNode = reviewDetailNode.Descendants("div").WithClass("star-rating-small-progress").Single();
                string ratingString = ratingNode.GetAttributeValue("style", null);
                if (ratingString == null)
                {
                    continue;
                }
                var rating = byte.Parse(ratingString.ToLowerInvariant().Replace("width:", string.Empty).Replace("%", string.Empty).Trim());
                var impressionToPopulate = impressions.FirstOrDefault(i => i.By == null && i.Rating == rating);
                if(impressionToPopulate == null)
                {
                    continue;
                }

                PopulateImpressionWithDetails(impressionToPopulate, reviewDetailNode);
            }
        }

        private void PopulateImpressionWithDetails(ReviewItem.Impression impressionToPopulate, HtmlNode reviewDetailNode)
        {
            impressionToPopulate.By = reviewDetailNode.Descendants("div").WithClass("review_user_caption").Single().Elements("a").Single().InnerText.Trim();
            var titleNode = reviewDetailNode.Descendants("div").WithClass("review_titlu").Single().Elements("a").Single();
            var commentNode = reviewDetailNode.Descendants("div").WithClass("review_body_full").SingleOrDefault() ?? 
                reviewDetailNode.Descendants("div").WithClass("review_body_truncated").Single();

            string commentHtml = string.Format("<strong>{0}</strong><br/><br/>{1}", titleNode.InnerHtml, commentNode.InnerHtml);

            impressionToPopulate.Comment = new ReviewItem.RichContent
            {
                Html = commentHtml,
                Text = HtmlToText(commentHtml)
            };
        }
    }
}
