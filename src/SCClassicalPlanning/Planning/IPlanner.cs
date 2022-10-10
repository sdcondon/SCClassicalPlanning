namespace SCClassicalPlanning.Planning
{
    /// <summary>
    /// Interface for types that can create <see cref="Plan"/>s for given <see cref="Problem"/>s.
    /// </summary>
    public interface IPlanner
    {
        /// <summary>
        /// Creates a plan to solve a given problem.
        /// </summary>
        /// <param name="problem">The problem to create a plan for.</param>
        /// <returns>A <see cref="Task"/> representing the process of creating a plan that solves the problem.</returns>
        Task<Plan> CreatePlanAsync(Problem problem, CancellationToken cancellationToken = default);
    }
}
