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
/// Abstract base class for <see cref="IPlanningTask"/>s that handles all of the properties itself,
/// leaving derived classes just needing to provide implementations for two template methods
/// (<see cref="ExecuteAsyncCore(CancellationToken)"/> and <see cref="Dispose"/>).
/// </summary>
public abstract class TemplatePlanningTask : IPlanningTask
{
    private int executeCount = 0;
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

        return execution = ExecuteAsyncCore(cancellationToken);
    }

    /// <summary>
    /// When implemented in a derived class, contains the core execution logic for the task.
    /// The <see cref="TemplatePlanningTask"/> class delegates to the returned task to provide
    /// the values of the various <see cref="IPlanningTask"/> properties.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to respect.</param>
    /// <returns>A task representing the planning process.</returns>
    protected abstract Task<Plan> ExecuteAsyncCore(CancellationToken cancellationToken = default);
}
