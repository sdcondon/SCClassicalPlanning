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
namespace SCClassicalPlanning.Planning;

/// <summary>
/// <para>
/// Interface for types that can create <see cref="Plan"/>s for given <see cref="Problem"/>s - 
/// via <see cref="IPlanningTask"/> instances, which allow for fine-grained and/or interrogable plan creation processes.
/// </para>
/// <para>
/// Essentially, defines an abstract factory for <see cref="IPlanningTask"/> instances. 
/// </para>
/// </summary>
public interface IPlanner
{
    /// <summary>
    /// Creates a planning task to work on solving a given problem.
    /// </summary>
    /// <param name="problem">The problem to create a plan for.</param>
    /// <returns>An <see cref="IPlanningTask"/> representing the process of creating a plan that solves the problem.</returns>
    Task<IPlanningTask> CreatePlanningTaskAsync(Problem problem);
}
