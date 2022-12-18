using System.Collections.Immutable;

namespace SCClassicalPlanning.InternalUtilities
{
    internal static class ImmutableHashSetExtensions
    {
        /// <summary>
        /// Determines whether the current set and the specified set contain the same elements.
        /// <para/>
        /// NB: In general, <see cref="ISet{T}.SetEquals(IEnumerable{T})"/> implementations have to handle the possibility that duplicates
        /// occur in the argument - note that even if the argument is another set, there is still the possibility that the equality
        /// comparers in use by each set differ. This necessarily slows it down. By accepting an immutable hash set in this method and
        /// checking its key comparer, we can avoid a bit of work if the key comparers are the same object (as they will tend to be for the 
        /// sets in this library).
        /// </summary>
        /// <typeparam name="T">The type of the elements held in both sets.</typeparam>
        /// <param name="thisSet">The current set.</param>
        /// <param name="otherSet">The other set.</param>
        /// <returns>True if and only if the two sets contain the same elements.</returns>
        public static bool SetEquals<T>(this ImmutableHashSet<T> thisSet, ImmutableHashSet<T> otherSet)
        {
            if (!object.ReferenceEquals(thisSet.KeyComparer, otherSet.KeyComparer))
            {
                // Fall back on the normal set equals implementation if the comparers aren't the same object.
                return thisSet.SetEquals(otherSet);
            }

            if (object.ReferenceEquals(thisSet, otherSet))
            {
                return true;
            }

            if (thisSet.Count != otherSet.Count)
            {
                return false;
            }

            foreach (var element in otherSet)
            {
                if (!thisSet.Contains(element))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
