// Copyright 2022 Simon Condon
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
using SCClassicalPlanning.Planning.Utilities;
using SCFirstOrderLogic.Inference;

namespace SCClassicalPlanning.Planning.Search.Strategies
{
    /// <summary>
    /// A decorator strategy that checks whether the goal violates any known invariants
    /// before invoking the inner strategy. If any invariants are violated, returns <see cref="float.PositiveInfinity"/>.
    /// Intended to be of use for early pruning of unreachable goals when backward searching.
    /// <para/>
    /// NB #1: This strategy isn't driven by any particular source material, but given that it's a fairly
    /// obvious idea, there could well be some terminology that I'm not using - I may rename/refactor it as and when.
    /// <para/>
    /// NB #2: Checking invariants obviously comes at a performance cost (though fact that goals consist only of unit
    /// clauses likely mitigates this quite a lot - because it means that the negation of the query we ask our KB it
    /// consists only of unit clauses).
    /// The question is whether the benefit it provides outweighs the cost. I do wonder if we can somehow check
    /// only the stuff that has changed.
    /// <para/>
    /// NB #3: Ultimately it should be possible to derive the invariants by examining the problem.
    /// The simplest example of this is if a predicate doesn't appear in any effects. If this is true, the
    /// the occurences of this predicate in the initial state must persist throughout the problem.
    /// Might research / play with this idea at some point.
    /// <para/>
    /// TODO: thinking about it, this might be better implemented as part of a strategy for determining
    /// relevant actions - happens earlier and means that we could e.g. not even consider goal elements that
    /// are entailed by the invariants (e.g. IsBlock(BlockA)) - as we don't need to worry about things that
    /// will always be true.
    /// </summary>
    public class GoalInvariantCheck : IStrategy
    {
        private readonly InvariantInspector invariantInspector;
        private readonly IStrategy innerStrategy;

        /// <summary>
        /// Initializes a new instance of the <see cref="GoalInvariantCheck"/>.
        /// </summary>
        /// <param name="invariantsKB">A knowledge base containing all of the invariants of the problem.</param>
        /// <param name="innerStrategy">The inner strategy to invoke if no invariants are violated by the goal.</param>
        public GoalInvariantCheck(IKnowledgeBase invariantsKB, IStrategy innerStrategy)
        {
            this.invariantInspector = new InvariantInspector(invariantsKB);
            this.innerStrategy = innerStrategy;
        }

        /// <inheritdoc/>
        public float GetCost(Action action) => innerStrategy.GetCost(action);

        /// <summary>
        /// Estimates the cost of getting from the given state to a state that satisfies the given goal.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <param name="goal">The goal.</param>
        /// <returns><see cref="float.PositiveInfinity"/> if any invariants are violated by the goal. Otherwise, the cost estimated by the inner strategy.</returns>
        public float EstimateCost(State state, Goal goal)
        {
            if (invariantInspector.IsGoalPrecludedByInvariants(goal))
            {
                return float.PositiveInfinity;
            }

            return innerStrategy.EstimateCost(state, goal);
        }
    }
}
