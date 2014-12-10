using System;
using System.Collections.Generic;
using System.Numerics;

namespace Wildling.Core
{
    class HashRange : Range<BigInteger>
    {
        public HashRange(BigInteger start, BigInteger end) : base(start, end)
        {
        }

        public override IEnumerator<BigInteger> GetEnumerator()
        {
            for (BigInteger c = Start; c <= End; c++)
            {
                yield return c;
            }
        }
    }
}