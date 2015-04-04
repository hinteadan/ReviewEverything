using HtmlAgilityPack;
using ReviewEverything.Model;
using System;
using System.Collections.Generic;

namespace ReviewEverything.DataProvider.AmazonCom
{
    public class AmazonSearch : WebsiteSearch
    {
        protected override string UrlPattern
        {
            get { return "http://www.amazon.com/s/ref=nb_sb_noss_1?url=search-alias%3Daps&field-keywords={0}"; }
        }

        protected override IEnumerable<ICanBeParsed> ParseSearchResult(string content)
        {
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(content);

            Log.Trace("Parsing Amazon.com search results");

            throw new NotImplementedException();
        }
    }
}
