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
using SCClassicalPlanning.Planning.GraphPlan;

namespace SCClassicalPlanning.Planning.StateAndGoalSpace.CostStrategies;

/// <summary>
/// <para>
/// Cost strategy that (gives all actions a cost of 1 and) uses a "max level" planning graph heuristic
/// to provide cost estimates.
/// </para>
/// <para>
/// To give an estimate, it first constructs a planning graph starting from the current state. 
/// The cost estimate is the sum of the level costs of all of the goal's elements.
/// </para>
/// </summary>
public class PlanningGraphLevelSum : ICostStrategy
{
    private readonly IQueryable<Action> actions;

    /// <summary>
    /// Initialises a new instance of the <see cref="PlanningGraphMaxLevel"/> class.
    /// </summary>
    /// <param name="actions">The available actions.</param>
    public PlanningGraphLevelSum(IQueryable<Action> actions) => this.actions = actions;

    /// <inheritdoc/>
    public float GetCost(Action action) => 1f;

    /// <inheritdoc/>
    public float EstimateCost(IState state, Goal goal)
    {
        var planningGraph = new PlanningGraph(new(state, goal, actions));

        return goal.Elements.Sum(e =>
        {
            var level = planningGraph.GetLevelCost(e);
            if (level != -1)
            {
                return level;
            }
            else
            {
                return float.PositiveInfinity;
            }
        });
    }
}
