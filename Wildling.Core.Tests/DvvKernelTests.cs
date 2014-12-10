using System;
using System.Collections.Generic;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Wildling.Core.Tests
{
    [TestFixture]
    public class DvvKernelTests
    {
        [Test]
        public void Sync_should_return_concurrent_siblings()
        {
            var kernel = new DvvKernel();

            var s1 = new Siblings
            { 
                {new JObject(new JProperty("v", 1)), "((r,1),{})"},
                {new JObject(new JProperty("v", 2)), "((r,2),{})"}
            };

            var s2 = new Siblings
            {
                {new JObject(new JProperty("v", 3)), "((r,3),{(r,1)})"}
            };

            var siblings = kernel.Sync(s1, s2);

            // Assert
            siblings.Should().HaveCount(2);
            siblings.Should().NotContain(x => x.Clock.Equals(DottedVersionVector.Parse("((r,1),{})")));
            siblings.Should().Contain(x => x.Clock.Equals(DottedVersionVector.Parse("((r,2),{})")));
            siblings.Should().Contain(x => x.Clock.Equals(DottedVersionVector.Parse("((r,3),{(r,1)})")));
        }

        [Test]
        public void Sync_should_return_s2_when_input_empty_and_s2()
        {
            var kernel = new DvvKernel();

            var s1 = new Siblings();

            var s2 = new Siblings
            {
                {new JObject(new JProperty("v", 1)), "((r,1),{})"}
            };

            var siblings = kernel.Sync(s1, s2);

            // Assert
            siblings.Should().HaveCount(1);
            siblings.Should().Contain(x => x.Clock.Equals(DottedVersionVector.Parse("((r,1),{})")));
        }

        [Test]
        public void Sync_should_return_s1_when_input_s1_and_empty()
        {
            var kernel = new DvvKernel();

            var s1 = new Siblings
            {
                {new JObject(new JProperty("v", 1)), "((r,1),{})"}
            };

            var s2 = new Siblings();

            var siblings = kernel.Sync(s1, s2);

            // Assert
            siblings.Should().HaveCount(1);
            siblings.Should().Contain(x => x.Clock.Equals(DottedVersionVector.Parse("((r,1),{})")));
        }

        [Test]
        public void Sync_should_return_s1_when_input_s1_and_s1()
        {
            var kernel = new DvvKernel();

            var s1 = new Siblings
            { 
                {new JObject(new JProperty("v", 1)), "((r,1),{})"}
            };

            var siblings = kernel.Sync(s1, s1);

            // Assert
            siblings.Should().HaveCount(1);
            siblings.Should().Contain(x => x.Clock.Equals(DottedVersionVector.Parse("((r,1),{})")));
        }

        [Test]
        public void Discard_should_remove_obsolete_siblings()
        {
            var kernel = new DvvKernel();

            var s1 = new Siblings
            {
                {new JObject(new JProperty("v", 1)), "((r,1),{})"},
                {new JObject(new JProperty("v", 2)), "((r,2),{})"}
            };

            // This is the context we'd receive representing ((r,3),{(r,1)}) : v3
            var context = new VersionVector(new CausalEvent("r", 1));

            Siblings siblings = kernel.Discard(s1, context);

            // Assert
            siblings.Should().HaveCount(1);

            siblings.Should().NotContain(x => x.Clock.Equals(DottedVersionVector.Parse("((r,1),{})")));
            siblings.Should().Contain(x => x.Clock.Equals(DottedVersionVector.Parse("((r,2),{})")));
        }

        [Test]
        public void Join_should_return_a_clock_that_describes_the_collective_causal_past()
        {
            var kernel = new DvvKernel();

            var s1 = new Siblings
            {
                {new JObject(new JProperty("v", 1)), "((r,1),{})"},
                {new JObject(new JProperty("v", 2)), "((r,2),{(s,2)})"}
            };

            VersionVector vv = kernel.Join(s1);

            vv.Events.Should().Contain(new[]
            {
                new KeyValuePair<string, long>("r", 2),
                new KeyValuePair<string, long>("s", 2)
            });
        }

        [Test]
        public void Event_should_generate_a_new_clock_to_represent_a_new_version()
        {
            var kernel = new DvvKernel();

            var context = new VersionVector(new[]
            {
                new CausalEvent("s", 2),
                new CausalEvent("r", 2),
            });

            var s1 = new Siblings
            {
                {new JObject(new JProperty("v", 1)), "((r,1),{})"},
                {new JObject(new JProperty("v", 2)), "((r,2),{(s,2)})"}
            };

            DottedVersionVector dvv = kernel.Event(context, s1, "r");

            dvv.ToString().Should().Be("((r,3),{(r,2),(s,2)})");
        }
    }
}