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
using SCClassicalPlanning.ProblemManipulation;
using SCFirstOrderLogic;
using System.Collections.Immutable;

namespace SCClassicalPlanning
{
    /// <summary>
    /// Encapsulates a planning problem.
    /// <para/>
    /// Problems exist within a <see cref="SCClassicalPlanning.Domain"/>, and consist of an initial <see cref="State"/>, an end <see cref="SCClassicalPlanning.Goal"/>,
    /// and a set of domain elements (represented by <see cref="Constant"/>s from the SCFirstOrderLogic library) that exist within the scope of the problem.
    /// </summary>
    public class Problem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Problem"/> class, in which the objects that exist are inferred from the constants that are present in the initial state and the goal.
        /// </summary>
        /// <param name="domain">The domain in which this problem resides.</param>
        /// <param name="initialState">The initial state of the problem.</param>
        /// <param name="goal">The goal of the problem.</param>
        public Problem(Domain domain, State initialState, Goal goal)
            : this(domain, initialState, goal, Array.Empty<Constant>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Problem"/> class.
        /// </summary>
        /// <param name="domain">The domain in which this problem resides.</param>
        /// <param name="initialState">The initial state of the problem.</param>
        /// <param name="goal">The goal of the problem.</param>
        /// <param name="additionalConstants">
        /// Any constants that exist in the problem other than those defined by the domain, initial state and goal.
        /// Yes, this is unusual - but we shouldn't preclude objects that serve as 'catalysts' (so don't feature
        /// in initial state or goal) and are used in actions that place no pre-conditions on them. (Very - most action params
        /// will at least have a 'type' constraint placed on them) unlikely but possible. Algorithms that can deal with
        /// variables don't need to know about such objects, but e.g. GraphPlan needs to know.
        /// </param>
        // TODO-EXTENSIBILITY: Problematic.. Large state? IO? Fairly big deal because could have significant impact.
        // Would just making State abstract or interface suffice?
        // Or would we want state and state storage separately?
        // Explore this. Later (prob last thing before v1 - want a 'complete' solution before looking at refactoring and abstractions).
        public Problem(Domain domain, State initialState, Goal goal, IEnumerable<Constant> additionalConstants)
        {
            Domain = domain;
            InitialState = initialState;
            Goal = goal;

            var constants = new HashSet<Constant>(additionalConstants);
            StateConstantFinder.Instance.Visit(initialState, constants);
            GoalConstantFinder.Instance.Visit(goal, constants);
            Constants = constants.ToImmutableHashSet();
        }

        /// <summary>
        /// Gets the domain in which this problem resides.
        /// </summary>
        public Domain Domain { get; }

        /// <summary>
        /// Gets the objects that exist in the problem.
        /// </summary>
        public ImmutableHashSet<Constant> Constants { get; }

        /// <summary>
        /// Gets the initial state of the problem.
        /// </summary>
        public State InitialState { get; }

        /// <summary>
        /// Gets the goal of the problem.
        /// </summary>
        public Goal Goal { get; }

        /// <summary>
        /// Utility class to find <see cref="Constant"/> instances within the elements of a <see cref="State"/>, and add them to a given <see cref="HashSet{T}"/>.
        /// </summary>
        private class StateConstantFinder : RecursiveStateVisitor<HashSet<Constant>>
        {
            /// <summary>
            /// Gets a singleton instance of the <see cref="StateConstantFinder"/> class.
            /// </summary>
            public static StateConstantFinder Instance { get; } = new();

            /// <inheritdoc/>
            public override void Visit(Constant constant, HashSet<Constant> constants) => constants.Add(constant);
        }

        /// <summary>
        /// Utility class to find <see cref="Constant"/> instances within the elements of a <see cref="SCClassicalPlanning.Goal"/>, and add them to a given <see cref="HashSet{T}"/>.
        /// </summary>
        private class GoalConstantFinder : RecursiveGoalVisitor<HashSet<Constant>>
        {
            /// <summary>
            /// Gets a singleton instance of the <see cref="GoalConstantFinder"/> class.
            /// </summary>
            public static GoalConstantFinder Instance { get; } = new();

            /// <inheritdoc/>
            public override void Visit(Constant constant, HashSet<Constant> constants) => constants.Add(constant);
        }
    }
}
