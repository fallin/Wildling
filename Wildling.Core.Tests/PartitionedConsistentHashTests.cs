using System;
using System.Collections.Generic;
using System.Numerics;
using FluentAssertions;
using NUnit.Framework;
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
            node.Should().Be("B");
        }

        [Test]
        public void Hash_should_generate_correct_value()
        {
            // Generated SHA1 hash (for comparison) using npm 'sha1' or http://www.sha1-online.com/

            // Generating a SHA1 in .NET is trivial, but this gives us a byte[]. We convert this
            // to a BigInteger to make it easier to work and perform comparisons. BigInteger treats
            // the byte array as little-endian which gives different results than you'll get from
            // other SHA1 hash functions...

            var ch = new PartitionedConsistentHash(new[] {"a"});
            var result = ch.Hash("foo");

            result.ToString("x").Should().Be("0beec7b5ea3f0fdbc95d0dd47f3c5bc275da8a33");
            result.Should().Be(BigInteger.Parse("68123873083688143418383284816464454849230703155"));

            // The Yubico ModHex converter (demo website) has some convenient hex-to-number conversions
            // which is helpful to ensure BigInteger is providing the values we're expecting.
        }

        [Test]
        public void Hash_should_be_correct_value_when_high_order_bit_is_one()
        {
            var ch = new PartitionedConsistentHash(new[] { "a" });

            var result = ch.Hash("this is a test");
            result.Should().BeGreaterThan(0);

            // Note: the 0 prefix is added by BigInteger to indicate it's a positive value
            result.ToString("x").Should().Be("0fa26be19de6bff93f70bc2308434e4a440bbad02");
            result.Should().Be(BigInteger.Parse("1428111681160539626773549155626795980060565941506"));
        }

        [Test]
        public void PreferenceList()
        {
            IEnumerable<string> nodes = new CharRange('A', 'J').ToStrings();
            var ch = new PartitionedConsistentHash(nodes, 32);

            string node = ch.Node("foo");
            node.Should().Be("B");

            var preferenceList = ch.PreferenceList("foo", 3); // belongs to node A
            preferenceList.Should().BeEquivalentTo(new[] { "B", "C", "D" });
        }
    }
}