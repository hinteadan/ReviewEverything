using Recognos.Core;
using ReviewEverything.Model;
using System;
using System.Threading.Tasks;
using HtmlAgilityPack;
using NLog;

namespace ReviewEverything.DataProvider
{
    public abstract class WebsiteProductDetails : ICanBeParsed
    {
        protected readonly string productDetailsUrl;

        protected Logger Log { get; private set; }

        public WebsiteProductDetails(string url)
        {
            Check.Condition(Uri.IsWellFormedUriString(url, UriKind.Absolute), "The given product details URL is invalid!");

            this.productDetailsUrl = url;
            this.Log = LogManager.GetLogger(this.GetType().FullName);
        }

        public string Name { get; set; }
        public string ThumbnailUrl { get; set; }
        public decimal Price { get; set; }
        public decimal? OldPrice { get; set; }
        public string Currency { get; set; }
        public byte? Rating { get; set; }

        public ReviewItem Parse()
        {
            return Parse(HttpOperations.Get(this.productDetailsUrl).Result);
        }

        protected abstract ReviewItem Parse(string content);

        protected string CleanSpecName(string name)
        {
            var trimmed = name.Trim();
            if (trimmed[trimmed.Length - 1] == ':')
            {
                return trimmed.Substring(0, trimmed.Length - 1);
            }
            return trimmed;
        }

        protected string HtmlToText(string html)
        {
            var result = html
                .Replace("<br/>", Environment.NewLine)
                .Replace("<br>", Environment.NewLine)
                .Replace("<br />", Environment.NewLine);

            var doc = new HtmlDocument();
            doc.LoadHtml(result);

            return doc.DocumentNode.InnerText;
        }

        public Task<ReviewItem> ParseAsync()
        {
            return Task.Run<ReviewItem>(() => this.Parse());
        }
    }
}
