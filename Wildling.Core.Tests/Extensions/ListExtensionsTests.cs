using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Wildling.Core.Extensions;

namespace Wildling.Core.Tests.Extensions
{
    [TestFixture]
    public class ListExtensionsTests
    {
        [Test]
        public void Shift_should_throw_when_list_is_null()
        {
            IList<int> list = null;

            // ReSharper disable once ExpressionIsAlwaysNull
            Action action = () => list.Shift();

            action.ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void Shift_should_throw_when_list_is_empty()
        {
            IList<int> list = new int[0];

            Action action = () => list.Shift();

            action.ShouldThrow<ArgumentOutOfRangeException>();
        }

        [Test]
        public void Shift_should_return_and_remove_first_element_of_the_list()
        {
            IList<int> list = new List<int> {1, 2, 3};

            int result = list.Shift();
            result.Should().Be(1);
            list.Should().Contain(new[] { 2, 3});
        }
    }
}