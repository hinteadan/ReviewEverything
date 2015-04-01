using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Recognos.Core;
using ReviewEverything.Model;

namespace ReviewEverything.DataProvider.eMag
{
    public class EMagProductDetails : WebsiteProductDetails
    {
        public EMagProductDetails(string url) : base(url) { }

        protected override ReviewItem Parse(string content)
        {
            throw new NotImplementedException();
        }
    }
}
