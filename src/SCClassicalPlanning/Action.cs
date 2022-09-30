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

        ////public bool IsRegressableFrom(Goal goal) => throw new NotImplementedException();

        ////public Goal Regress(Goal goal) => new State(goal.Elements.Except(Effect.AddList).Union(Precondition.Elements));
    }
}
