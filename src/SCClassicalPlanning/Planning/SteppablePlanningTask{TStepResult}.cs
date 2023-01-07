using System.Threading;

namespace SCClassicalPlanning.Planning
{
    /// <summary>
    /// Base class for <see cref="IPlanningTask"/> implementations that are executable step-by-step.
    /// </summary>
    /// <typeparam name="TStepResult">The type of the result of each step. This type should be a container for information on what happened during the step.</typeparam>
    public abstract class SteppablePlanningTask<TStepResult> : IPlanningTask
    {
        /// <inheritdoc />
        public abstract bool IsComplete { get; }

        /// <inheritdoc />
        public abstract bool IsSucceeded { get; }

        /// <inheritdoc />
        public abstract Plan Result { get; }

        /// <summary>
        /// Executes the next step of the planning task.
        /// <para/>
        /// Calling <see cref="NextStep"/> on a completed planning task should result in an <see cref="InvalidOperationException"/>.
        /// </summary>
        /// <returns>A container for information about what happened during the step.</returns>
        public abstract TStepResult NextStep();

        /// <inheritdoc />
        public async Task<Plan> ExecuteAsync(CancellationToken cancellationToken = default)
        {
            // TODO: decide on re-entry handling, if any
            while (!IsComplete)
            {
                NextStep();
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Yield();
            }

            return Result;
        }

        /// <inheritdoc />
        public abstract void Dispose();
    }
} 
