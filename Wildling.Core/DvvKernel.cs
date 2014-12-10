using System;
using System.Collections.Generic;
using System.Linq;
using EnsureThat;

namespace Wildling.Core
{
    class DvvKernel
    {
        public Siblings Sync(Siblings s1, Siblings s2)
        {
            Ensure.That(s1, "s1").IsNotNull();
            Ensure.That(s2, "s2").IsNotNull();

            IEnumerable<VersionedObject> r1 = s1.Where(sibling1 => !s2.Any(sibling1.HappensBefore));
            IEnumerable<VersionedObject> r2 = s2.Where(sibling2 => !s1.Any(sibling2.HappensBefore));

            IEnumerable<VersionedObject> union = r1.Union(r2).ToList();

            return new Siblings(union);
        }

        public VersionVector Join(Siblings s1)
        {
            Ensure.That(s1, "s1").IsNotNull();

            var ids = s1.Ids().Select(i => new CausalEvent(i, s1.MaxDot(i)));
            return new VersionVector(ids);
        }

        /// <summary>
        /// Remove obsolete versions.
        /// </summary>
        public Siblings Discard(Siblings s, VersionVector context)
        {
            Ensure.That(s, "s").IsNotNull();
            Ensure.That(context, "context").IsNotNull();

            // discard all siblings that are obsolete because they are included
            // in the context.
            IEnumerable<VersionedObject> concurrent = s.Where(sibling => !sibling.Clock.HappensBefore(context));
            return new Siblings(concurrent);
        }

        /// <summary>
        /// Generates a new clock.
        /// </summary>
        public DottedVersionVector Event(VersionVector context, Siblings s, string i)
        {
            Ensure.That(s, "s").IsNotNull();
            Ensure.That(context, "context").IsNotNull();
            Ensure.That(i, "i").IsNotNullOrEmpty();

            long maxDot = s.MaxDot(i);
            long maxCausalHistory = context[i];

            long maxCounter = Math.Max(maxDot, maxCausalHistory);
            var dot = new CausalEvent(i, maxCounter + 1);

            return new DottedVersionVector(dot, context);
        }
    }

    static class DvvKernelExtensions
    {
        public static Siblings Merge(this DvvKernel kernel, IList<Siblings> siblingsList)
        {
            Ensure.That(siblingsList, "siblingsList").IsNotNull();

            Siblings merged = siblingsList.Aggregate(kernel.Sync);
            return merged;
        }
    }
}
