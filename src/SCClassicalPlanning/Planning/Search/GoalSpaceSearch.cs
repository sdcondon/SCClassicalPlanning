// Copyright 2022 Simon Condon
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
    /// A simple implementation of <see cref="IPlanner"/> that carries out an A-star search of
    /// the goal space to create plans.
    /// </summary>
    public class GoalSpaceSearch : IPlanner
    {
        private readonly IStrategy strategy;

        /// <summary>
        /// Initializes a new instance of the <see cref="GoalSpaceSearch"/> class.
        /// </summary>
        /// <param name="strategy">The strategy to use.</param>
        public GoalSpaceSearch(IStrategy strategy) => this.strategy = strategy;

        /// <summary>
        /// Creates a (concretely-typed) planning task to work on solving a given problem.
        /// </summary>
        /// <param name="problem">The problem to create a plan for.</param>
        /// <returns>A new <see cref="GoalSpaceSearchPlanningTask"/> instance.</returns>
        public GoalSpaceSearchPlanningTask CreatePlanningTask(Problem problem) => new(problem, strategy);

        /// <inheritdoc />
        IPlanningTask IPlanner.CreatePlanningTask(Problem problem) => CreatePlanningTask(problem);
    }
}
