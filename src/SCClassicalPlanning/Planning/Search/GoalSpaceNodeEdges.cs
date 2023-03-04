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

namespace SCClassicalPlanning.Planning.Search
{
    /// <summary>
    /// Represents the collection of outbound edges of a node in the goal space of a planning problem. 
    /// </summary>
    public readonly struct GoalSpaceNodeEdges : IReadOnlyCollection<GoalSpaceEdge>
    {
        /// <summary>
        /// The problem whose goal space the node that owns this collection is a member of.
        /// </summary>
        public readonly Problem Problem;

        /// <summary>
        /// The goal represented by the node that owns this collection.
        /// </summary>
        public readonly Goal Goal;

        /// <summary>
        /// Initialises a new instance of the <see cref="GoalSpaceNodeEdges"/> struct.
        /// </summary>
        /// <param name="problem">The problem whose goal space the node that owns this collection is a member of.</param>
        /// <param name="goal">The goal represented by the node that owns this collection.</param>
        public GoalSpaceNodeEdges(Problem problem, Goal goal) => (Problem, Goal) = (problem, goal);

        /// <inheritdoc />
        public int Count => ProblemInspector.GetRelevantActions(Problem, Goal).Count();

        /// <inheritdoc />
        public IEnumerator<GoalSpaceEdge> GetEnumerator()
        {
            foreach (var action in ProblemInspector.GetRelevantActions(Problem, Goal))
            {
                yield return new GoalSpaceEdge(Problem, Goal, action);
            }
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
