using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReviewEverything.Model
{
    public static class ReviewItemExtensions
    {
        public static double? Rating(this IEnumerable<ReviewItem> items)
        {
            var countableRatings = items.Where(i => i.Rating() != null);

            if(!countableRatings.Any())
            {
                return null;
            }

            return Math.Round(countableRatings.Sum(i => i.Rating().Value) / countableRatings.Count(), 2);
        }
    }
}
