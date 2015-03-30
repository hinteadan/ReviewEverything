using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace ReviewEverything.DataProvider
{
    internal static class HtmlQueryExtensions
    {
        public static IEnumerable<HtmlNode> WithClass(this IEnumerable<HtmlNode> nodes, string className)
        {
            return nodes.Where(n => n.HasAttributes
                && n.Attributes["class"] != null
                && n.Attributes["class"].Value != null
                && HasClass(n.Attributes["class"].Value, className)
                );
        }

        private static bool HasClass(string classAttributValue, string classToCheck)
        {
            return classAttributValue
                .Split(' ')
                .Any(c => string.Equals(c.Trim(), classToCheck, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
