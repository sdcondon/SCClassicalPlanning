﻿// Copyright 2022-2024 Simon Condon
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

namespace SCClassicalPlanning.Planning.StateAndGoalSpace;

/// <summary>
/// Represents the collection of outbound edges of a node in the goal space of a planning problem. 
/// </summary>
public readonly struct GoalSpaceNodeEdges : IReadOnlyCollection<GoalSpaceEdge>
{
    private readonly Tuple<Problem, InvariantInspector> problemAndInvariants;
    private readonly Goal goal;

    public GoalSpaceNodeEdges(Tuple<Problem, InvariantInspector> problemAndInvariants, Goal goal) => (this.problemAndInvariants, this.goal) = (problemAndInvariants, goal);

    /// <inheritdoc />
    public int Count => ProblemInspector.GetRelevantLiftedActions(goal, problemAndInvariants.Item1.ActionSchemas).Count();

    /// <inheritdoc />
    public IEnumerator<GoalSpaceEdge> GetEnumerator()
    {
        if (problemAndInvariants.Item2 != null)
        {
            foreach (var action in ProblemInspector.GetRelevantLiftedActions(goal, problemAndInvariants.Item1.ActionSchemas))
            {
                var effectiveAction = action;

                var nonTrivialPreconditions = problemAndInvariants.Item2.RemoveTrivialElements(action.Precondition);
                if (nonTrivialPreconditions != action.Precondition)
                {
                    effectiveAction = new(action.Identifier, nonTrivialPreconditions, action.Effect);
                }

                if (!problemAndInvariants.Item2.IsGoalPrecludedByInvariants(effectiveAction.Regress(goal)))
                {
                    yield return new GoalSpaceEdge(problemAndInvariants, goal, effectiveAction);
                }
            }
        }
        else
        {
            foreach (var action in ProblemInspector.GetRelevantLiftedActions(goal, problemAndInvariants.Item1.ActionSchemas))
            {
                yield return new GoalSpaceEdge(problemAndInvariants, goal, action);
            }
        }
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
