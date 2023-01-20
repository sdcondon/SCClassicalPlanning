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

namespace SCClassicalPlanning.Planning.Search
{
    /// <summary>
    /// <para>
    /// A simple implementation of <see cref="IPlanner"/> that carries out an A-star search of
    /// the state space to create plans.
    /// </para>
    /// <para>
    /// See section 10.2.2 of "Artificial Intelligence: A Modern Approach" for more on this.
    /// </para>
    /// </summary>
    public class StateSpaceSearch : IPlanner
    {
        private readonly ICostStrategy costStrategy;

        /// <summary>
        /// Initializes a new instance of the <see cref="StateSpaceSearch"/> class.
        /// </summary>
        /// <param name="costStrategy">The cost strategy to use.</param>
        public StateSpaceSearch(ICostStrategy costStrategy) => this.costStrategy = costStrategy;

        /// <summary>
        /// Creates a (concretely-typed) planning task to work on solving a given problem.
        /// </summary>
        /// <param name="problem">The problem to create a plan for.</param>
        /// <returns>A new <see cref="StateSpaceSearchPlanningTask"/> instance.</returns>
        public StateSpaceSearchPlanningTask CreatePlanningTask(Problem problem) => new(problem, costStrategy);

        /// <inheritdoc />
        IPlanningTask IPlanner.CreatePlanningTask(Problem problem) => CreatePlanningTask(problem);
    }
}
