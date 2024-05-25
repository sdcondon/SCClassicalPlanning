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
using SCFirstOrderLogic;
using SCFirstOrderLogic.SentenceManipulation;

namespace SCClassicalPlanning;

/// <summary>
/// <para>
/// Interface for containers of information about a state.
/// </para>
/// <para>
/// A state is essentially just a (potentially large) set of ground (i.e. variable-free) <see cref="Predicate"/>s.
/// State instances occur as the initial state of <see cref="Problem"/> instances - and are also used by some planning
/// algorithms to track intermediate states while looking for a solution to a problem.
/// </para>
/// </summary>
public interface IState
{
    /// <summary>
    /// Gets the set of predicates that comprise this state.
    /// </summary>
    IQueryable<Predicate> Elements { get; }

    /// <summary>
    /// Applies a given <see cref="Effect"/> to the state, producing a new state.
    /// </summary>
    /// <param name="effect">The effect to apply.</param>
    /// <returns>The new state.</returns>
    IState Apply(Effect effect);

    /// <summary>
    /// <para>
    /// Gets a value indicating whether this state satisfies a given goal.
    /// </para>
    /// <para>
    /// A goal is satisfied by a state if all of its positive elements and none of its negative elements are present in the state.
    /// NB: This methods checks only if the goal is satisfied by the state exactly - meaning that it'll never return true if the goal
    /// has variables in it. See <see cref="GetSatisfyingSubstitutions(Goal)"/> for an alternative that allows for non-ground goals.
    /// </para>
    /// </summary>
    /// <param name="goal">The goal to check.</param>
    /// <returns>A value indicating whether this state satisfies a given goal.</returns>
    // TODO: if we're keeping Elements, not necessarily needed, because can do this with queries? Could add a default implementation, at least?
    bool Satisfies(Goal goal);

    /// <summary>
    /// Gets the substitutions (if any) that can be applied to a given goal so that this state satisfies it.
    /// </summary>
    /// <param name="goal">The goal to check.</param>
    /// <returns>An enumerable of substitutions that satisfy the goal.</returns>
    // TODO: could also be done with queries..
    IEnumerable<VariableSubstitution> GetSatisfyingSubstitutions(Goal goal);
}
