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
namespace SCClassicalPlanning.Planning.StateAndGoalSpace;

/// <summary>
/// <para>
/// Interface for state/goal-space search cost strategy implementations.
/// </para>
/// <para>
/// Implementations of this interface provide the "cost" (whatever that means in the context of the
/// problem being solved) of any given action, and can also estimate the total cost of getting from
/// a given state to a state that satisfies a given goal.
/// </para>
/// </summary>
public interface ICostStrategy
{
    /// <summary>
    /// Gets the cost of a given action.
    /// </summary>
    /// <param name="action">The action to get the cost of.</param>
    /// <returns>The cost of the given action.</returns>
    float GetCost(Action action);

    /// <summary>
    /// Estimates the total cost of getting from a given state to a state that satisfies a given goal.
    /// </summary>
    /// <param name="state">The start state.</param>
    /// <param name="goal">The goal to be satisfied.</param>
    /// <returns>The estimated cost.</returns>
    float EstimateCost(State state, Goal goal);
}
