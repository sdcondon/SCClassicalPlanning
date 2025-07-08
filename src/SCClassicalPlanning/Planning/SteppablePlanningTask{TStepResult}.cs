// Copyright 2022-2024 Simon Condon
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
namespace SCClassicalPlanning.Planning;

/// <summary>
/// Abstract base class for <see cref="IPlanningTask"/> implementations that are executable step-by-step.
/// </summary>
/// <typeparam name="TStepResult">The type of the result of each step. This type should be a container for information on what happened during the step.</typeparam>
public abstract class SteppablePlanningTask<TStepResult> : IPlanningTask
{
    private int executeCount = 0;

    // TODO-BREAKING: No real point in making this or Result abstract - only one kind of
    // implementation makes much sense - e.g. IsComplete returns if result (nullable bool)
    // has a value. Result throws InvalidOperationEx if not complete. Add protected
    // SetSucceeded(Plan) & SetFailed() methods.
    /// <inheritdoc />
    public abstract bool IsComplete { get; }

    /// <inheritdoc />
    public abstract bool IsSucceeded { get; }

    /// <inheritdoc />
    public abstract Plan Result { get; }

    /// <summary>
    /// <para>
    /// Executes the next step of the planning task.
    /// </para>
    /// <para>
    /// Calling <see cref="NextStepAsync"/> on a completed planning task should result in an <see cref="InvalidOperationException"/>.
    /// </para>
    /// </summary>
    /// <returns>A container for information about what happened during the step.</returns>
    // NB: While this is a rather low-level method, and some scenarios/planners might not do anything async,
    // some cursory performance testing shows that using ValueTask here tends to (slightly reduce GC pressure, sure, but) slow things down a bit overall.
    public abstract Task<TStepResult> NextStepAsync(CancellationToken cancellationToken = default);

    /// <inheritdoc />
    public async Task<Plan> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        // ..while it might be nice to allow for other threads to just get the existing task back
        // if its already been started, the possibility of the cancellation token being different
        // makes it awkward. The complexity added by dealing with that simply isn't worth it.
        // (at a push could PERHAPS just throw if the CT is different - see CT equality remarks).
        // So, we just throw if the query is already in progress. Messing about with a query from
        // multiple threads is fairly unlikely anyway (as opposed to wanting an individual query to
        // parallelise itself - which is definitely something I want to look at).
        if (Interlocked.Exchange(ref executeCount, 1) == 1)
        {
            throw new InvalidOperationException("Planning task execution has already begun via a prior ExecuteAsync invocation");
        }

        while (!IsComplete)
        {
            await NextStepAsync(cancellationToken);
        }

        return Result;
    }

    /// <inheritdoc />
    public abstract void Dispose();
}

