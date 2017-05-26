using System;
using EnsureThat;

namespace Wildling.Core
{
    public class CausalEvent : IEquatable<CausalEvent>
    {
        readonly string _i; // id
        readonly long _n; // counter

        public CausalEvent(string i, long n)
        {
            Ensure.That(i, "i").IsNotNullOrWhiteSpace();
            Ensure.That(n, "n").IsGt(0);

            _i = i;
            _n = n;
        }

        public override string ToString()
        {
            return $"({_i},{_n})";
        }

        /// <summary>
        /// The server (replica node) identifier
        /// </summary>
        public string I => _i;

        /// <summary>
        /// The counter representing a unique write
        /// </summary>
        public long N => _n;

        #region Equality Members

        public bool Equals(CausalEvent other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return string.Equals(_i, other._i, StringComparison.OrdinalIgnoreCase) 
                && _n == other._n;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((CausalEvent) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((_i != null ? _i.GetHashCode() : 0)*397) ^ _n.GetHashCode();
            }
        }

        #endregion
    }
}