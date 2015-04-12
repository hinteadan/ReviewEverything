﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ReviewEverything.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NLog;
using System.Globalization;

namespace ReviewEverything.DataProvider.Storage
{
    public class LocalStoreJsonContractResolver : DefaultContractResolver
    {
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            var properties = base.CreateProperties(type, memberSerialization);

            if (type == typeof(SearchCriteria))
            {
                var property = properties.Single(x => x.PropertyName == "RawValue");
                property.Readable = true;
                property.Writable = true;
            }

            return properties;
        }
    }

    public class LocalStore : ICanStore
    {
        private static Logger log = LogManager.GetCurrentClassLogger();

        private const string stringPairJoint = ":=:";
        private readonly string storeBasePath;
        private const string indexFileName = "index.json";
        private const string documentsFolderName = "Docs";
        private readonly string indexFilePath;
        private readonly string documentsPath;
        private readonly FileSystemWatcher indexWatcher;
        private bool isIndexUpdatedBySelf = false;

        private readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings 
        { 
            ContractResolver = new LocalStoreJsonContractResolver()
        };

        private Dictionary<Guid, StoreIndexEntry> documentsIndexDictionary = new Dictionary<Guid, StoreIndexEntry>();

        public LocalStore()
        {
            storeBasePath = Directory.GetCurrentDirectory() + @"\LocalDataStore";
            string basePathFromAppSettings = ConfigurationManager.AppSettings["LocalStore.BasePath"];
            if (!string.IsNullOrWhiteSpace(basePathFromAppSettings))
            {
                storeBasePath = basePathFromAppSettings;
            }
            indexFilePath = string.Format(@"{0}\{1}", storeBasePath, indexFileName);
            documentsPath = string.Format(@"{0}\{1}", storeBasePath, documentsFolderName);

            EnsureDirectoryPath(storeBasePath);
            EnsureDirectoryPath(documentsPath);
            EnsureFilePath(indexFilePath);

            indexWatcher = new FileSystemWatcher(storeBasePath, indexFileName);
            indexWatcher.Changed += OnIndexFileChanged;
        }

        public void Persist(SearchCriteria criteria, IEnumerable<ReviewItem> items)
        {
            if (!documentsIndexDictionary.Keys.Any())
            {
                UpdateDocumentIndex();
            }
            var storedItems = PersistItems(items);
            PersistSearchCriteria(criteria, storedItems);
        }

        public ReviewItem Retrieve(Uri reference)
        {
            if (!documentsIndexDictionary.Keys.Any())
            {
                UpdateDocumentIndex();
            }

            Guid? docId = FindReviewItemIdByReference(reference);

            StoreDocument<ReviewItem> doc = JsonConvert.DeserializeObject<StoreDocument<ReviewItem>>(
                File.ReadAllText(string.Format(@"{0}\{1}", documentsPath, docId))
                );

            return doc.Payload;
        }

        private Guid? FindReviewItemIdByReference(Uri reference)
        {
            string type = typeof(SearchCriteria).AssemblyQualifiedName;

            var entry = documentsIndexDictionary.Values
                .Where(x => x.Type == type)
                .FirstOrDefault(x => x.Fields["ReviewItems"].Contains(reference.ToString()));

            if (entry == null)
            {
                return null;
            }

            return ParseIndexedReviewItems(entry.Fields["ReviewItems"])
                .First(i => i.Value == reference)
                .Key;
        }

        public IEnumerable<ICanBeParsed> SearchFor(SearchCriteria criteria)
        {
            if (!documentsIndexDictionary.Keys.Any())
            {
                UpdateDocumentIndex();
            }

            string type = typeof(SearchCriteria).AssemblyQualifiedName;

            return documentsIndexDictionary.Values
                .Where(x => x.Type == type)
                .Where(x => x.Fields["Value"] == criteria.RawValue)
                .SelectMany(x => ParseIndexedReviewItems(x.Fields["ReviewItems"]))
                .Select(i => new ReviewItemLoader(documentsPath, i.Key));
        }

        private void EnsureFilePath(string path)
        {
            if (File.Exists(path))
            {
                EnsureAccessToFilePath(path);
                return;
            }

            File.WriteAllText(path, string.Empty);
        }

        private void EnsureAccessToFilePath(string path)
        {
            File.GetAccessControl(path);
        }

        private void EnsureDirectoryPath(string path)
        {
            if (Directory.Exists(path))
            {
                EnsureAccessToDirectoryPath(path);
                return;
            }

            Directory.CreateDirectory(path);
        }

        private void EnsureAccessToDirectoryPath(string path)
        {
            Directory.GetAccessControl(path);
        }

        private void OnIndexFileChanged(object sender, FileSystemEventArgs e)
        {
            UpdateDocumentIndex();
        }

        private void UpdateDocumentIndex()
        {
            if (isIndexUpdatedBySelf)
            {
                isIndexUpdatedBySelf = false;
                return;
            }
            documentsIndexDictionary = JsonConvert.DeserializeObject<Dictionary<Guid, StoreIndexEntry>>(File.ReadAllText(indexFilePath))
                ?? new Dictionary<Guid, StoreIndexEntry>();
        }

        private IEnumerable<StoreDocument<ReviewItem>> PersistItems(IEnumerable<ReviewItem> items)
        {
            log.Trace("Persisting {0} review items to local disk", items.Count());

            string type = typeof(ReviewItem).AssemblyQualifiedName;

            foreach (var item in items)
            {
                var doc = new StoreDocument<ReviewItem>
                {
                    Id = Guid.NewGuid(),
                    Payload = item
                };
                File.WriteAllText(string.Format(@"{0}\{1}", documentsPath, doc.Id), JsonConvert.SerializeObject(doc, Formatting.Indented));

                documentsIndexDictionary.Add(doc.Id, new StoreIndexEntry
                {
                    Id = doc.Id,
                    Type = type,
                    Fields = IndexFieldsFor(item)
                });

                yield return doc;
            }

            isIndexUpdatedBySelf = true;
            File.WriteAllText(indexFilePath, JsonConvert.SerializeObject(documentsIndexDictionary, Formatting.Indented));
        }

        private StoreIndexEntry PersistSearchCriteria(SearchCriteria criteria, IEnumerable<StoreDocument<ReviewItem>> storedReviewItems)
        {
            log.Trace("Persisting search criteria '{0}' to local disk", criteria.RawValue);

            StoreDocument<SearchCriteria> doc = new StoreDocument<SearchCriteria>
            {
                Id = Guid.NewGuid(),
                Payload = criteria
            };
            File.WriteAllText(string.Format(@"{0}\{1}", documentsPath, doc.Id), JsonConvert.SerializeObject(doc, Formatting.Indented));
            StoreIndexEntry indexEntry = new StoreIndexEntry
            {
                Id = doc.Id,
                Type = typeof(SearchCriteria).AssemblyQualifiedName,
                Fields = IndexFieldsFor(criteria, storedReviewItems)
            };
            documentsIndexDictionary.Add(indexEntry.Id, indexEntry);
            isIndexUpdatedBySelf = true;
            File.WriteAllText(indexFilePath, JsonConvert.SerializeObject(documentsIndexDictionary, Formatting.Indented));
            return indexEntry;
        }

        private Dictionary<string, string> IndexFieldsFor(SearchCriteria criteria, IEnumerable<StoreDocument<ReviewItem>> storedReviewItems)
        {
            return new Dictionary<string, string> 
            { 
                { "Value", criteria.RawValue },
                { "ReviewItems", string.Join(" ", storedReviewItems.Select(i => 
                        string.Format("{0}{1}{2}", i.Id, stringPairJoint, i.Payload.Reference)
                    ).ToArray()) 
                }
            };
        }

        private Dictionary<string, string> IndexFieldsFor(ReviewItem item)
        {
            return new Dictionary<string, string> 
            { 
                { "Reference", item.Reference.ToString() },
                { "Name", item.Name },
                { "CreatedOn", item.CreatedOn.ToString(CultureInfo.InvariantCulture) },
                { "Currency", item.Currency },
                { "Price", item.Price.ToString(CultureInfo.InvariantCulture) },
                { "Rating", item.Rating().ToString() },
                { "SpecificationsCount", item.Specifications.Length.ToString(CultureInfo.InvariantCulture) },
                { "ImpressionsCount", item.Impressions.Length.ToString(CultureInfo.InvariantCulture) }
            };
        }

        private KeyValuePair<Guid, Uri>[] ParseIndexedReviewItems(string reviewItems)
        {
            return reviewItems
                .Split(' ')
                .Select(p =>
                {
                    var pair = p.Split(new string[] { stringPairJoint }, StringSplitOptions.None);
                    return new KeyValuePair<Guid, Uri>(new Guid(pair[0]), new Uri(pair[1]));
                })
                .ToArray();
        }

        public Task PersistAsync(SearchCriteria criteria, IEnumerable<ReviewItem> items)
        {
            return Task.Run(() => this.Persist(criteria, items));
        }

        public Task<ReviewItem> RetrieveAsync(Uri reference)
        {
            return Task.Run<ReviewItem>(() => this.Retrieve(reference));
        }

        public Task<IEnumerable<ICanBeParsed>> SearchForAsync(SearchCriteria criteria)
        {
            return Task.Run<IEnumerable<ICanBeParsed>>(() => this.SearchFor(criteria));
        }

        public IEnumerable<T> Find<T>(Predicate<Dictionary<string, string>> indexPredicate)
        {
            if (!documentsIndexDictionary.Keys.Any())
            {
                UpdateDocumentIndex();
            }

            var type = typeof(T).AssemblyQualifiedName;

            return documentsIndexDictionary
                .Values
                .Where(v => v.Type == type)
                .Where(v => indexPredicate(v.Fields))
                .Select(v => LoadDocument<T>(v.Id).Payload);
        }

        private StoreDocument<T> LoadDocument<T>(Guid id)
        {
            return JsonConvert.DeserializeObject<StoreDocument<T>>(File.ReadAllText(string.Format(@"{0}\{1}", documentsPath, id)), jsonSettings);
        }
    }
}
