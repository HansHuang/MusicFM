using System.Collections.Generic;
using System.Linq;

namespace CommonHelperLibrary
{
    /// <summary>
    /// Class: EnumerableHelper
    /// Author: Hans Huang @ Jungo Studio
    /// Create On: September 8st, 2014
    /// Description: Extension helper for Enumerable
    /// Version: 0.1
    /// </summary> 
    public static class EnumerableHelper
    {
        /// <summary>
        /// Split the chunk collection to several sublists
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">source chunk collectoin</param>
        /// <param name="chunkSize">the size of the sublist</param>
        /// <returns></returns>
        public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> source, int chunkSize)
        {
            while (source.Any())
            {
                yield return source.Take(chunkSize);
                source = source.Skip(chunkSize);
            }
        }
    }
}
