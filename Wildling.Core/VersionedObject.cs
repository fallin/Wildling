using System;
using System.Diagnostics;
using EnsureThat;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Wildling.Core.Converters;

namespace Wildling.Core
{
    /// <summary>
    /// An JSON object with causal (version) information.
    /// </summary>
    [JsonConverter(typeof(VersionedObjectJsonConverter))]
    [DebuggerDisplay("Dot={Clock._dot}")]
    public sealed class VersionedObject : IEquatable<VersionedObject>, IJsonSerializable
    {
        public JObject Value { get; private set; }
        public DottedVersionVector Clock { get; private set; }

        public VersionedObject(JObject value, DottedVersionVector clock)
        {
            Ensure.That(value, "value").IsNotNull();
            Ensure.That(clock, "clock").IsNotNull();

            Value = value;
            Clock = clock;
        }

        public bool HappensBefore(VersionedObject other)
        {
            return Clock.HappensBefore(other.Clock);
        }

        #region Equality Members

        public bool Equals(VersionedObject other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return Clock.Equals(other.Clock);
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
            if (obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((VersionedObject) obj);
        }

        public override int GetHashCode()
        {
            return Clock.GetHashCode();
        }

        #endregion

        public JToken ToJson()
        {
            var jsonObject = new JObject(
                new JProperty("value", Value),
                new JProperty("clock", Clock.ToJson())
            );

            return jsonObject;
        }

        public static VersionedObject FromJson(JToken token)
        {
            JObject value = (JObject) token["value"];
            DottedVersionVector dvv = DottedVersionVector.FromJson(token["clock"]);
            return new VersionedObject(value, dvv);
        }
    }
}