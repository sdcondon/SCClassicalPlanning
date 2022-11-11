namespace SCClassicalPlanning.Planning
{
    /// <summary>
    /// An interface for encapsulations of an attempt to create a <see cref="Plan"/>. 
    /// <para/>
    /// We define our own interface (instead of just using <see cref="Task{Plan}"/>) so that it is
    /// easy for implementations to add additional behaviours - such as step-by-step execution 
    /// (see <see cref="SteppablePlanningTask{TStepResult}"/>) - and result explanations. However, note the existence
    /// of the <see cref="IPlanningTaskExtensions.GetAwaiter(IPlanningTask)"/> extension method, so that instances
    /// can be awaited directly.
    /// </summary>
    public interface IPlanningTask : IDisposable
    {
        /// <summary>
        /// Gets a value indicating whether the planning task is complete.
        /// The result of completed queries is available via the <see cref="Result"/> property.
        /// </summary>
        bool IsComplete { get; }

        /// <summary>
        /// Gets a value indicating the result of the task.
        /// </summary>
        /// <exception cref="InvalidOperationException">The task is not yet complete.</exception>
        Plan Result { get; }

        /// <summary>
        /// Executes the task to completion.
        /// <para/>
        /// NB: This is an asynchronous method ultimately because "real" planners will often need to do IO to create plans.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token for the operation.</param>
        /// <returns>A task, the result of which is the plan.</returns>
        Task<Plan> ExecuteAsync(CancellationToken cancellationToken = default);
    }
}
