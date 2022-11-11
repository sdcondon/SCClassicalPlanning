using System;
using System.Threading;
using System.Threading.Tasks;

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
        public abstract Plan Result { get; }

        /// <summary>
        /// Executes the next step of the planning task.
        /// <para/>
        /// Calling <see cref="NextStepAsync"/> on a completed planning task should result in an <see cref="InvalidOperationException"/>.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token for the operation.</param>
        /// <returns>A container for information on what happened during the step.</returns>
        // TODO: Should this use ValueTask? Investigate me. Yeah, high-perf isn't the point of this package,
        // but given that this is an abstraction, its constraining what people *could* achieve with it. So worth a look at least.
        public abstract TStepResult NextStep(CancellationToken cancellationToken = default);

        /// <inheritdoc />
        public async Task<Plan> ExecuteAsync(CancellationToken cancellationToken = default)
        {
            // TODO: decide on re-entry handling, if any
            while (!IsComplete)
            {
                NextStep(cancellationToken);
            }

            return Result;
        }

        /// <inheritdoc />
        public abstract void Dispose();
    }
} 
