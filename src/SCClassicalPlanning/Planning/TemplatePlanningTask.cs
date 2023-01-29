namespace SCClassicalPlanning.Planning
{
    /// <summary>
    /// Abstract base class for <see cref="IPlanningTask"/>s that handles all of the properties itself,
    /// leaving derived classes just needing to provide implementations for two template methods
    /// (<see cref="ExecuteAsyncCore(CancellationToken)"/> and <see cref="Dispose"/>).
    /// </summary>
    public abstract class TemplatePlanningTask : IPlanningTask
    {
        private Task<Plan>? execution;

        /// <inheritdoc />
        public bool IsComplete => execution?.IsCompleted ?? false;

        /// <inheritdoc />
        public bool IsSucceeded => execution?.IsCompletedSuccessfully ?? false && execution.Result != null;

        /// <inheritdoc />
        public Plan Result => execution?.GetAwaiter().GetResult() ?? throw new InvalidOperationException("Planning task not started, or returned a null plan");

        /// <inheritdoc />
        public abstract void Dispose();

        /// <inheritdoc />
        public Task<Plan> ExecuteAsync(CancellationToken cancellationToken)
        {
            return execution = ExecuteAsyncCore(cancellationToken);
        }

        /// <summary>
        /// When implemented in a derived class, contains the core execution logic for the task.
        /// The <see cref="TemplatePlanningTask"/> class delegates to the returned task to provide
        /// the values of the various <see cref="IPlanner"/> properties.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to respect.</param>
        /// <returns>A task representing the planning process.</returns>
        protected abstract Task<Plan> ExecuteAsyncCore(CancellationToken cancellationToken = default);
    }
}
