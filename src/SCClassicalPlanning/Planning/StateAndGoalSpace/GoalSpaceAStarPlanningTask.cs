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
using SCGraphTheory.Search.Classic;

namespace SCClassicalPlanning.Planning.StateAndGoalSpace;

/// <summary>
/// A concrete subclass of <see cref="SteppablePlanningTask{TStepResult}"/> that carries out
/// an A-star search of a problem's goal space to create a plan.
/// </summary>
public class GoalSpaceAStarPlanningTask : SteppablePlanningTask<GoalSpaceEdge>
{
    private readonly AStarAsyncSearch<GoalSpaceNode, GoalSpaceEdge> search;

    private bool isComplete;
    private Plan? result;

    private GoalSpaceAStarPlanningTask(AStarAsyncSearch<GoalSpaceNode, GoalSpaceEdge> search)
    {
        this.search = search;
        CheckForSearchCompletion();
    }

    /// <inheritdoc />
    public override bool IsComplete => isComplete;

    /// <inheritdoc />
    public override bool IsSucceeded => result != null;

    /// <inheritdoc />
    public override Plan Result
    {
        get
        {
            if (!IsComplete)
            {
                throw new InvalidOperationException("Task is not yet complete");
            }
            else if (result == null)
            {
                throw new InvalidOperationException("Plan creation failed");
            }
            else
            {
                return result;
            }
        }
    }

    /// <summary>
    /// Creates a new instance of the <see cref="GoalSpaceAStarPlanningTask"/> class.
    /// </summary>
    /// <param name="problem">The problem to solve.</param>
    /// <param name="costStrategy">The cost strategy to use.</param>
    /// <param name="invariantInspector"></param>
    public static async Task<GoalSpaceAStarPlanningTask> CreateAsync(Problem problem, ICostStrategy costStrategy, InvariantInspector invariantInspector)
    {
        return new(await AStarAsyncSearch<GoalSpaceNode, GoalSpaceEdge>.CreateAsync(
            source: new GoalSpaceNode(Tuple.Create(problem, invariantInspector), problem.EndGoal),
            isTarget: n => problem.InitialState.Meets(n.Goal),
            getEdgeCost: e => costStrategy.GetCost(e.Action),
            getEstimatedCostToTarget: n => costStrategy.EstimateCost(problem.InitialState, n.Goal)));
    }

    /// <inheritdoc />
    public override async Task<GoalSpaceEdge> NextStepAsync(CancellationToken cancellationToken = default)
    {
        if (IsComplete)
        {
            throw new InvalidOperationException("Task is already complete");
        }

        var edge = await search.NextStepAsync(cancellationToken);
        CheckForSearchCompletion();
        return edge;
    }

    /// <inheritdoc />
    public override void Dispose()
    {
        // Nothing to do
        GC.SuppressFinalize(this);
    }

    private void CheckForSearchCompletion()
    {
        if (search.IsConcluded)
        {
            if (search.IsSucceeded)
            {
                result = new Plan(search.PathToTarget().Reverse().Select(e => e.Action).ToList());
            }

            isComplete = true;
        }
    }
}
