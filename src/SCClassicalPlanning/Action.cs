namespace SCClassicalPlanning
{
    /// <summary>
    /// Container for information about an action that can be carried out within a domain.
    /// </summary>
    public sealed class Action
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
        /// This elements of this state must be a subset of the current state in order for the action to be applicable.
        /// </summary>
        public Goal Precondition { get; }

        /// <summary>
        /// Gets the effect of the action. All unmentioned predicates are assumed to be unchanged.
        /// </summary>
        public Effect Effect { get; }

        /// TODO: variables - lazily, via visitor

        /// <summary>
        /// Gets a value indicating whether the action is applicable in a given state.
        /// </summary>
        /// <param name="state">The state to examine.</param>
        /// <returns>A value indicating whether the action is applicable in a given state.</returns>
        public bool IsApplicableTo(State state) => Precondition.IsSatisfiedBy(state);

        /// <summary>
        /// Applies this action to a given state, producing a new state.
        /// <para/>
        /// NB: Does NOT validate preconditions - to be of use with particular heuristics.
        /// </summary>
        /// <param name="state">The state to apply the action to.</param>
        /// <returns>The new state.</returns>
        public State ApplyTo(State state) => Effect.ApplyTo(state);

        /// <summary>
        /// Returns a value indicating whether this action is conceivably a useful final step in achieving a given goal.
        /// <para/>
        /// An action is relevant if it accomplishes at least one element of the goal, and does not undo anything.
        /// That is, the effect's elements overlap with the goals elements, and the negation of each of the effect's elements does not.
        /// </summary>
        /// <param name="goal"></param>
        /// <returns></returns>
        public bool IsRelevantTo(Goal goal)
        {
            return goal.Elements.Overlaps(Effect.Elements) && !goal.Elements.Overlaps(Effect.Elements.Select(l => l.Negate()));
        }

        /// <summary>
        /// Returns the goal that must be satisfied prior to performing this action, in order to satisfy a given goal after the action is performed. 
        /// <para/>
        /// NB: AIMA gets regression a bit.. err.. wrong, because it gets a little confused between states (which, under the
        /// closed-world assumption, need include only positive fluents) and goals (which can include both positive and negative fluents).
        /// It makes a distinction between add lists and delete lists. The flaw in the argument about how delete lists can be ignored can be seen via a moments
        /// thought about how nothing *should* fundamentally change if you restate a problem so that every fluent is negated (e.g. instead of "IsThing", we use "IsNotThing") -
        /// but would because doing so would swap add lists and delete lists.
        /// A sound way to reason about regression is that any element of the given goal (positive OR negative) that is applied by the action doesn't have to hold 
        /// in the regressed goal (because it'll be applied by the action) - so is removed. However, all preconditions do have to hold - so they are added.
        /// </summary>
        /// <param name="goal">The goal that must be satisfied after performing this action.</param>
        /// <returns>The goal that must be satisfied prior to performing this action.</returns>
        public Goal Regress(Goal goal) => new Goal(goal.Elements.Except(Effect.Elements).Union(Precondition.Elements));
    }
}
