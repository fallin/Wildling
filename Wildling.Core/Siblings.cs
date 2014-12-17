using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Wildling.Core
{
    /// <summary>
    /// Causally concurrent versions (aka 'siblings').
    /// </summary>
    public class Siblings : HashSet<VersionedObject>
    {
        public Siblings()
        {
        }

        public Siblings(IEnumerable<VersionedObject> collection) : base(collection)
        {
        }

        public void Add(JToken value, DottedVersionVector clock)
        {
            Add(new VersionedObject(value, clock));
        }

        public void Add(JToken value, string version)
        {
            Add(value, DottedVersionVector.Parse(version));
        }

        public void Add(Siblings siblings)
        {
            foreach (VersionedObject sibling in siblings)
            {
                Add(sibling);
            }
        }

        public HashSet<string> Ids()
        {
            var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            this.Select(cv => cv.Clock).Aggregate(ids, (set, dvv) =>
            {
                set.UnionWith(dvv.Ids());
                return set;
            });

            return ids;
        }

        public long MaxDot(string i)
        {
            var counters = new HashSet<long>();
            this.Select(cv => cv.Clock).Aggregate(counters, (set, dvv) =>
            {
                set.UnionWith(new[] {dvv.MaxDot(i)});
                return set;
            });

            return counters.Any() ? counters.Max() : 0;
        }
    }
}