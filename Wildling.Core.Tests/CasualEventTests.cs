using System;
using FluentAssertions;
using NUnit.Framework;

namespace Wildling.Core.Tests
{
    [TestFixture]
    public class CasualEventTests
    {
        [Test]
        public void ToString_should_return_representation_of_event()
        {
            // Arrange
            var vve = new CausalEvent("r", 1);

            // Act
            string repr = vve.ToString();

            // Assert
            repr.Should().Be("(r,1)");
        }
    }
}