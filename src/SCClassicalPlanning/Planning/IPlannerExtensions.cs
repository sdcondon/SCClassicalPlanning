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
namespace SCClassicalPlanning.Planning
{
    /// <summary>
    /// Useful extension methods for <see cref="IPlanner"/> implementations.
    /// </summary>
    public static class IPlannerExtensions
    {
        /// <summary>
        /// Creates a plan to solve a given problem.
        /// </summary>
        /// <param name="planner">The planner to use to create the plan.</param>
        /// <param name="problem">The problem to solve.</param>
        /// <param name="cancellationToken">A cancellation token for the operation. Optional, the default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A plan to solve the problem.</returns>
        public static Task<Plan> CreatePlanAsync(this IPlanner planner, Problem problem, CancellationToken cancellationToken = default)
        {
            return planner.CreatePlanningTask(problem).ExecuteAsync(cancellationToken);

            //throw new ArgumentException("Problem is unsolvable", nameof(problem)); ...?
        }

        /// <summary>
        /// Creates a plan to solve a given problem.
        /// </summary>
        /// <param name="planner">The planner to use to create the plan.</param>
        /// <param name="problem">The problem to solve.</param>
        /// <param name="cancellationToken">A cancellation token for the operation. Optional, the default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A plan to solve the problem.</returns>
        public static Plan CreatePlan(this IPlanner planner, Problem problem, CancellationToken cancellationToken = default)
        {
            return planner.CreatePlanningTask(problem).ExecuteAsync(cancellationToken).GetAwaiter().GetResult();
        }
    }
}
