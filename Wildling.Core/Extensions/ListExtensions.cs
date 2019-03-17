using System;
using System.Collections.Generic;
using System.Linq;
using EnsureThat;

namespace Wildling.Core.Extensions
{
    /// <summary>
    /// Provides additional (ruby/javascript-like) features to lists.
    /// </summary>
    static class ListExtensions
    {
        /// <summary>
        /// Returns and removes the first element of the list (shifts other elements down by one).
        /// </summary>
        /// <typeparam name="T">The type of elements in the list.</typeparam>
        /// <param name="list">The list.</param>
        /// <returns>T</returns>
        /// <remarks>Returns nil if the array is empty.</remarks>
        public static T Shift<T>(this IList<T> list)
        {
            Ensure.That(list).IsNotNull();

            T value;
            if (list.Count > 0)
            {
                value = list[0];
                list.RemoveAt(0);
            }
            else
            {
                value = default(T);
            }

            return value;
        }

        //public static IList<T> Unshift<T>(this IList<T> list, T value)
        //{
        //    list.Insert(0, value);
        //    return list;
        //}
    }
}