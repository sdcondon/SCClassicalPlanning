namespace SCClassicalPlanning
{
    public sealed class Action
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Action"/> class. 
        /// </summary>
        /// <param name="identifier"></param>
        /// <param name="precondition"></param>
        /// <param name="effect"></param>
        public Action(object identifier, State precondition, State effect) => (Identifier, Precondition, Effect) = (identifier, precondition, effect);

        /// <summary>
        /// Gets the unique identifier for the action.
        /// </summary>
        public object Identifier { get; }

        public State Precondition { get; }

        /// <summary>
        /// Gets the effect of the action. All unmentioneds predicates are assumed to be unchanged.
        /// <para/>
        /// NB: In real PDDL, effects are their own type, not the same type as is used to represent goals and preconditions.
        /// The reason being that said types include more than just elements, and these extra members differ.
        /// Here though, we don't have any of those extras, and using a different type is more complex than is worth it.
        /// </summary>
        public State Effect { get; }
        
        /// <summary>
        /// Gets the "add list" of the action - the non-negated atoms in the action's effect.
        /// </summary>
        public IEnumerable<Literal> AddList => Effect.Elements.Where(a => !a.IsNegated);

        /// <summary>
        /// Gets the "delete list" of the action - the negated atoms in the action's effect.
        /// </summary>
        public IEnumerable<Literal> DeleteList => Effect.Elements.Where(a => a.IsNegated);

        /// <summary>
        /// Gets a value indicating whether the action is applicable in a given state.
        /// </summary>
        /// <param name="state"></param>
        /// <returns>A value indicating whether the action is applicable in a given state.</returns>
        public bool IsApplicableTo(State state) => Precondition.Elements.IsSubsetOf(state.Elements);

        /// <summary>
        /// Applies this action to a given state, producing a new state.
        /// <para/>
        /// NB: Does not validate preconditions to be of use with particular heuristics..
        /// </summary>
        /// <param name="state">The state to apply the action to.</param>
        /// <returns>The new state.</returns>
        public State ApplyTo(State state) => new State(state.Elements.Except(DeleteList).Union(AddList));
    }
}
