using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReviewEverything.Model;
using Newtonsoft.Json;
using System.IO;

namespace ReviewEverything.DataProvider.Storage
{
    internal class ReviewItemLoader : ICanBeParsed
    {
        private readonly string documentsPath;
        private readonly Guid documentId;

        public ReviewItemLoader(string documentsPath, Guid documentId)
        {
            this.documentsPath = documentsPath;
            this.documentId = documentId;
        }

        public ReviewItem Parse()
        {
            string docFilePath = string.Format(@"{0}\{1}", documentsPath, documentId);

            if(!File.Exists(docFilePath))
            {
                throw new FileNotFoundException(string.Format("Document {0} cannot be found", documentId));
            }

            return JsonConvert.DeserializeObject<StoreDocument<ReviewItem>>(File.ReadAllText(docFilePath)).Payload;
        }
    }
}
