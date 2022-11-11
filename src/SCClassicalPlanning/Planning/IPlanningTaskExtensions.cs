using System.Runtime.CompilerServices;

namespace SCClassicalPlanning.Planning
{
    /// <summary>
    /// Helpful extension methods for <see cref="IPlanningTask"/> instances.
    /// </summary>
    public static class IPlanningTaskExtensions
    {
        /// <summary>
        /// Gets an awaiter used to await a given <see cref="IPlanningTask"/>
        /// </summary>
        /// <param name="planningTask">The planning task to get an awaiter for.</param>
        /// <returns>An awaiter instance.</returns>
        public static TaskAwaiter<Plan> GetAwaiter(this IPlanningTask planningTask)
        {
            return planningTask.ExecuteAsync().GetAwaiter();

        }

        /// <summary>
        /// Executes a planning task to completion.
        /// </summary>
        /// <param name="planningTask">The planning task to execute.</param>
        /// <returns>The result of the planning task.</returns>
        public static Plan Execute(this IPlanningTask planningTask)
        {
            return planningTask.ExecuteAsync().GetAwaiter().GetResult();
        }
    }
}
