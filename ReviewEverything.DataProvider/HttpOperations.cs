using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using NLog;

namespace ReviewEverything.DataProvider
{
    internal static class HttpOperations
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        private static ConcurrentDictionary<string, RequestInfo> requests = new ConcurrentDictionary<string, RequestInfo>();

        public static Task<string> Get(string url)
        {
            log.Trace("Trying HTTP GET {0}", url);
            return Task.Run<string>(() =>
            {
                var domain = new Uri(url, UriKind.Absolute).Host;

                if (!requests.ContainsKey(domain))
                {
                    requests.TryAdd(domain, RequestInfo.CreateFor(domain));
                }

                while (requests[domain].IsHot())
                {
                    log.Info("Domain \"{0}\" is hot. Waiting {1} seconds for cooldown.", domain, RequestInfo.Cooldown.TotalSeconds);
                    Task.WaitAll(Task.Run(() =>Task.Delay(RequestInfo.Cooldown)));

                    requests[domain].Reset();
                }

                log.Trace("Requesting HTTP GET {0}", url);
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
        private static TimeSpan cooldown = TimeSpan.FromSeconds(30);
        private static byte hotWire = 3;

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
