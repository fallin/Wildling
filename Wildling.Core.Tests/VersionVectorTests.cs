using System;
using FluentAssertions;
using NUnit.Framework;

namespace Wildling.Core.Tests
{
    [TestFixture]
    public class VersionVectorTests
    {
        [Test]
        public void ToString_should_handle_and_empty_set()
        {
            var vv = new VersionVector();
            vv.ToString().Should().Be("{}");
        }

        [Test]
        public void ToString_should_handle_single_event()
        {
            var vv = new VersionVector(new CausalEvent("r", 1));
            vv.ToString().Should().Be("{(r,1)}");
        }

        [Test]
        public void ToString_should_handle_multiple_events()
        {
            var vv = new VersionVector(new[] {new CausalEvent("r", 1), new CausalEvent("s", 1)});
            vv.ToString().Should().Be("{(r,1),(s,1)}");
        }

        [Test]
        public void Indexer_should_return_counter_for_id()
        {
            var vv = new VersionVector(new CausalEvent("r", 1));
            vv["r"].Should().Be(1);
        }

        [Test]
        public void Indexer_should_return_counter_for_id_regardless_of_case()
        {
            var vv = new VersionVector(new CausalEvent("r", 1));
            vv["R"].Should().Be(1);
        }

        [Test]
        public void Indexer_should_return_zero_for_missing_id()
        {
            var vv = new VersionVector(new CausalEvent("r", 1));
            vv["x"].Should().Be(0);
        }

        [Test]
        public void Ids_should_return_set_of_identifiers()
        {
            var vv = new VersionVector(new[] { new CausalEvent("r", 1), new CausalEvent("s", 1) });
            var ids = vv.Ids();

            ids.Should().Contain(new[] { "r", "s" });
            ids.Should().HaveCount(2);
            ids.Should().OnlyHaveUniqueItems();
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase("  ")]
        public void Parse_should_return_empty_vv_when_value_is_null_or_whitespace(object value)
        {
            var vv = VersionVector.Parse((string)value);
            vv.Events.Should().HaveCount(0);
        }

        [Test]
        public void Parse_should_return_empty_vv_when_no_causal_events()
        {
            var vv = VersionVector.Parse("{}");
            vv.Events.Should().HaveCount(0);
        }

        [Test]
        public void Parse_should_handle_single_event()
        {
            var vv = VersionVector.Parse("{(r,1)}");
            vv["r"].Should().Be(1);
            vv["s"].Should().Be(0);
        }

        [Test]
        public void Parse_should_handle_multiple_events()
        {
            var vv = VersionVector.Parse("{(r,1),(s,3)}");
            vv["r"].Should().Be(1);
            vv["s"].Should().Be(3);
        }
    }
}