using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Wildling.Core.Tests.SupportingTypes
{
    public class TestableNode : Node
    {
        public TestableNode(string name, IEnumerable<string> nodes, int partitions = 32) : base(name, nodes, partitions)
        {
        }

        protected override Task RemotePutAsync(string node, string key, object value)
        {
            // Invoke public virtual method to provide test seam
            return RemotePutAsyncTestSeam(node, key, value);
        }

        public virtual Task RemotePutAsyncTestSeam(string node, string key, object value)
        {
            throw new NotImplementedException("The stub/mock should override RemotePutAsyncTestSeam");
        }

        protected override Task<object> RemoteGetAsync(string node, string key)
        {
            return RemoteGetAsyncTestSeam(node, key);
        }

        public virtual Task<object> RemoteGetAsyncTestSeam(string node, string key)
        {
            // Invoke public virtual method to provide test seam
            throw new NotImplementedException("The stub/mock should override RemoteGetAsyncTestSeam");
        }
    }
}