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
using SCClassicalPlanning.Planning.Utilities;
using System.Collections;

namespace SCClassicalPlanning.Planning.StateAndGoalSpace
{
    /// <summary>
    /// Represents the collection of outbound edges of a node in the state space of a planning problem. 
    /// </summary>
    public readonly struct StateSpaceNodeEdges : IReadOnlyCollection<StateSpaceEdge>
    {
        /// <summary>
        /// The problem whose state space the node that owns this collection is a member of.
        /// </summary>
        public readonly Problem Problem;

        /// <summary>
        /// The state represented by the node that owns this collection.
        /// </summary>
        public readonly State State;

        /// <summary>
        /// Initialises a new instance of the <see cref="StateSpaceNodeEdges"/> struct.
        /// </summary>
        /// <param name="problem">The problem whose state space the node that owns this collection is a member of.</param>
        /// <param name="state">The state represented by the node that owns this collection.</param>
        public StateSpaceNodeEdges(Problem problem, State state) => (Problem, State) = (problem, state);

        /// <inheritdoc />
        public int Count => ProblemInspector.GetApplicableActions(Problem, State).Count();

        /// <inheritdoc />
        public IEnumerator<StateSpaceEdge> GetEnumerator()
        {
            foreach (var action in ProblemInspector.GetApplicableActions(Problem, State))
            {
                yield return new StateSpaceEdge(Problem, State, action);
            }
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
