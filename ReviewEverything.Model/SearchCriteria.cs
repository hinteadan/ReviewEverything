using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReviewEverything.Model
{
    public class SearchCriteria
    {
        private readonly string rawSearchString;

        private SearchCriteria() { }
        public SearchCriteria(string searchString)
        {
            this.rawSearchString = searchString;
        }

        public string RawValue
        {
            get
            {
                return this.rawSearchString;
            }
        }

        public string UriFriendly()
        {
            return Uri.EscapeUriString(this.rawSearchString);
        }
    }
}
