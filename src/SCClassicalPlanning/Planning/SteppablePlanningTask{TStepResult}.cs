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
namespace SCClassicalPlanning.Planning
{
    /// <summary>
    /// Abstract base class for <see cref="IPlanningTask"/> implementations that are executable step-by-step.
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
        /// <para>
        /// Executes the next step of the planning task.
        /// </para>
        /// <para>
        /// Calling <see cref="NextStep"/> on a completed planning task should result in an <see cref="InvalidOperationException"/>.
        /// </para>
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
