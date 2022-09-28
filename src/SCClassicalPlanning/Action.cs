﻿using SCFirstOrderLogic;

namespace SCClassicalPlanning
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class Action
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Action"/> class. 
        /// </summary>
        /// <param name="identifier">The identifier for this action.</param>
        /// <param name="precondition">The preconditional state for the action.</param>
        /// <param name="effect">The effect of the action.</param>
        public Action(object identifier, State precondition, State effect) => (Identifier, Precondition, Effect) = (identifier, precondition, effect);

        /// <summary>
        /// Gets the identifier for the action.
        /// </summary>
        public object Identifier { get; }

        /// <summary>
        /// Gets the preconditional state for the action.
        /// This elements of this state must be a subset of the current state in order for the action to be applicable.
        /// </summary>
        public State Precondition { get; }

        /// <summary>
        /// Gets the effect of the action. All unmentioned predicates are assumed to be unchanged.
        /// <para/>
        /// NB: In real PDDL, effects are their own type, not the same type as is used to represent goals and preconditions.
        /// The reason being that said types include more than just elements, and these extra members differ.
        /// Here though, we don't have any of those extras, and using a different type is more complex than is worth it.
        /// </summary>
        public State Effect { get; }

        /// TODI: variables - lazily, via visitor

        /// <summary>
        /// Gets the "add list" of the action - the non-negated atoms in the action's effect.
        /// </summary>
        public IEnumerable<Predicate> AddList => Effect.Elements.Where(a => !a.IsNegated).Select(l => l.Predicate);

        /// <summary>
        /// Gets the "delete list" of the action - the negated atoms in the action's effect.
        /// </summary>
        public IEnumerable<Predicate> DeleteList => Effect.Elements.Where(a => a.IsNegated).Select(l => l.Predicate);

        /// <summary>
        /// Gets a value indicating whether the action is applicable in a given state.
        /// </summary>
        /// <param name="state">The state to examine.</param>
        /// <returns>A value indicating whether the action is applicable in a given state.</returns>
        public bool IsApplicableTo(State state) => Precondition.Elements.IsSubsetOf(state.Elements);

        /// <summary>
        /// Applies this action to a given state, producing a new state.
        /// <para/>
        /// NB: Does not validate preconditions to be of use with particular heuristics.
        /// </summary>
        /// <param name="state">The state to apply the action to.</param>
        /// <returns>The new state.</returns>
        public State ApplyTo(State state) => new State(state.Elements.Except(DeleteList).Union(AddList));

        /// <summary>
        /// Gets a value indicate whether a given state could be the result of applying this action.
        /// </summary>
        /// <param name="state">The state to examine.</param>
        /// <returns>A value indicate whether a given state could be the result of applying this action.</returns>
        public bool IsRegressableFrom(State state) => throw new NotImplementedException();

        /// <summary>
        /// Regresses from a given state over the action.
        /// That is, gives the minimal state that would have existed prior to performing the action, if the given state was the result.
        /// </summary>
        /// <param name="state">The state to regress from.</param>
        /// <returns>The minimal state that would have existed prior to performing the action, if the given state was the result.</returns>
        public State Regress(State state) => new State(state.Elements.Except(AddList).Union(Precondition.Elements));
    }
}
