using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Wildling.Core
{
    public interface IRemoteNodeClient
    {
        Task RemotePutAsync(string node, string key, JObject value);
        Task<JArray> RemoteGetAsync(string node, string key);
    }
}