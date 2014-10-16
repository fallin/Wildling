using System;
using Newtonsoft.Json;

namespace Wildling.Core
{
    /// <summary>
    /// An object to store in a server node
    /// </summary>
    public class NodeObject
    {
        readonly object _value;

        public NodeObject(object value)
        {
            _value = value;
        }

        public object Value
        {
            get { return _value; }
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(_value);
        }

        public static NodeObject Parse(string json)
        {
            object value = JsonConvert.DeserializeObject(json);
            return new NodeObject(value);
        }
    }
}