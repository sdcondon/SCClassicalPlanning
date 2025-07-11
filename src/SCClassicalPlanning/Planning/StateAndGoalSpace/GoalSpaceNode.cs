// Copyright 2022-2024 Simon Condon
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
using SCGraphTheory;

namespace SCClassicalPlanning.Planning.StateAndGoalSpace;

/// <summary>
/// Represents a node in the goal space of a planning problem.
/// The outbound edges of the node represent relevant actions.
/// </summary>
public readonly struct GoalSpaceNode : IAsyncNode<GoalSpaceNode, GoalSpaceEdge>, IEquatable<GoalSpaceNode>
{
    private readonly Tuple<Problem, InvariantInspector> problemAndInvariants;

    public GoalSpaceNode(Tuple<Problem, InvariantInspector> problemAndInvariants, Goal goal) => (this.problemAndInvariants, Goal) = (problemAndInvariants, goal);

    /// <summary>
    /// Gets the goal represented by this node.
    /// </summary>
    public Goal Goal { get; }

    /// <inheritdoc />
    public IAsyncEnumerable<GoalSpaceEdge> Edges => new GoalSpaceNodeEdges(problemAndInvariants, Goal);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is GoalSpaceNode node && Equals(node);

    /// <inheritdoc />
    // NB: we don't compare the problem, since in expected usage (i.e. searching a particular
    // state space) it'll always match, so would be a needless drag on performance.
    public bool Equals(GoalSpaceNode node) => Equals(Goal, node.Goal);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Goal);

    /// <inheritdoc />
    public override string ToString() => Goal.ToString();
}
