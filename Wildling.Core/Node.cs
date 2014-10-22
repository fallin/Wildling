using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Threading.Tasks;
using Common.Logging;
using Newtonsoft.Json.Linq;
using Wildling.Core.Extensions;

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
        IRemoteNodeClient _remote;

        public Node(string name, IEnumerable<string> nodes, int partitions = 32)
        {
            _name = name ?? GenerateNodeName();
            string[] combinedNodes = new List<string>(nodes) { _name }.ToArray();
            _ring = new PartitionedConsistentHash(combinedNodes, partitions);

            _remote = new RemoteNodeClient(this);
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

        public async Task PutAsync(string key, JObject value, int n = 1)
        {
            if (n == 0) // store locally
            {
                Log.DebugFormat("put n={0} k={1} v={2}", n, key, value);
                BigInteger hash = _ring.Hash(key);
                _data[hash] = new NodeObject(value);
            }
            else if (_ring.PreferenceList(key, n).Contains(_name))
            {
                Log.DebugFormat("put n={0} k={1} v={2}", n, key, value);
                BigInteger hash = _ring.Hash(key);
                _data[hash] = new NodeObject(value);
                await ReplicatePutAsync(key, value, n);
            }
            else
            {
                string node = _ring.Node(key);
                Log.DebugFormat("forward to node {0}", node);
                await _remote.RemotePutAsync(node, key, value);
            }
        }

        public async Task<JArray> GetAsync(string key, int n = 1)
        {
            JArray results = new JArray();
            if (n == 0) // store locally
            {
                Log.DebugFormat("get n={0} k={1}", n, key);
                BigInteger hash = _ring.Hash(key);
                JObject value = _data[hash].Value;
                results.Add(value);
            }
            else if (_ring.PreferenceList(key, n).Contains(_name))
            {
                Log.DebugFormat("get n={0} k={1}", n, key);
                JArray replicaResponse = await ReplicateGetAsync(key, n);
                results = replicaResponse;

                NodeObject nodeObject = _data[_ring.Hash(key)];
                results.Add(nodeObject.Value);
            }
            else
            {
                string node = _ring.Node(key);
                Log.DebugFormat("forward to node {0}", node);
                JArray remoteResponse = await _remote.RemoteGetAsync(node, key);
                results = remoteResponse;
            }

            return results;
        }

        async Task ReplicatePutAsync(string key, JObject value, int n)
        {
            IList<string> list = _ring.PreferenceList(key, n);
            list.Remove(_name);

            string replicateNode;
            while ((replicateNode = list.Shift()) != null)
            {
                await _remote.RemotePutAsync(replicateNode, key, value);
            }
        }

        async Task<JArray> ReplicateGetAsync(string key, int n)
        {
            IList<string> list = _ring.PreferenceList(key, n);
            list.Remove(_name);

            var results = new JArray();
            string replicateNode;
            while ((replicateNode = list.Shift()) != null)
            {
                JArray result = await _remote.RemoteGetAsync(replicateNode, key);
                foreach (JObject token in result.Children<JObject>())
                {
                    results.Add(token);
                }
            }

            return results;
        }

        public UriBuilder GetUriBuilder(string node)
        {
            Configuration config = Configuration.Default;

            string host = config.Read("host", "localhost", node);
            int port = config.Read("port", 8000, node);

            var builder = new UriBuilder(Uri.UriSchemeHttp, host, port);
            return builder;
        }

        public void UseRemoteNodeClient(IRemoteNodeClient remote)
        {
            _remote = remote;
        }
    }
}