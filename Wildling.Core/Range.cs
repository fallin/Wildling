using System;
using System.Collections;
using System.Collections.Generic;
using EnsureThat;

namespace Wildling.Core
{
    abstract class Range<T> : IEnumerable<T>, IEquatable<Range<T>>, IComparable<Range<T>> 
        where T : struct, IComparable<T>
    {
        readonly T _start;
        readonly T _end;

        protected Range(T start, T end)
        {
            Ensure.That(start, "start").IsLte(end);

            _start = start;
            _end = end;
        }

        protected T Start => _start;

        protected T End => _end;

        public bool Covers(T value)
        {
            return value.CompareTo(_start) >= 0 && value.CompareTo(_end) <= 0;
        }

        public bool Equals(Range<T> other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return _start.Equals(other._start) && _end.Equals(other._end);
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
            if (obj.GetType() != this.GetType())
            {
                return false;
            }
            return Equals((Range<T>) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (_start.GetHashCode()*397) ^ _end.GetHashCode();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public abstract IEnumerator<T> GetEnumerator();


        public int CompareTo(Range<T> other)
        {
            return Start.CompareTo(other.Start);
        }

        public override string ToString()
        {
            return $"{_start}..{_end}";
        }
    }
}