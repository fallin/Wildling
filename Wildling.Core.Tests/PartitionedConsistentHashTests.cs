using System;
using System.Collections.Generic;
using System.Numerics;
using FluentAssertions;
using NUnit.Framework;
using Wildling.Core.Extensions;
using Wildling.Core.Tests.SupportingTypes;

namespace Wildling.Core.Tests
{
    [TestFixture]
    public class PartitionedConsistentHashTests
    {
        [Test]
        public void Node_should_return_appropriate_node_associated_with_partition()
        {
            IEnumerable<string> nodes = new CharRange('A', 'C').ToStrings();
            var ch = new PartitionedConsistentHash(nodes, 32);

            string node = ch.Node("foo");
            node.Should().Be("A");
        }

        [Test]
        public void Hash_should_return_appropriate_values()
        {
            IEnumerable<string> nodes = new CharRange('A', 'C').ToStrings();
            var ch = new PartitionedConsistentHash(nodes, 32);

            ch.Hash("foo").Should().Be(BigInteger.Parse("294255062699127052481571644205017775360447081995"));
            ch.Hash("foo1").Should().Be(BigInteger.Parse("651913850979875114214452572601928477260433432856"));
            ch.Hash("foo2").Should().Be(BigInteger.Parse("225616181129260556051456902711111941755487497642"));
        }

        [Test]
        public void PreferenceList()
        {
            IEnumerable<string> nodes = new CharRange('A', 'J').ToStrings();
            var ch = new PartitionedConsistentHash(nodes, 32);

            string[] preferenceList = ch.PreferenceList("foo", 3);

            preferenceList.Should().HaveCount(3);
            Console.WriteLine(string.Join(",", preferenceList));
        }
    }
}