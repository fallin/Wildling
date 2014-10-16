using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;
using Newtonsoft.Json;

namespace Wildling.Core
{
    /// <summary>
    /// Manages a hash ring as well as a hash of data
    /// </summary>
    public class Node
    {
        static readonly ILog Log = LogManager.GetCurrentClassLogger();
        readonly string _name;
        readonly PartitionedConsistentHash _ring;
        readonly Dictionary<BigInteger, NodeObject> _data = new Dictionary<BigInteger, NodeObject>();

        public Node(string name, IEnumerable<string> nodes, int partitions = 32)
        {
            _name = name ?? GenerateNodeName();
            string[] combinedNodes = new List<string>(nodes) { _name }.ToArray();
            _ring = new PartitionedConsistentHash(combinedNodes, partitions);
        }

        string GenerateNodeName()
        {
            Process process = Process.GetCurrentProcess();
            int processId = process.Id;

            return string.Format("{0}-{1:d5}", Environment.MachineName, processId);
        }

        public string Name
        {
            get { return _name; }
        }

        public async Task PutAsync(string key, object value)
        {
            string node = _ring.Node(key);
            if (node == _name)
            {
                Log.DebugFormat("put {0} {1}", key, value);
                BigInteger hash = _ring.Hash(key);
                _data[hash] = new NodeObject(value);
            }
            else
            {
                Log.DebugFormat("forwarded to node {0}", node);
                await RemotePutAsync(node, key, value);
            }
        }

        public async Task<object> GetAsync(string key)
        {
            object value;

            string node = _ring.Node(key);
            if (node == _name)
            {
                BigInteger hash = _ring.Hash(key);
                value = _data[hash].Value;
                Log.DebugFormat("get {0} {1}", key, value);
            }
            else
            {
                Log.DebugFormat("forwarded to node {0}", node);
                value = await RemoteGetAsync(node, key);
            }

            return value;
        }

        protected virtual async Task RemotePutAsync(string node, string key, object value)
        {
            const string mediaType = "application/json";
            var requestUri = GetRequestUri(node, key);

            using (var client = HttpClientFactory.Create())
            {
                string json = JsonConvert.SerializeObject(value);
                HttpContent content = new StringContent(json, Encoding.UTF8, mediaType);

                await client.PutAsync(requestUri, content);
            }
        }

        protected virtual async Task<object> RemoteGetAsync(string node, string key)
        {
            var requestUri = GetRequestUri(node, key);

            object value;
            using (var client = HttpClientFactory.Create())
            {
                // TODO: find a better solution
                HttpResponseMessage response = await client.GetAsync(requestUri);
                value = response;
            }

            return value;
        }

        Uri GetRequestUri(string node, string key)
        {
            string uriSafeKey = Uri.EscapeUriString(key);

            UriBuilder builder = GetUriBuilder(node);
            builder.Path = string.Format("wildling/api/{0}", uriSafeKey);
            Uri requestUri = builder.Uri;
            return requestUri;
        }

        public UriBuilder GetUriBuilder(string node)
        {
            Configuration config = Configuration.Default;

            string host = config.Read("host", "localhost", node);
            int port = config.Read("port", 8000, node);

            var builder = new UriBuilder(Uri.UriSchemeHttp, host, port);
            return builder;
        }
    }
}