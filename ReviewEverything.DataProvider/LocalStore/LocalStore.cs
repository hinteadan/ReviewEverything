using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReviewEverything.Model;
using Newtonsoft.Json;

namespace ReviewEverything.DataProvider.LocalStore
{
    public class LocalStore : ICanStore
    {
        private readonly string storeBasePath;

        public LocalStore()
        {
            storeBasePath = Directory.GetCurrentDirectory() + @"\LocalDataStore";
            string basePathFromAppSettings = ConfigurationManager.AppSettings["LocalStore.BasePath"];
            if (!string.IsNullOrWhiteSpace(basePathFromAppSettings))
            {
                storeBasePath = basePathFromAppSettings;
            }

            EnsureBasePath();
        }

        public void Persist(SearchCriteria criteria, IEnumerable<ReviewItem> items)
        {
            throw new NotImplementedException();
        }

        public ReviewItem Retrieve(Uri reference)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ICanBeParsed> SearchFor(SearchCriteria criteria)
        {
            throw new NotImplementedException();
        }

        private void EnsureBasePath()
        {
            if (Directory.Exists(storeBasePath))
            {
                EnsureAccessToBasePath();
                return;
            }

            Directory.CreateDirectory(storeBasePath);
        }

        private void EnsureAccessToBasePath()
        {
            Directory.GetAccessControl(storeBasePath);
        }
    }
}
