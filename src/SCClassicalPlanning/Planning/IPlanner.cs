namespace SCClassicalPlanning.Planning
{
    /// <summary>
    /// Interface for types that can create plans for given problems.
    /// </summary>
    public interface IPlanner
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="problem">The problem to create a plan for.</param>
        /// <returns></returns>
        Task<IPlan> CreatePlanAsync(Problem problem);
    }
}
