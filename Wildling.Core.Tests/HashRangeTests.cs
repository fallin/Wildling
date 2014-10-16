using System;
using System.Numerics;
using FluentAssertions;
using NUnit.Framework;

namespace Wildling.Core.Tests
{
    [TestFixture]
    public class HashRangeTests
    {
        [Test]
        public void Covers_should_be_true_if_value_equals_start()
        {
            var range = new HashRange(0, 10);
            range.Covers(0).Should().BeTrue();
        }

        [Test]
        public void Covers_should_be_true_if_value_gt_start()
        {
            var range = new HashRange(0, 10);
            range.Covers(2).Should().BeTrue();
        }

        [Test]
        public void Covers_should_be_true_if_value_equals_end()
        {
            var range = new HashRange(0, 10);
            range.Covers(10).Should().BeTrue();
        }

        [Test]
        public void Covers_should_be_true_if_value_lt_end()
        {
            var range = new HashRange(0, 10);
            range.Covers(8).Should().BeTrue();
        }

        [Test]
        public void Covers_should_be_false_if_value_lt_start()
        {
            var range = new HashRange(0, 10);
            range.Covers(-1).Should().BeFalse();
        }

        [Test]
        public void Covers_should_be_false_if_value_gt_end()
        {
            var range = new HashRange(0, 10);
            range.Covers(11).Should().BeFalse();
        }

        [Test]
        public void Covers_should_be_true_if_big_value_gt_end()
        {
            var range = new HashRange(0, BigInteger.Parse("45671926166590716193865151022383844364247891967"));
            var value = BigInteger.Parse("294255062699127052481571644205017775360447081995");
            range.Covers(value).Should().BeFalse();
        }

        [Test]
        public void Covers_should_be_true_if_big_value_lt_end()
        {
            var range = new HashRange(0, BigInteger.Parse("45671926166590716193865151022383844364247891967"));
            var value = BigInteger.Parse("45671926166590716193865151022383844364247891966");
            range.Covers(value).Should().BeTrue();
        }
    }
}