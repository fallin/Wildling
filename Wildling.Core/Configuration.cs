using System;
using System.Collections.Generic;
using System.IO;
using Common.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Wildling.Core
{
    public class Configuration
    {
        static readonly ILog Log = LogManager.GetCurrentClassLogger();
        readonly Dictionary<string, JObject> _configs;
        const StringComparison Comparison = StringComparison.CurrentCultureIgnoreCase;

        public static readonly Configuration Default = new Configuration();

        private Configuration()
        {
            _configs = new Dictionary<string, JObject>(StringComparer.CurrentCultureIgnoreCase);
        }

        public T Read<T>(string key, T defaultValue, string name)
        {
            T value = defaultValue;
            JToken jtoken;
            if (GetNodeConfig(name).TryGetValue(key, Comparison, out jtoken))
            {
                value = jtoken.ToObject<T>();
            }

            return value;
        }

        JObject GetNodeConfig(string name)
        {
            JObject nodeConfig;
            if (!_configs.TryGetValue(name, out nodeConfig))
            {
                string fileName = Path.Combine("Configuration", string.Format("{0}.json", name));
                if (File.Exists(fileName))
                {
                    //Log.TraceFormat("Configuration file [{0}] exists", fileName);
                    StreamReader streamReader = File.OpenText(fileName);
                    using (JsonReader jsonReader = new JsonTextReader(streamReader))
                    {
                        nodeConfig = JObject.Load(jsonReader);
                    }
                }
                else
                {
                    Log.WarnFormat("Configuration file [{0}] does not exist -- using defaults", fileName);
                    nodeConfig = new JObject();
                }

                _configs.Add(name, nodeConfig);
            }
            return nodeConfig;
        }
    }
}