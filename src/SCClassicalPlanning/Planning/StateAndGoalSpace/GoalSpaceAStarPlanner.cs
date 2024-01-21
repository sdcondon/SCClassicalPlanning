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
namespace SCClassicalPlanning.Planning.StateAndGoalSpace;

/// <summary>
/// An implementation of <see cref="IPlanner"/> that uses <see cref="GoalSpaceAStarPlanningTask"/> instances.
/// </summary>
public class GoalSpaceAStarPlanner : IPlanner
{
    private readonly ICostStrategy costStrategy;

    /// <summary>
    /// Initializes a new instance of the <see cref="GoalSpaceAStarPlanner"/> class.
    /// </summary>
    /// <param name="costStrategy">The cost strategy to use.</param>
    public GoalSpaceAStarPlanner(ICostStrategy costStrategy) => this.costStrategy = costStrategy;

    /// <summary>
    /// Creates a (specifically-typed) planning task to work on solving a given problem.
    /// </summary>
    /// <param name="problem">The problem to create a plan for.</param>
    /// <returns>A new <see cref="GoalSpaceAStarPlanningTask"/> instance.</returns>
    public GoalSpaceAStarPlanningTask CreatePlanningTask(Problem problem) => new(problem, costStrategy);

    /// <inheritdoc />
    IPlanningTask IPlanner.CreatePlanningTask(Problem problem) => CreatePlanningTask(problem);
}
