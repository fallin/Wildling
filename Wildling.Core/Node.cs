using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Common.Logging;
using EnsureThat;
using Newtonsoft.Json.Linq;
using Wildling.Core.Extensions;

namespace Wildling.Core
{
    /// <summary>
    /// Manages a hash ring as well as a hash of data
    /// </summary>
    [DebuggerDisplay("Node={" + nameof(_name) + "}")]
    public class Node
    {
        static readonly ILog Log = LogManager.GetLogger<Node>();
        readonly string _name;
        readonly PartitionedConsistentHash _ring;
        readonly Dictionary<BigInteger, Siblings> _data = new Dictionary<BigInteger, Siblings>();
        readonly DvvKernel _kernel = new DvvKernel();
        IRemoteNodeClient _remote;

        public Node(string name, IEnumerable<string> nodes, int partitions = 32)
        {
            N = 3;
            _name = name ?? GenerateNodeName();
            string[] combinedNodes = new List<string>(nodes) { _name }.ToArray();
            _ring = new PartitionedConsistentHash(combinedNodes, partitions);

            _remote = new RemoteNodeClient(this);
        }

        string GenerateNodeName()
        {
            Process process = Process.GetCurrentProcess();
            int processId = process.Id;

            return $"{Environment.MachineName}-{processId:d5}";
        }

        public string Name => _name;

        internal DvvKernel Kernel => _kernel;

        public int N { get; set; }
        public int W { get; set; }
        public int R { get; set; }

        internal async Task<Siblings> GetAsync(string key)
        {
            Ensure.That(key, "key").IsNotNullOrWhiteSpace();

            BigInteger hash = _ring.Hash(key);
            Siblings siblings;

            if (_ring.PreferenceList(key, N).Contains(_name))
            {
                Log.DebugFormat("get k={0}", key);

                List<Siblings> replicaValues = await ReplicateGetAsync(key);
                if (replicaValues != null)
                {
                    replicaValues = replicaValues.Where(x => x != null).ToList();
                }
                else
                {
                    replicaValues = new List<Siblings>();
                }
                
                replicaValues.Add(_data[hash]);

                siblings = _kernel.Merge(replicaValues);
            }
            else
            {
                string node = _ring.Node(key);
                Log.DebugFormat("forward to coordinating node {0}", node);
                siblings = await _remote.GetAsync(node, key);
            }

            return siblings;
        }

        internal async Task PutAsync(string key, JToken value, VersionVector context = null)
        {
            Ensure.That(key, "key").IsNotNullOrWhiteSpace();

            context = context ?? new VersionVector();

            string coordinatingNode = _ring.Node(key);
            if (_ring.PreferenceList(key, N).Contains(_name))
            {
                Log.DebugFormat("put k={0}", key);

                BigInteger hash = _ring.Hash(key);
                Siblings siblings = _data.GetValueOrDefault(hash);
                if (siblings != null)
                {
                    // discard obsolete versions
                    siblings = _kernel.Discard(siblings, context);
                }
                else
                {
                    siblings = new Siblings();
                }
                
                DottedVersionVector dvv = _kernel.Event(context, siblings, _name);
                var versionedObject = new VersionedObject(value, dvv);

                siblings.Add(versionedObject);

                _data[hash] = siblings;

                await ReplicatePutAsync(key, siblings);
            }
            else
            {
                Log.DebugFormat("forward to coordinating node {0}", coordinatingNode);
                await _remote.PutAsync(coordinatingNode, key, value, context);
            }
        }

        internal Siblings GetReplicaAsync(string key)
        {
            Ensure.That(key, "key").IsNotNullOrWhiteSpace();

            Log.DebugFormat("get-replica k={0}", key);

            BigInteger hash = _ring.Hash(key);
            Siblings siblings = _data[hash];

            return siblings;
        }

        internal void PutReplicaAsync(string key, Siblings coordinatorSiblings)
        {
            Ensure.That(key, "key").IsNotNullOrWhiteSpace();

            Log.DebugFormat("put-replica k={0}", key);

            BigInteger hash = _ring.Hash(key);
            Siblings siblings = _data.GetValueOrDefault(hash) ?? new Siblings();
            siblings = _kernel.Sync(siblings, coordinatorSiblings);

            _data[hash] = siblings;
        }

        async Task<List<Siblings>> ReplicateGetAsync(string key)
        {
            IList<string> replicaNodes = _ring.PreferenceList(key, N);
            replicaNodes.Remove(_name);

            var replicaValues = new List<Siblings>();

            List<Task<Siblings>> pendingGets = replicaNodes.Select(r => _remote.GetReplicaAsync(r, key)).ToList();
            await Task.WhenAll(pendingGets);
            
            foreach (var pendingGet in pendingGets)
            {
                try
                {
                    Siblings siblings = await pendingGet;
                    replicaValues.Add(siblings);
                }
                catch (Exception e)
                {
                    Log.Error("Error replicating get -- ignored", e);
                }
            }

            return replicaValues;
        }

        async Task ReplicatePutAsync(string key, Siblings siblings)
        {
            IList<string> replicaNodes = _ring.PreferenceList(key, N);
            replicaNodes.Remove(_name);

            List<Task> pendingPuts = replicaNodes.Select(r => _remote.PutReplicaAsync(r, key, siblings)).ToList();
            await Task.WhenAll(pendingPuts);
        }

        public UriBuilder GetUriBuilder(string node)
        {
            Configuration config = Configuration.Default;

            string host = config.Read("host", "localhost", node);
            int port = config.Read("port", 8000, node);

            var builder = new UriBuilder(Uri.UriSchemeHttp, host, port);
            return builder;
        }

        internal void UseRemoteNodeClient(IRemoteNodeClient remote)
        {
            _remote = remote;
        }
    }
}