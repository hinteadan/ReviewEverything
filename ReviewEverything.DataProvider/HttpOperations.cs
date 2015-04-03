using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ReviewEverything.DataProvider
{
    internal static class HttpOperations
    {
        private static ConcurrentDictionary<string, RequestInfo> requests = new ConcurrentDictionary<string, RequestInfo>();

        public static Task<string> Get(string url)
        {
            return Task.Run<string>(() =>
            {
                var domain = new Uri(url, UriKind.Absolute).Host;

                if (!requests.ContainsKey(domain))
                {
                    requests.TryAdd(domain, RequestInfo.CreateFor(domain));
                }

                if (requests[domain].IsHot())
                {
                    Task.WaitAll(Task.Run(() =>
                        Task.Delay(RequestInfo.Cooldown)
                        .ContinueWith(t =>
                        {
                            requests[domain].Reset();
                        })
                        ));
                }

                HttpWebRequest request = HttpWebRequest.CreateHttp(url);
                var response = request.GetResponse() as HttpWebResponse;

                requests[domain].Track();

                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    return reader.ReadToEnd();
                }
            });
        }
    }
    internal class RequestInfo
    {
        private static TimeSpan cooldown = TimeSpan.FromSeconds(15);
        private static byte hotWire = 5;

        private RequestInfo() { }

        public string Domain { get; set; }
        public DateTime LastRequest { get; set; }
        public byte Count { get; set; }
        public static TimeSpan Cooldown
        {
            get
            {
                return cooldown;
            }
        }

        public void Reset()
        {
            this.Count = 0;
        }

        public void Track()
        {
            this.Count++;
            this.LastRequest = DateTime.Now;
        }

        public bool IsHot()
        {
            return this.Count >= hotWire && DateTime.Now - this.LastRequest <= cooldown;
        }

        public static RequestInfo CreateFor(string domain)
        {
            return new RequestInfo
            {
                Domain = domain,
                LastRequest = DateTime.MinValue,
                Count = 0
            };
        }
    }

}
