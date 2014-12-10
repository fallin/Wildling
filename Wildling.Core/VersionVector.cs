using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Wildling.Core.Extensions;

namespace Wildling.Core
{
    /// <summary>
    /// Version vectors are efficient representations of causal histories.
    /// </summary>
    public class VersionVector
    {
        readonly Dictionary<string, long> _events;

        public VersionVector() : this((IDictionary<string, long>)null) {}

        private VersionVector(IDictionary<string, long> events)
        {
            _events = events == null
                ? new Dictionary<string, long>(DefaultComparer)
                : new Dictionary<string, long>(events, DefaultComparer);
        }

        public VersionVector(CausalEvent @event) : this()
        {
            _events.Add(@event.I, @event.N);
        }

        public VersionVector(IEnumerable<CausalEvent> events)
        {
            _events = (events != null)
                ? events.ToDictionary(e => e.I, e => e.N, DefaultComparer)
                : new Dictionary<string, long>(DefaultComparer);
        }

        public IReadOnlyDictionary<string, long> Events
        {
            get { return _events; }
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            int count = -1;
            builder.Append('{');
            _events.OrderBy(pair => pair.Key).Aggregate(builder, (b, pair) =>
            {
                count++;
                if (count > 0) b.Append(',');
                b.AppendFormat("({0},{1})", pair.Key, pair.Value);
                return b;
            });
            builder.Append('}');

            return builder.ToString();
        }

        public long this[string i]
        {
            get { return _events.GetValueOrDefault(i); }
        }

        IEqualityComparer<string> DefaultComparer
        {
            get { return StringComparer.OrdinalIgnoreCase; }
        }

        public HashSet<string> Ids()
        {
            IEnumerable<string> ids = from e in _events select e.Key;
            return new HashSet<string>(ids, DefaultComparer);
        }

        public string ToContextString()
        {
            string contextJson = JsonConvert.SerializeObject(_events, Formatting.None);
            byte[] contextBytes = Encoding.UTF8.GetBytes(contextJson);
            string contextBase64 = Convert.ToBase64String(contextBytes);

            return contextBase64;
            //return contextJson;
        }

        public static VersionVector FromContextString(string contextBase64)
        {
            byte[] bytes = Convert.FromBase64String(contextBase64);
            string json = Encoding.UTF8.GetString(bytes);

            var events = JsonConvert.DeserializeObject<Dictionary<string, long>>(json);
            var vv = new VersionVector(events);
            return vv;

            //var events = JsonConvert.DeserializeObject<Dictionary<string, long>>(contextBase64);
            //var vv = new VersionVector(events);
            //return vv;
        }
    }
}