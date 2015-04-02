using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReviewEverything.DataProvider.Storage
{
    public class StoreDocument<TPayload>
    {
        public Guid Id { get; set; }
        public TPayload Payload { get; set; }
    }
}