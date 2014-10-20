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
        public void Shift_should_return_null_when_string_list_is_empty()
        {
            IList<string> list = new string[0];
            string result = list.Shift();
            result.Should().BeNull();
        }

        [Test]
        public void Shift_should_return_null_when_int_list_is_empty()
        {
            IList<int> list = new int[0];
            int result = list.Shift();
            result.Should().Be(0);
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