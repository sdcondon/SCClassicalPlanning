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
    /// Represents a node in the state space of a planning problem.
    /// The outbound edges of the node represent applicable actions.
    /// </summary>
    public readonly struct StateSpaceNode : INode<StateSpaceNode, StateSpaceEdge>, IEquatable<StateSpaceNode>
    {
        /// <summary>
        /// The problem whose state space this node is a member of.
        /// </summary>
        public readonly Problem Problem;

        /// <summary>
        /// The state represented by this node.
        /// </summary>
        public readonly State State;

        /// <summary>
        /// Initialises a new instance of the <see cref="StateSpaceNode"/> struct.
        /// </summary>
        /// <param name="problem">The problem whose state space this node is a member of.</param>
        /// <param name="state">The state represented by this node.</param>
        public StateSpaceNode(Problem problem, State state) => (Problem, State) = (problem, state);

        /// <inheritdoc />
        public IReadOnlyCollection<StateSpaceEdge> Edges => new StateSpaceNodeEdges(Problem, State);

        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is StateSpaceNode node && Equals(node);

        /// <inheritdoc />
        // NB: we don't compare the problem, since in expected usage (i.e. searching a particular
        // state space) it'll always match, so would be a needless drag on performance.
        public bool Equals(StateSpaceNode node) => Equals(State, node.State);

        /// <summary>
        /// Computes a hash code for this struct.
        /// </summary>
        /// <returns>A hash code.</returns>
        public override int GetHashCode() => HashCode.Combine(State);
    }
}
