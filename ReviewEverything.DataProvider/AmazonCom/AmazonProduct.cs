using ReviewEverything.Model;
using System;
using System.Threading.Tasks;

namespace ReviewEverything.DataProvider.AmazonCom
{
    public class AmazonProduct : WebsiteProductDetails
    {
        public AmazonProduct(string url) : base(url) { }

        protected override ReviewItem Parse(string content)
        {
            throw new NotImplementedException();
        }
    }
}
