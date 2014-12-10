using System;
using System.Collections.Generic;
using EnsureThat;

namespace Wildling.Core.Extensions
{
    static class DictionaryExtensions
    {
        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            Ensure.That(dictionary, "dictionary").IsNotNull();

            TValue value;
            if (!dictionary.TryGetValue(key, out value))
            {
                value = default(TValue);
            }
            return value;
        }
    }
}