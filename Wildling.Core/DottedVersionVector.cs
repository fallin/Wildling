using System;
using System.Collections.Generic;
using System.Linq;
using EnsureThat;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Piglet.Parser;
using Wildling.Core.Converters;

namespace Wildling.Core
{
    /// <summary>
    /// This data structure encapsulates the tracking, maintaining and reasoning about a value's causality.
    /// </summary>
    /// <remarks>See https://github.com/ricardobcl/Dotted-Version-Vectors
    /// </remarks>
    [JsonConverter(typeof(DottedVersionVectorJsonConverter))]
    public sealed class DottedVersionVector : IEquatable<DottedVersionVector>, IJsonSerializable
    {
        /// <summary>
        /// The dot uniquely represents a write and it's associated version.
        /// </summary>
        readonly CausalEvent _dot;
        readonly VersionVector _v;
        readonly static Lazy<IParser<object>> Parser = new Lazy<IParser<object>>(CreateParser);

        public DottedVersionVector(CausalEvent dot, VersionVector v)
        {
            Ensure.That(dot, "dot").IsNotNull();
            Ensure.That(v, "v").IsNotNull();

            _dot = dot;
            _v = v;
        }

        // CausallyDescendant --> replace older values
        // CausallyConcurrent --> all kept (for further reconsiliation)

        // CausallyDescendantOf
        public bool HappensBefore(DottedVersionVector other)
        {
            Ensure.That(other, "other").IsNotNull();

            // --this--     --dvv--
            // ((i,n),u) < ((j,m),v) <=> n <= v[i]
            return _dot.N <= other._v[_dot.I];
        }

        public bool HappensBefore(VersionVector vv)
        {
            Ensure.That(vv, "vv").IsNotNull();

            return _dot.N <= vv[_dot.I];
        }

        public HashSet<string> Ids()
        {
            IEnumerable<string> ids = new[] { _dot.I }.Union(_v.Ids());
            return new HashSet<string>(ids, StringComparer.OrdinalIgnoreCase);
        }

        public long MaxDot(string i)
        {
            var counters = new HashSet<long>();
            if (string.Equals(_dot.I, i, StringComparison.OrdinalIgnoreCase))
            {
                counters.Add(_dot.N);
            }

            counters.Add(_v[i]);

            return counters.Max();
        }

        public override string ToString()
        {
            return $"({_dot},{_v})";
        }

        static IParser<object> CreateParser()
        {
            var config = ParserFactory.Fluent();

            var dvv = config.Rule();
            var ce = config.Rule(); // causal event
            var vv = config.Rule(); // version vector

            var serverId = config.Expression();
            serverId.ThatMatches(@"[a-z]+").AndReturns(x => x);

            var longValue = config.Expression();
            longValue.ThatMatches(@"\d+").AndReturns(x => long.Parse(x));

            ce.IsMadeUp.By("(")
                .Followed.By(serverId).As("i")
                .Followed.By(",")
                .Followed.By(longValue).As("n")
                .Followed.By(")")
                .WhenFound(x => new CausalEvent(x.i, x.n));

            vv.IsMadeUp.By("{")
                .Followed.ByListOf<CausalEvent>(ce).As("events").ThatIs.SeparatedBy(",").Optional
                .Followed.By("}")
                .WhenFound(x => new VersionVector(x.events ?? Enumerable.Empty<CausalEvent>()));

            dvv.IsMadeUp.By("(")
                .Followed.By(ce).As("ce")
                .Followed.By(",")
                .Followed.By(vv).As("vv")
                .Followed.By(")")
                .WhenFound(x => new DottedVersionVector(x.ce, x.vv));

            IParser<object> parser = config.CreateParser();
            return parser;
        }

        public static DottedVersionVector Parse(string value)
        {
            var parsed = (DottedVersionVector)Parser.Value.Parse(value);
            return parsed;
        }

        #region Equality Members

        public bool Equals(DottedVersionVector other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return _dot.Equals(other._dot);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            return obj is DottedVersionVector && Equals((DottedVersionVector) obj);
        }

        public override int GetHashCode()
        {
            return _dot.GetHashCode();
        }

        #endregion

        public JToken ToJson()
        {
            return JValue.CreateString(ToString());
        }

        public static DottedVersionVector FromJson(JToken token)
        {
            string value = token.Value<string>();
            DottedVersionVector dvv = Parse(value);
            return dvv;
        }
    }
}