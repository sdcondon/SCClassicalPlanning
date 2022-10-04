namespace SCClassicalPlanning
{
    /// <summary>
    /// Container for information about an action.
    /// <para/>
    /// Actions can be applied to <see cref="State"/>s to create new states (via the action's Effect),
    /// provided that the action's Precondition (which is a <see cref="Goal"/>) is met. <see cref="Domain"/>s include a description of all
    /// actions that are valid in the domain.
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
        /// This must be satisfied by a <see cref="State"/> for an action to be applicable in that state.
        /// </summary>
        public Goal Precondition { get; }

        /// <summary>
        /// Gets the effect of the action.
        /// This is what is applied to a state when the action is applied.
        /// </summary>
        public Effect Effect { get; }

        /// <summary>
        /// Gets a value indicating whether the action is applicable in a given state.
        /// <para/>
        /// An action is applicable in a state if its <see cref="Precondition"/> is satisfied by that state.
        /// </summary>
        /// <param name="state">The state to examine.</param>
        /// <returns>A value indicating whether the action is applicable in a given state.</returns>
        public bool IsApplicableTo(State state) => state.Satisfies(Precondition);

        /// <summary>
        /// Applies this action to a given state, producing a new state.
        /// <para/>
        /// TODO-NEEDED?: NB: Does NOT validate preconditions - to be of use with particular planning heuristics.
        /// </summary>
        /// <param name="state">The state to apply the action to.</param>
        /// <returns>The new state.</returns>
        public State ApplyTo(State state) => state.Apply(Effect);

        /// <summary>
        /// Returns a value indicating whether this action is conceivably a useful final step in achieving a given goal.
        /// <para/>
        /// An action is relevant to a goal if its effect is relevant to the goal.
        /// </summary>
        /// <param name="goal"></param>
        /// <returns></returns>
        public bool IsRelevantTo(Goal goal) => Effect.IsRelevantTo(goal);

        /// <summary>
        /// Returns the goal that must be satisfied prior to performing this action, in order to satisfy a given goal after the action is performed. 
        /// <para/>
        /// NB: AIaMA gets regression a bit.. err.. wrong, because it gets a little confused between states (which, under the
        /// closed-world assumption, need include only positive fluents) and goals (which can include both positive and negative fluents).
        /// It treats add lists and delete lists fundamentally differently, but shouldn't.
        /// <para/>
        /// A sound way to reason about regression is to first note that it operates on a goal to give a goal - that is, both positive and negative fluents appear can
        /// occur in both the argument and the result. Then note that any element of the given goal (positive or negative) that is applied by the action doesn't
        /// have to hold in the returned goal (because it'll be applied by the action) - so is removed. However, all preconditions do have to hold - so they are added.
        /// </summary>
        /// <param name="goal">The goal that must be satisfied after performing this action.</param>
        /// <returns>The goal that must be satisfied prior to performing this action.</returns>
        public Goal Regress(Goal goal) => new Goal(goal.Elements.Except(Effect.Elements).Union(Precondition.Elements));
    }
}
