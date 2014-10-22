using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Wildling.Core
{
    /// <summary>
    /// An object to store in a server node
    /// </summary>
    public class NodeObject
    {
        readonly JObject _value;

        public NodeObject(JObject value)
        {
            _value = value;
        }

        public JObject Value
        {
            get { return _value; }
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(_value);
        }

        public static NodeObject Parse(string json)
        {
            return new NodeObject(JObject.Parse(json));
        }
    }
}