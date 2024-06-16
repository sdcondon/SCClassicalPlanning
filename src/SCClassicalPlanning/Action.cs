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
namespace SCClassicalPlanning;

/// <summary>
/// <para>
/// Container for information about an action.
/// </para>
/// <para>
/// Actions can be applied to <see cref="IState"/>s to create new states (via the action's Effect),
/// provided that the action's Precondition (which is a <see cref="Goal"/>) is satisfied by the current state.
/// <see cref="Problem"/>s include a description of all available actions.
/// </para>
/// </summary>
public class Action
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Action"/> class. 
    /// </summary>
    /// <param name="identifier">The identifier for this action.</param>
    /// <param name="precondition">The precondition for the action.</param>
    /// <param name="effect">The effect of the action.</param>
    public Action(object identifier, Goal precondition, Effect effect) => (Identifier, Precondition, Effect) = (identifier, precondition, effect);

    /// <summary>
    /// Gets the identifier for the action.
    /// </summary>
    public object Identifier { get; }

    /// <summary>
    /// Gets the precondition for the action.
    /// This must be satisfied by a <see cref="IState"/> for an action to be applicable in that state.
    /// </summary>
    public Goal Precondition { get; }

    /// <summary>
    /// Gets the effect of the action.
    /// This is the change that is made to a state when the action is applied.
    /// </summary>
    public Effect Effect { get; }

    /// <summary>
    /// <para>
    /// Gets a value indicating whether the action is applicable in a given state.
    /// </para>
    /// <para>
    /// An action is applicable in a state if its <see cref="Precondition"/> is satisfied by that state.
    /// </para>
    /// </summary>
    /// <param name="state">The state to examine.</param>
    /// <returns>A value indicating whether the action is applicable in a given state.</returns>
    public bool IsApplicableTo(IState state) => state.Satisfies(Precondition);

    /// <summary>
    /// <para>
    /// Applies this action to a given state, producing a new state.
    /// </para>
    /// <para>
    /// NB: Does NOT validate preconditions - to be of use with particular planning heuristics.
    /// TODO: Maybe rethink this. Any heuristic that wants to do this could just look at the effect and apply it..
    /// </para>
    /// </summary>
    /// <param name="state">The state to apply the action to.</param>
    /// <returns>The new state.</returns>
    public IState ApplyTo(IState state) => state.Apply(Effect);

    /// <summary>
    /// <para>
    /// Returns a value indicating whether this action is conceivably a useful final step in achieving a given goal.
    /// </para>
    /// <para>
    /// An action is relevant to a goal if its effect is relevant to the goal.
    /// </para>
    /// </summary>
    /// <param name="goal">The goal to examine.</param>
    /// <returns>A value indicating whether this action is conceivably a useful final step in achieving the given goal.</returns>
    public bool IsRelevantTo(Goal goal) => goal.IsRelevant(Effect);

    /// <summary>
    /// Returns the goal that must be satisfied prior to performing this action, in order to ensure that a given goal is satisfied after the action is performed. 
    /// </summary>
    /// <param name="goal">The goal that must be satisfied after performing this action.</param>
    /// <returns>The goal that must be satisfied prior to performing this action.</returns>
    public Goal Regress(Goal goal) => goal.Regress(this);

    /// <summary>
    /// <para>
    /// Determines whether the specified object is equal to the current object.
    /// </para>
    /// <para>
    /// Actions implement value semantics for equality - two Actions are equal if their Identifiers, Preconditions and Effects are.
    /// </para>
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
    public override bool Equals(object? obj)
    {
        return obj is Action action 
            && action.Identifier.Equals(Identifier) 
            && action.Precondition.Equals(Precondition) 
            && action.Effect.Equals(Effect);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(Identifier, Precondition, Effect);
    }
}
