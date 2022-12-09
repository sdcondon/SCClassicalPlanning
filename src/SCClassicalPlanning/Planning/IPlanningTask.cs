namespace SCClassicalPlanning.Planning
{
    /// <summary>
    /// An interface for encapsulations of an attempt to create a <see cref="Plan"/>. 
    /// <para/>
    /// We define our own interface (instead of just using <see cref="Task{Plan}"/>) so that it is
    /// easy for implementations to add additional behaviours such as step-by-step execution 
    /// (see <see cref="SteppablePlanningTask{TStepResult}"/>) and result explanations.
    /// </summary>
    public interface IPlanningTask : IDisposable
    {
        /// <summary>
        /// Gets a value indicating whether the planning task is complete - irrespective of whether a plan was successfully created or not.
        /// </summary>
        bool IsComplete { get; }

        /// <summary>
        /// Gets a value indicating whether the planning task is complete and managed to create a plan.
        /// The result of successful queries is available via the <see cref="Result"/> property.
        /// </summary>
        bool IsSucceeded { get; }

        /// <summary>
        /// Gets a value indicating the result of the task.
        /// </summary>
        /// <exception cref="InvalidOperationException">The task is not yet complete or failed to create a plan.</exception>
        Plan Result { get; }

        /// <summary>
        /// Executes the task to completion.
        /// <para/>
        /// NB: This is an asynchronous method ultimately because "real" planners will often need to do IO to create plans.
        /// (While not true at the time of writing, ultimately <see cref="State"/> access is likely to become async, to facilitate
        /// states that are large enough to warrant secondary storage - so anything using states would also benefit from being async).
        /// </summary>
        /// <param name="cancellationToken">A cancellation token for the operation.</param>
        /// <returns>A task, the result of which is the plan.</returns>
        Task<Plan> ExecuteAsync(CancellationToken cancellationToken = default);
    }
}
