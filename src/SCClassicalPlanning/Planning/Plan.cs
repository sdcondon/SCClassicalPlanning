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
using System.Collections.Immutable;

namespace SCClassicalPlanning.Planning;

/// <summary>
/// Container for a (totally-ordered) plan of action - essentially just a list of steps.
/// </summary>
public class Plan
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Plan"/> class.
    /// </summary>
    /// <param name="steps">The actions that comprise the plan.</param>
    public Plan(IEnumerable<Action> steps) => Steps = steps.ToImmutableList();

    /// <summary>
    /// Gets a singleton instance of an empty plan;
    /// </summary>
    public static Plan Empty { get; } = new Plan(Array.Empty<Action>());

    /// <summary>
    /// Gets the steps of the plan.
    /// </summary>
    public ImmutableList<Action> Steps { get; }

    /// <summary>
    /// <para>
    /// Applies this plan to a given state.
    /// </para>
    /// <para>
    /// An exception will be thrown if any of the actions in the plan are not applicable to the current state when they are applied.
    /// </para>
    /// </summary>
    public IState ApplyTo(IState state)
    {
        foreach (var action in Steps)
        {
            if (!action.IsApplicableTo(state))
            {
                throw new ArgumentException("Invalid plan of action - current action is not applicable in the current state");
            }

            state = action.ApplyTo(state);
        }

        return state;
    }
}
