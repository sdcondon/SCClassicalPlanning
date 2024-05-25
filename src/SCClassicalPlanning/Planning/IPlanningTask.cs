// Copyright 2022-2023 Simon Condon
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
/// <para>
/// An interface for encapsulations of an attempt to create a <see cref="Plan"/>. 
/// </para>
/// <para>
/// We define our own interface (instead of, say, just using <see cref="Task{T}"/> of <see cref="Plan"/>) so that it is
/// easy for implementations to add additional behaviours such as step-by-step execution 
/// (see <see cref="SteppablePlanningTask{TStepResult}"/>) and result explanations.
/// </para>
/// </summary>
// TODO: Not 100% happy with the design, here. Can't help but think it should have Start() and a GetAwaiter() instead of ExecuteAsync
// (and the CancellationToken should be CreatePlanningTask's problem). In so doing, it'd echo the API of Tasks a little
// more, and by eliminating the cancellation token from here, we remove any possible awkwardness of ExecuteAsync being called
// multiple times with different CTs, making it simpler to handle multiple threads trying to interact with the task (even though,
// yeah, thats not a very likely scenario). Then again, Task.Start simply throws if called more than once. And using Start would
// perhaps be problematic (semantically) for tasks that are executable step-by-step.
// Maybe experiment at some point (obv a breaking change).
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
    /// Gets the created plan. Should throw an <see cref="InvalidOperationException"/> if the task is not yet complete or failed to create a plan.
    /// </summary>
    /// <exception cref="InvalidOperationException">The task is not yet complete or failed to create a plan.</exception>
    Plan Result { get; }

    /// <summary>
    /// Executes the task to completion.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token for the operation.</param>
    /// <returns>A task, the result of which is the plan.</returns>
    Task<Plan> ExecuteAsync(CancellationToken cancellationToken = default);
}
