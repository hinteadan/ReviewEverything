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

        public static bool HasAttribute(this HtmlNode node, string attrName, string attrValue = null)
        {
            return node.HasAttributes && node.Attributes[attrName] != null
                && (string.IsNullOrWhiteSpace(attrValue) ? true : string.Equals(node.Attributes[attrName].Value, attrValue, StringComparison.InvariantCultureIgnoreCase));
        }

        public static IEnumerable<HtmlNode> WithAttribute(this IEnumerable<HtmlNode> nodes, string attrName, string attrValue = null)
        {
            return nodes.Where(n => n.HasAttribute(attrName, attrValue));
        }

        private static bool HasClass(string classAttributValue, string classToCheck)
        {
            return classAttributValue
                .Split(' ')
                .Any(c => string.Equals(c.Trim(), classToCheck, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
