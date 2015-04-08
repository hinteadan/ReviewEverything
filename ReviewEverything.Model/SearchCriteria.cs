using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReviewEverything.Model
{
    public class SearchCriteria
    {
        private string rawSearchString;

        public SearchCriteria(string searchString)
        {
            this.rawSearchString = searchString;
            this.CreatedOn = DateTime.Now;
        }

        public DateTime CreatedOn { get; set; }

        public string RawValue
        {
            get
            {
                return this.rawSearchString;
            }
            private set
            {
                this.rawSearchString = value;
            }
        }

        public string UriFriendly()
        {
            return Uri.EscapeUriString(this.rawSearchString);
        }

        public string FileNameFriendly()
        {
            string fileName = this.RawValue;
            foreach(char invalidChar in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(invalidChar.ToString(), string.Empty);
            }
            if(fileName.Length > 256)
            {
                fileName = fileName.Substring(0, 256);
            }
            return fileName;
        }
    }
}
