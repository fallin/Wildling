using System;
using System.Collections.Generic;
using System.Linq;

namespace Wildling.Core.Tests.SupportingTypes
{
    public class CharRange : Range<char>
    {
        public CharRange(char start, char end) : base(start, end)
        {
        }

        public override IEnumerator<char> GetEnumerator()
        {
            for (char c = Start; c <= End; c++)
            {
                yield return c;
            }
        }

        public IEnumerable<string> ToStrings()
        {
            return this.Select(c => new string(c, 1));
        }
    }
}