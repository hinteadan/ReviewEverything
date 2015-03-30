using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Recognos.Core;
using ReviewEverything.Model;

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
            return new ReviewItem 
            { 
                Name = productDetailsUrl
            };
        }
    }
}
