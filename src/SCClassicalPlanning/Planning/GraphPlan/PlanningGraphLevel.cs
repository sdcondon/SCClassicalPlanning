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
    /// Representation of a (proposition) level within a planning graph.
    /// </summary>
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
        /// Gets all of the proposition nodes in this level, keyed by their respective propositions.
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
        /// Gets an enumerable of the propositions in this level.
        /// </summary>
        public IEnumerable<Literal> Propositions => NodesByProposition.Keys;

        /// <summary>
        /// Gets an enumerable of the proposition nodes in this level.
        /// </summary>
        public IEnumerable<PlanningGraphPropositionNode> Nodes => NodesByProposition.Values;

        /// <summary>
        /// Gets a value indicating whether this level is after the point at which the graph levels off.
        /// That is, whether it is identical to the previous level.
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
