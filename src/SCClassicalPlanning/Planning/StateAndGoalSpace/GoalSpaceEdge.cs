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
/// Represents an edge in the goal space of a planning problem.
/// </summary>
// NB: three ref-valued fields puts this on the verge of being too large for a struct.
// Probably worth comparing performance with a class-based graph at some point, but meh, it'll do for now.
public readonly struct GoalSpaceEdge : IAsyncEdge<GoalSpaceNode, GoalSpaceEdge>
{
    private readonly Tuple<Problem, InvariantInspector> problemAndInvariants;
    private readonly Goal fromGoal;

    public GoalSpaceEdge(Tuple<Problem, InvariantInspector> problemAndInvariants, Goal fromGoal, Action action)
    {
        this.problemAndInvariants = problemAndInvariants;
        this.fromGoal = fromGoal;
        this.Action = action;
    }

    /// <inheritdoc />
    public GoalSpaceNode From => new(problemAndInvariants, fromGoal);

    /// <inheritdoc />
    public GoalSpaceNode To => new(problemAndInvariants, Action.Regress(fromGoal));

    /// <summary>
    /// Gets the action that is regressed over to achieve this goal transition.
    /// </summary>
    public Action Action { get; }

    /// <inheritdoc />
    public override string ToString() => new PlanFormatter(problemAndInvariants.Item1).Format(Action);


}
