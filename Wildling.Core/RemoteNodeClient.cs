using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Wildling.Core
{
    class RemoteNodeClient : IRemoteNodeClient
    {
        const string JsonMediaType = "application/json";
        readonly Node _node;

        public RemoteNodeClient(Node node)
        {
            _node = node;
        }

        public async Task RemotePutAsync(string node, string key, JObject value)
        {
            var requestUri = GetRequestUri(node, key);

            using (var client = HttpClientFactory.Create())
            {
                var req = new HttpRequestMessage(HttpMethod.Put, requestUri);
                req.Headers.Add("X-Wildling-N", "0");
                req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(JsonMediaType));

                // TODO: Serialize JSON to stream, not string
                //MemoryStream stream = new MemoryStream();
                //StreamReader streamWriter = new StreamReader(stream);
                //using (JsonWriter jsonWriter = new JsonTextWriter(streamWriter))
                //{
                //    JsonSerializer serializer = new JsonSerializer();
                //    serializer.Serialize(jsonWriter, value);
                //}
                //req.Content = new StreamContent();

                string json = JsonConvert.SerializeObject(value);
                req.Content = new StringContent(json, Encoding.UTF8, JsonMediaType);

                HttpResponseMessage response = await client.SendAsync(req);
                response.EnsureSuccessStatusCode();
            }
        }

        public async Task<JArray> RemoteGetAsync(string node, string key)
        {
            var requestUri = GetRequestUri(node, key);

            JArray value;
            using (var client = HttpClientFactory.Create())
            {
                var req = new HttpRequestMessage(HttpMethod.Get, requestUri);
                req.Headers.Add("X-Wildling-N", "0");
                req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(JsonMediaType));

                HttpResponseMessage response = await client.SendAsync(req);
                response.EnsureSuccessStatusCode();

                Stream stream = await response.Content.ReadAsStreamAsync();
                StreamReader streamReader = new StreamReader(stream);
                using (JsonReader jsonReader = new JsonTextReader(streamReader))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    value = serializer.Deserialize<JArray>(jsonReader);
                }
            }

            return value;
        }

        Uri GetRequestUri(string node, string key)
        {
            string uriSafeKey = Uri.EscapeUriString(key);

            UriBuilder builder = _node.GetUriBuilder(node);
            builder.Path = string.Format("wildling/api/{0}", uriSafeKey);
            Uri requestUri = builder.Uri;
            return requestUri;
        }
    }
}