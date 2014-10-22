using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Wildling.Core.Tests.SupportingTypes;

namespace Wildling.Core.Tests
{
    [TestFixture]
    public class NodeTests
    {
        [Test]
        public async Task Put_should_store_value_for_key_when_node_owns_partition()
        {
            var node = new Node("A", new[] { "A", "B", "C" });
            var remote = new Mock<IRemoteNodeClient>();
            node.UseRemoteNodeClient(remote.Object);

            await node.PutAsync("foo", JObject.Parse("{'value':'bar'}"));

            JArray result = await node.GetAsync("foo");
            string value = ((dynamic) result[0]).value;
            value.Should().Be("bar");
        }

        [Test]
        public async Task Put_should_not_store_value_for_key_when_it_does_not_own_partition()
        {
            var node = new Node("B", new[] { "A", "B", "C" }, 32);
            var remote = new Mock<IRemoteNodeClient>();
            node.UseRemoteNodeClient(remote.Object);

            await node.PutAsync("foo", JObject.Parse("{'value':'bar'}"));
            remote.Verify(x => x.RemotePutAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<JObject>()));
        }

        [Test]
        public async Task Put_should_only_be_stored_by_a_single_node_in_cluster()
        {
            var names = new CharRange('A', 'J').ToStrings().ToArray();
            var nodes = names.Select(name =>
            {
                var node = new Node(name, names);
                var remote = new Mock<IRemoteNodeClient>();
                node.UseRemoteNodeClient(remote.Object);
                return node;
            });

            int nodesStoringValue = 0;
            foreach (var node in nodes)
            {
                await node.PutAsync("foo", JObject.Parse("{'value':'bar'}"));
                JArray value = await node.GetAsync("foo");
                if (value != null)
                {
                    nodesStoringValue++;
                    Console.WriteLine("{0} {1}", node.Name, value);
                }
            }
            nodesStoringValue.Should().Be(1);
        }

        [Test]
        public void Ctor_should_generate_name_if_null()
        {
            var node = new Node(null, Enumerable.Empty<string>());
            node.Name.Should().NotBeNullOrEmpty("A node should always have a valid name");
        }
    }
}