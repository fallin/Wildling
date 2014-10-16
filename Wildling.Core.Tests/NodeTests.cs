using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Wildling.Core.Extensions;
using Wildling.Core.Tests.SupportingTypes;

namespace Wildling.Core.Tests
{
    [TestFixture]
    public class NodeTests
    {
        [Test]
        public void Put_should_store_value_for_key_when_node_owns_partition()
        {
            var node = CreateSubstituteNode("A", new[] { "A", "B", "C" });

            node.Put("foo", "bar");
            node.Get("foo").Should().Be("bar");
        }

        [Test]
        public void Put_should_not_store_value_for_key_when_it_does_not_own_partition()
        {
            var node = CreateSubstituteNode("B", new[] { "A", "B", "C" }, 32);

            node.Put("foo", "bar");
            node.ReceivedWithAnyArgs().RemotePutAsyncTestSeam(null, null, null);
        }

        [Test]
        public void Put_should_only_be_stored_by_a_single_node_in_cluster()
        {
            var names = new CharRange('A', 'J').ToStrings().ToArray();
            var nodes = names.Select(name => CreateSubstituteNode(name, names));

            int nodesStoringValue = 0;
            foreach (var node in nodes)
            {
                node.Put("foo", "bar");
                object value = node.Get("foo");
                if (value != null && value.GetType() != typeof(HttpResponseMessage))
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

        TestableNode CreateSubstituteNode(string name, IEnumerable<string> nodes, int partitions = 32)
        {
            var node = Substitute.ForPartsOf<TestableNode>(name, nodes, partitions);
            node.WhenForAnyArgs(n => n.RemotePutAsyncTestSeam(null, null, null)).DoNotCallBase();
            node.WhenForAnyArgs(n => n.RemoteGetAsyncTestSeam(null, null)).DoNotCallBase();
            return node;
        }
    }
}