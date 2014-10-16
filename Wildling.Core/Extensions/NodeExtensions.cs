using System;
using System.Threading;
using System.Threading.Tasks;

namespace Wildling.Core.Extensions
{
    public static class NodeExtensions
    {
        public static void Put(this Node node, string key, object value)
        {
            Task.Factory.StartNew(
                s => ((Node)s).PutAsync(key, value),
                node, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default
                )
                .Unwrap().GetAwaiter().GetResult();
        }

        public static object Get(this Node node, string key)
        {
            return Task.Factory.StartNew(
                s => ((Node)s).GetAsync(key),
                node, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default
                )
                .Unwrap().GetAwaiter().GetResult();
        }
    }
}