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
    [DebuggerDisplay("Node={_name}")]
    public class Node
    {
        static readonly ILog Log = LogManager.GetCurrentClassLogger();
        readonly string _name;
        readonly PartitionedConsistentHash _ring;
        readonly Dictionary<BigInteger, Siblings> _data = new Dictionary<BigInteger, Siblings>();
        readonly DvvKernel _kernel = new DvvKernel();
        IRemoteNodeClient _remote;
        int _n = 3;

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

        internal DvvKernel Kernel
        {
            get { return _kernel; }
        }

        internal async Task<Siblings> GetAsync(string key)
        {
            Ensure.That(key, "key").IsNotNullOrWhiteSpace();

            BigInteger hash = _ring.Hash(key);
            Siblings siblings;

            if (_ring.PreferenceList(key, _n).Contains(_name))
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

        internal async Task PutAsync(string key, JObject value, VersionVector context = null)
        {
            Ensure.That(key, "key").IsNotNullOrWhiteSpace();

            context = context ?? new VersionVector();

            string coordinatingNode = _ring.Node(key);
            if (_ring.PreferenceList(key, _n).Contains(_name))
            {
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

            Log.DebugFormat("put-local k={0}", key);

            BigInteger hash = _ring.Hash(key);
            Siblings siblings = _data.GetValueOrDefault(hash) ?? new Siblings();
            siblings = _kernel.Sync(siblings, coordinatorSiblings);

            _data[hash] = siblings;
        }

        async Task<List<Siblings>> ReplicateGetAsync(string key)
        {
            IList<string> list = _ring.PreferenceList(key, _n);
            list.Remove(_name);

            var replicaValues = new List<Siblings>();
            string replicateNode;
            while ((replicateNode = list.Shift()) != null)
            {
                Siblings result = await _remote.GetReplicaAsync(replicateNode, key);
                replicaValues.Add(result);
            }

            return replicaValues;
        }

        async Task ReplicatePutAsync(string key, Siblings siblings)
        {
            IList<string> list = _ring.PreferenceList(key, _n);
            list.Remove(_name);

            string replicateNode;
            while ((replicateNode = list.Shift()) != null)
            {
                await _remote.PutReplicaAsync(replicateNode, key, siblings);
            }
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