// Copyright 2022-2023 Simon Condon
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
using SCFirstOrderLogic;

namespace SCClassicalPlanning.Planning.GraphPlan
{
    /// <summary>
    /// <para>
    /// Representation of a (proposition) level within a planning graph.
    /// </para>
    /// </summary>
    // NB: this is a struct rather than a class ultimately to protect against
    // needless consumption of heap memory. Once a graph has levelled off,
    // we shouldn't need to allocate anything else on the heap, regardless of
    // how many "levels" we need to retrieve beyond that (in e.g. GraphPlan). 
    // This is easy to accomplish by making this type a struct.
    public readonly struct PlanningGraphLevel
    {
        internal PlanningGraphLevel(PlanningGraph graph, int index) => (Graph, Index) = (graph, index);

        /// <summary>
        /// Gets the planning graph in which this level resides.
        /// </summary>
        public PlanningGraph Graph { get; }

        /// <summary>
        /// Gets the index of this level - with index 0 indicating the propositions of the initial state of the problem.
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// <para>
        /// Gets all of the proposition nodes in this level, keyed by their respective propositions.
        /// </para>
        /// <para>
        /// NB: Be careful with this for the mo. See the comment against <see cref="Nodes"/> for details.
        /// </para>
        /// </summary>
        public IReadOnlyDictionary<Literal, PlanningGraphPropositionNode> NodesByProposition => Graph.GetNodesByProposition(Index);

        /// <summary>
        /// Gets the previous level of the graph. An exception will be thrown if the current level's index is 0.
        /// </summary>
        public PlanningGraphLevel PreviousLevel
        {
            get => Index >= 0 ? Graph.GetLevel(Index - 1) : throw new InvalidOperationException("Cannot retrieve the previous level to level 0");
        }

        /// <summary>
        /// Gets the next level of the graph.
        /// </summary>
        public PlanningGraphLevel NextLevel
        {
            get => Graph.GetLevel(Index + 1);
        }

        /// <summary>
        /// Gets an enumerable of the propositions in this level.
        /// </summary>
        public IEnumerable<Literal> Propositions => NodesByProposition.Keys;

        /// <summary>
        /// <para>
        /// Gets an enumerable of the proposition nodes in this level.
        /// </para>
        /// <para>
        /// NB: Be careful when following links back through the graph from here
        /// if <see cref="IsLevelledOff"/> is true. For ALL such (i.e. arbitrarily high-index)
        /// levels, the nodes of this prop will be the same - those of the final (levelled-off)
        /// level of the graph, and as such there will be no actions and all "backwards" links
        /// will be to the last distinct level of the graph.
        /// TODO-V1: this isn't really good enough - I should do something about this (by e.g.
        /// making public-facing nodes struct-valued).
        /// </para>
        /// </summary>
        public IEnumerable<PlanningGraphPropositionNode> Nodes => NodesByProposition.Values;

        /// <summary>
        /// Gets a value indicating whether this level is (strictly) after the point at which
        /// the graph levels off. That is, whether it is identical to the previous level.
        /// </summary>
        public bool IsLevelledOff => Graph.IsLevelledOff(Index);

        /// <summary>
        /// Gets a value indicating whether this level contains a given proposition.
        /// </summary>
        /// <param name="proposition">The proposition to check for.</param>
        /// <returns>True if and only if the given proposition is present in this level.</returns>
        public bool Contains(Literal proposition) => NodesByProposition.ContainsKey(proposition);

        /// <summary>
        /// Gets a value indicating whether all of a set of propositions are present at this level,
        /// with no pair of them being mutually exclusive.
        /// </summary>
        /// <param name="propositions">The propositions to look for.</param>
        /// <returns>True if and only if all the given propositions are present at this level, with no pair of them being mutually exclusive.</returns>
        public bool ContainsNonMutex(IEnumerable<Literal> propositions)
        {
            var propositionIndex = 0;
            foreach (var proposition in propositions)
            {
                if (!NodesByProposition.TryGetValue(proposition, out var node))
                {
                    return false;
                }

                if (node.Mutexes.Select(e => e.Proposition).Intersect(propositions.Take(propositionIndex)).Any())
                {
                    return false;
                }

                propositionIndex++;
            }

            return true;
        }
    }
}
