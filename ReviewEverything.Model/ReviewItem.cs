using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReviewEverything.Model
{
    public class ReviewItem
    {
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
        public Dictionary<string, string> Properties { get; set; }

        public string MainImageUrl { get; set; }
        public string[] ImagesUrls { get; set; }
        public decimal Price { get; set; }
        public decimal? OldPrice { get; set; }
        public string Currency { get; set; }

    }
}
