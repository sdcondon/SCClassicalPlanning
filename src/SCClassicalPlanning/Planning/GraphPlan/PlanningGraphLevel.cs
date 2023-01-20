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
    public class PlanningGraphLevel
    {
        internal PlanningGraphLevel(
            PlanningGraph graph,
            int index,
            IReadOnlyDictionary<Literal, PlanningGraphPropositionNode> nodesByProposition,
            PlanningGraphLevel? previousLevel)
        {
            Graph = graph;
            Index = index;
            NodesByProposition = nodesByProposition;
            PreviousLevel = previousLevel;
        }

        /// <summary>
        /// Gets the planning graph in which this level resides.
        /// </summary>
        public PlanningGraph Graph { get; }

        /// <summary>
        /// Gets the previous level of the graph - or null if this level represents the initial state of the problem.
        /// </summary>
        public PlanningGraphLevel? PreviousLevel { get; }

        /// <summary>
        /// Gets the index of this level - with node index 0 indicating the propositions of the initial state of the problem.
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// Gets all of the planning graph nodes in this level, keyed by their respective propositions.
        /// </summary>
        public IReadOnlyDictionary<Literal, PlanningGraphPropositionNode> NodesByProposition { get; }

        /// <summary>
        /// Gets an enumerable of the propositions in this level.
        /// </summary>
        public IEnumerable<Literal> Propositions => NodesByProposition.Keys;

        /// <summary>
        /// Gets an enumerable of the planning graph nodes in this level.
        /// </summary>
        public IEnumerable<PlanningGraphPropositionNode> Nodes => NodesByProposition.Values;

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
