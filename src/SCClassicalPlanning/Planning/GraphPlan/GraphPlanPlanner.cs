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
namespace SCClassicalPlanning.Planning.GraphPlan;

/// <summary>
/// An implementation of <see cref="IPlanner"/> that uses <see cref="GraphPlanPlanningTask"/> instances.
/// </summary>
internal class GraphPlanPlanner : IPlanner
{
    /// <summary>
    /// Creates a (specifically-typed) planning task to work on solving a given problem.
    /// </summary>
    /// <param name="problem">The problem to create a plan for.</param>
    /// <returns>A new <see cref="GraphPlanPlanningTask"/> for solving the problem.</returns>
    public static Task<GraphPlanPlanningTask> CreatePlanningTaskAsync(Problem problem) => Task.FromResult(new GraphPlanPlanningTask(problem));

    /// <inheritdoc />
    async Task<IPlanningTask> IPlanner.CreatePlanningTaskAsync(Problem problem) => await CreatePlanningTaskAsync(problem);
}
