using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReviewEverything.DataProvider.Storage
{
    public class StoreIndexEntry
    {
        public Guid Id { get; set; }
        public string Type { get; set; }
        public Dictionary<string, string> Fields { get; set; }
    }
}
