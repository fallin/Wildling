using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Wildling.Core.Converters
{
    class DottedVersionVectorJsonConverter : JsonSerializableJsonConverter<DottedVersionVector>
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);
            return DottedVersionVector.FromJson(token);
        }
    }
}