using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using Wildling.Core.Extensions;

namespace Wildling.Core
{
    class PartitionedConsistentHash
    {
        const int SHA1Bits = 160;
        readonly List<string> _nodes;
        readonly int _partitions;
        readonly SortedDictionary<HashRange, string> _ring = new SortedDictionary<HashRange, string>();

        public PartitionedConsistentHash(IEnumerable<string> nodes, int partitions = 32)
        {
            ValidatePartitionsArgument(partitions);

            _partitions = partitions;
            _nodes = Cluster(nodes);
        }

        /// <summary>
        /// Calculates the hash of the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The hash value of the key.</returns>
        public BigInteger Hash(string key)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(key);
            using (var crypto = SHA1.Create())
            {
                byte[] hashBytes = crypto.ComputeHash(buffer);

                // This isn't really necessary, but maintains compatibility with Eric Redmond's
                // ruby implementation of distributed data-structures teaching tool/samples:
                // Presumably, ruby treats byte[] as big-endian when converting to integer (string#hex)
                //Array.Reverse(hashBytes);

                // Make sure that a positive value is not incorrectly instantiated as a 
                // negative value by adding a byte whose value is zero to the end of the array
                if ((hashBytes[hashBytes.Length - 1] & 0x80) > 0)
                {
                    byte[] temp = new byte[hashBytes.Length];
                    Array.Copy(hashBytes, temp, hashBytes.Length);
                    hashBytes = new byte[temp.Length + 1];
                    Array.Copy(temp, hashBytes, temp.Length);
                }

                return new BigInteger(hashBytes);
            }
        }

        public void Add(string node)
        {
            // every N partitions, reassign to the new node
            _nodes.Add(node);
            int pow = SHA1Bits - (int)Log2(_partitions);

            for (int i = 0; i < _partitions; i += _nodes.Count)
            {
                _ring[Range(i, pow)] = node;
            }
        }

        /// <summary>
        /// Gets the coordinating node for the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The name of the coordinating node.</returns>
        public string Node(string key)
        {
            // returns the correct node in the ring (for the key)
            string node = null;
            if (!string.IsNullOrEmpty(key))
            {
                BigInteger hash = Hash(key);
                node = _ring.First(pair => pair.Key.Covers(hash)).Value;
            }
            return node;
        }

        public IList<string> PreferenceList(string key, int n = 3)
        {
            // return a list of successive nodes that can also hold this value
            var list = new List<string>();
            BigInteger hash = Hash(key);
            int cover = n;

            foreach (KeyValuePair<HashRange, string> pair in _ring)
            {
                HashRange range = pair.Key;
                string node = pair.Value;
                if (range.Covers(hash) || (cover < n && cover > 0))
                {
                    list.Add(node);
                    cover -= 1;
                }
            }
            return list;
        }

        List<string> Cluster(IEnumerable<string> nodes)
        {
            List<string> n = nodes.Distinct().ToList();
            n.Sort();

            int pow = SHA1Bits - (int)Log2(_partitions);

            for (int i = 0; i < _partitions; i++)
            {
                _ring[Range(i, pow)] = n[0];
                n.Add(n.Shift());
            }

            n.Sort();
            return n;
        }

        static void ValidatePartitionsArgument(int partitions)
        {
            // partitions must be a power of 2
            double power = Log2(partitions);
            if (Math.Abs(power - (int) power) > Double.Epsilon)
            {
                throw new ArgumentOutOfRangeException("partitions", partitions, "Must be a power of 2");
            }
        }

        HashRange Range(int partition, int power)
        {
            BigInteger start = partition *  BigInteger.Pow(2, power);
            BigInteger end = (partition + 1) * BigInteger.Pow(2, power) - 1;
            return new HashRange(start, end);
        }

        static double Log2(double value)
        {
            return Math.Log(value, 2);
        }

        //static string Hexify(IEnumerable<byte> bytes)
        //{
        //    const int capacity = 40; // SHA1 contains 40 characters
        //    StringBuilder builder = bytes.Aggregate(new StringBuilder(capacity),
        //        (a, b) => a.AppendFormat("{0:x2}", b));
        //    return builder.ToString();
        //}
    }
}