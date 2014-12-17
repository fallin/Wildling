using System;
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

        public async Task PutAsync(string node, string key, JToken value, VersionVector context)
        {
            var requestUri = GetRequestUri(node, key);

            using (var client = HttpClientFactory.Create())
            {
                var req = new HttpRequestMessage(HttpMethod.Put, requestUri);
                req.Headers.Add("X-Context", context.ToString());
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

        public async Task<Siblings> GetAsync(string node, string key)
        {
            var requestUri = GetRequestUri(node, key);

            Siblings siblings;
            using (var client = HttpClientFactory.Create())
            {
                var req = new HttpRequestMessage(HttpMethod.Get, requestUri);
                req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(JsonMediaType));

                HttpResponseMessage response = await client.SendAsync(req);
                response.EnsureSuccessStatusCode();

                Stream stream = await response.Content.ReadAsStreamAsync();
                StreamReader streamReader = new StreamReader(stream);
                using (JsonReader jsonReader = new JsonTextReader(streamReader))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    siblings = serializer.Deserialize<Siblings>(jsonReader);
                }
            }

            return siblings;
        }

        Uri GetRequestUri(string node, string key)
        {
            string uriSafeKey = Uri.EscapeUriString(key);

            UriBuilder builder = _node.GetUriBuilder(node);
            builder.Path = string.Format("wildling/api/{0}", uriSafeKey);
            Uri requestUri = builder.Uri;
            return requestUri;
        }

        public async Task PutReplicaAsync(string node, string key, Siblings siblings)
        {
            var requestUri = GetReplicaRequestUri(node, key);

            using (var client = HttpClientFactory.Create())
            {
                var req = new HttpRequestMessage(HttpMethod.Put, requestUri);
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

                string json = JsonConvert.SerializeObject(siblings);
                req.Content = new StringContent(json, Encoding.UTF8, JsonMediaType);

                HttpResponseMessage response = await client.SendAsync(req);
                response.EnsureSuccessStatusCode();
            }
        }

        public async Task<Siblings> GetReplicaAsync(string node, string key)
        {
            var requestUri = GetReplicaRequestUri(node, key);

            Siblings siblings;
            using (var client = HttpClientFactory.Create())
            {
                var req = new HttpRequestMessage(HttpMethod.Get, requestUri);
                req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(JsonMediaType));

                HttpResponseMessage response = await client.SendAsync(req);
                response.EnsureSuccessStatusCode();

                Stream stream = await response.Content.ReadAsStreamAsync();
                StreamReader streamReader = new StreamReader(stream);
                using (JsonReader jsonReader = new JsonTextReader(streamReader))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    siblings = serializer.Deserialize<Siblings>(jsonReader);
                }
            }

            return siblings;
        }

        Uri GetReplicaRequestUri(string node, string key)
        {
            string uriSafeKey = Uri.EscapeUriString(key);

            UriBuilder builder = _node.GetUriBuilder(node);
            builder.Path = string.Format("wildling/api/replica/{0}", uriSafeKey);
            Uri requestUri = builder.Uri;
            return requestUri;
        }
    }
}