using System;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Wildling.Core.Tests
{
    [TestFixture]
    public class SiblingsTests
    {
        [Test]
        public void Ids_should_return_set_of_identifiers()
        {
            var clocks = new Siblings
            {
                {new JObject(new JProperty("v", 1)), DottedVersionVector.Parse("((r,1),{})")},
                {new JObject(new JProperty("v", 2)), DottedVersionVector.Parse("((r,2),{})")},
                {new JObject(new JProperty("v", 1)), DottedVersionVector.Parse("((s,1),{(t,4)})")},
            };

            var ids = clocks.Ids();
            ids.Should().Contain(new[] {"r", "s", "t"});
            ids.Should().HaveCount(3);
            ids.Should().OnlyHaveUniqueItems();
        }

        [Test]
        public void MaxDot_should_return_the_max_counter_for_a_server_id_in_dot()
        {
            var clocks = new Siblings
            {
                {new JObject(new JProperty("v", 1)), DottedVersionVector.Parse("((r,1),{})")},
                {new JObject(new JProperty("v", 2)), DottedVersionVector.Parse("((r,2),{})")},
                {new JObject(new JProperty("v", 1)), DottedVersionVector.Parse("((s,5),{(t,4)})")},
            };

            long counter = clocks.MaxDot("r");
            counter.Should().Be(2);
        }

        [Test]
        public void MaxDot_should_return_the_max_counter_for_a_server_id_in_vv()
        {
            var clocks = new Siblings
            {
                {new JObject(new JProperty("v", 1)), DottedVersionVector.Parse("((r,1),{})")},
                {new JObject(new JProperty("v", 2)), DottedVersionVector.Parse("((r,2),{})")},
                {new JObject(new JProperty("v", 1)), DottedVersionVector.Parse("((s,5),{(t,4)})")},
            };

            long counter = clocks.MaxDot("t");
            counter.Should().Be(4);
        }

        [Test]
        public void MaxDot_should_return_the_max_counter_for_a_server_id_not_found()
        {
            var clocks = new Siblings
            {
                {new JObject(new JProperty("v", 1)), DottedVersionVector.Parse("((r,1),{})")},
                {new JObject(new JProperty("v", 2)), DottedVersionVector.Parse("((r,2),{})")},
                {new JObject(new JProperty("v", 1)), DottedVersionVector.Parse("((s,5),{(t,4)})")},
            };

            long counter = clocks.MaxDot("x");
            counter.Should().Be(0);
        }

        [Test]
        public void MaxDot_should_return_zero_for_empty_set()
        {
            var clocks = new Siblings();

            long counter = clocks.MaxDot("x");
            counter.Should().Be(0);
        }
    }
}