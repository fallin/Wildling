using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Wildling.Core.Converters
{
    abstract class JsonSerializableJsonConverter<T> : JsonConverter where T : IJsonSerializable
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var t = (T)value;
            if (!ReferenceEquals(t, null))
            {
                JToken token = t.ToJson();
                token.WriteTo(writer);
            }
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(T).IsAssignableFrom(objectType);
        }
    }
}