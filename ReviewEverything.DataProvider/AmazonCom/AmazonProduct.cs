using ReviewEverything.Model;
using System;
using System.Threading.Tasks;

namespace ReviewEverything.DataProvider.AmazonCom
{
    public class AmazonProduct : ICanBeParsed
    {
        public ReviewItem Parse()
        {
            throw new NotImplementedException();
        }

        public Task<ReviewItem> ParseAsync()
        {
            throw new NotImplementedException();
        }
    }
}
