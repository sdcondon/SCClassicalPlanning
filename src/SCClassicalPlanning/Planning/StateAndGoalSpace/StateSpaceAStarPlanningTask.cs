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
using SCGraphTheory.Search.Classic;

namespace SCClassicalPlanning.Planning.StateAndGoalSpace;

/// <summary>
/// A concrete subclass of <see cref="SteppablePlanningTask{TStepResult}"/> that carries out
/// an A-star search of a problem's state space to create a plan.
/// </summary>
public class StateSpaceAStarPlanningTask : SteppablePlanningTask<StateSpaceEdge>
{
    private readonly AStarSearch<StateSpaceNode, StateSpaceEdge> search;

    private bool isComplete;
    private Plan? result;

    /// <summary>
    /// Initializes a new instance of the <see cref="StateSpaceAStarPlanningTask"/> class.
    /// </summary>
    /// <param name="problem">The problem to solve.</param>
    /// <param name="costStrategy">The cost strategy to use.</param>
    public StateSpaceAStarPlanningTask(Problem problem, ICostStrategy costStrategy)
    {
        search = new AStarSearch<StateSpaceNode, StateSpaceEdge>(
            source: new StateSpaceNode(problem, problem.InitialState),
            isTarget: n => n.State.Meets(problem.EndGoal),
            getEdgeCost: e => costStrategy.GetCost(e.Action),
            getEstimatedCostToTarget: n => costStrategy.EstimateCost(n.State, problem.EndGoal));

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

    /// <inheritdoc />
    public override Task<StateSpaceEdge> NextStepAsync(CancellationToken cancellationToken = default)
    {
        if (IsComplete)
        {
            throw new InvalidOperationException("Task is already complete");
        }

        var edge = search.NextStep();
        CheckForSearchCompletion();
        return Task.FromResult(edge);
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
                result = new Plan(search.PathToTarget().Select(e => e.Action).ToList());
            }

            isComplete = true;
        }
    }
}
