using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Wildling.Core
{
    interface IRemoteNodeClient
    {
        Task PutAsync(string node, string key, JToken value, VersionVector context);
        Task<Siblings> GetAsync(string node, string key);

        Task PutReplicaAsync(string node, string key, Siblings siblings);
        Task<Siblings> GetReplicaAsync(string node, string key);
    }
}