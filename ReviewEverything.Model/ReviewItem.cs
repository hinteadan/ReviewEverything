using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReviewEverything.Model
{
    public class ReviewItem
    {
        public class Specification
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }

        public class Impression
        {
            public string By { get; set; }
            public DateTime? On { get; set; }
            public string Comment { get; set; }
            public byte? Rating { get; set; }
        }

        private readonly Uri reference;

        public ReviewItem(Uri reference)
        {
            this.reference = reference;
        }

        public Uri Reference
        {
            get
            {
                return this.reference;
            }
        }

        public string Name { get; set; }

        public string Description { get; set; }
        public Specification[] Specifications { get; set; }

        public string MainImageUrl { get; set; }
        public string[] ImagesUrls { get; set; }
        public decimal Price { get; set; }
        public decimal? OldPrice { get; set; }
        public string Currency { get; set; }
        public Impression[] Impressions { get; set; }

    }
}
