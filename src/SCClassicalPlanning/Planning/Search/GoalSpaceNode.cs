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
using SCGraphTheory;

namespace SCClassicalPlanning.Planning.Search
{
    /// <summary>
    /// Represents a node in the goal space of a planning problem.
    /// The outbound edges of the node represent relevant actions.
    /// </summary>
    public readonly struct GoalSpaceNode : INode<GoalSpaceNode, GoalSpaceEdge>, IEquatable<GoalSpaceNode>
    {
        /// <summary>
        /// The problem whose goal space this node is a member of.
        /// </summary>
        public readonly Problem Problem;

        /// <summary>
        /// The goal represented by this node.
        /// </summary>
        public readonly Goal Goal;

        /// <summary>
        /// Initialises a new instance of the <see cref="StateSpaceNode"/> struct.
        /// </summary>
        /// <param name="problem">The problem whose goal space this node is a member of.</param>
        /// <param name="goal">The goal represented by this node.</param>
        public GoalSpaceNode(Problem problem, Goal goal) => (Problem, Goal) = (problem, goal);

        /// <inheritdoc />
        public IReadOnlyCollection<GoalSpaceEdge> Edges => new GoalSpaceNodeEdges(Problem, Goal);

        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is GoalSpaceNode node && Equals(node);

        /// <inheritdoc />
        // NB: we don't compare the problem, since in expected usage (i.e. searching a particular
        // state space) it'll always match, so would be a needless drag on performance.
        public bool Equals(GoalSpaceNode node) => Equals(Goal, node.Goal);

        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(Goal);

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString() => Goal.ToString();
    }
}
