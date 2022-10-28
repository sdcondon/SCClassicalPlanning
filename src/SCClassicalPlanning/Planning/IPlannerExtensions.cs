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
        public static Plan CreatePlan(this IPlanner planner, Problem problem, CancellationToken cancellationToken = default)
        {
            return planner.CreatePlanAsync(problem, cancellationToken).GetAwaiter().GetResult();
        }
    }
}
