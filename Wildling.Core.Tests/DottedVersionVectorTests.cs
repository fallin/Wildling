using System;
using FluentAssertions;
using NUnit.Framework;

namespace Wildling.Core.Tests
{
    [TestFixture]
    public class DottedVersionVectorTests
    {
        [Test]
        public void Parse_should_return_ddv_with_one_event_in_causal_history()
        {
            const string input = "((r,3),{(s,1)})";
            var dvv = DottedVersionVector.Parse(input);

            // Assert
            dvv.ToString().Should().Be(input);
        }

        [Test]
        public void Parse_should_return_ddv_with_two_events_in_causal_history()
        {
            const string input = "((r,3),{(s,1),(t,5)})";
            var dvv = DottedVersionVector.Parse(input);

            // Assert
            dvv.ToString().Should().Be(input);
        }

        [Test]
        public void Parse_should_return_dvv_with_no_events_in_causal_history()
        {
            const string input = "((r,3),{})";
            var dvv = DottedVersionVector.Parse(input);

            // Assert
            dvv.ToString().Should().Be(input);
        }

        [Test]
        public void Ids_should_return_set_of_identifiers()
        {
            const string input = "((r,3),{(s,1),(t,5)})";
            var dvv = DottedVersionVector.Parse(input);
            var ids = dvv.Ids();

            ids.Should().Contain(new[] { "r", "s", "t" });
            ids.Should().HaveCount(3);
            ids.Should().OnlyHaveUniqueItems();
        }

        [Test]
        public void MaxDot_should_return_the_max_counter_for_a_server_id_in_dot()
        {
            const string input = "((r,3),{(s,1),(t,5)})";
            var dvv = DottedVersionVector.Parse(input);

            long counter = dvv.MaxDot("r");
            counter.Should().Be(3);
        }

        [Test]
        public void MaxDot_should_return_the_max_counter_for_a_server_id_in_vv()
        {
            const string input = "((r,3),{(s,1),(t,5)})";
            var dvv = DottedVersionVector.Parse(input);

            long counter = dvv.MaxDot("t");
            counter.Should().Be(5);
        }

        [Test]
        public void MaxDot_should_return_the_max_counter_for_a_server_id_not_found()
        {
            const string input = "((r,3),{(s,1),(t,5)})";
            var dvv = DottedVersionVector.Parse(input);

            long counter = dvv.MaxDot("x");
            counter.Should().Be(0);
        }
    }
}