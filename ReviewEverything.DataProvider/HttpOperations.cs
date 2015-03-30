using System;
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
        public static string Get(string url)
        {
            HttpWebRequest request = HttpWebRequest.CreateHttp(url);
            var response = request.GetResponse() as HttpWebResponse;

            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
