namespace SCClassicalPlanning
{
    /// <summary>
    /// Container for descriptive information about an action.
    /// </summary>
    public class Action
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Action"/> class.
        /// </summary>
        /// <param name="symbol">The symbol for the action.</param>
        /// <param name="precondition">The preconditional state for the action to be applicable.</param>
        /// <param name="effect">The effective state after the application of the action.</param>
        public Action(object symbol, State precondition, State effect)
        {
            Symbol = symbol;
            Precondition = precondition;
            Effect = effect;
        }

        /// <summary>
        /// Gets the symbol for the action.
        /// </summary>
        public object Symbol { get; }

        /// <summary>
        /// Gets the preconditional state for the action to be applicable.
        /// </summary>
        public State Precondition { get; }

        /// <summary>
        /// Gets the effective state after the application of the action.
        /// </summary>
        public State Effect { get; }

        /// <summary>
        /// Gets the "add list" of the action - the non-negated atoms in the action's effect.
        /// </summary>
        public IEnumerable<Literal> AddList => Effect.Atoms.Where(a => !a.IsNegated);

        /// <summary>
        /// Gets the "delete list" of the action - the negated atoms in the action's effect.
        /// </summary>
        public IEnumerable<Literal> DeleteList => Effect.Atoms.Where(a => a.IsNegated);

        /// <summary>
        /// Gets a value indicating whether the action is applicable in a given state.
        /// </summary>
        /// <param name="state"></param>
        /// <returns>A value indicating whether the action is applicable in a given state.</returns>
        public bool IsApplicableTo(State state) => Precondition.Atoms.IsSubsetOf(state.Atoms);

        /// <summary>
        /// Applies this action to a given state, producing a new state.
        /// <para/>
        /// NB: Does not validate preconditions to be of use with particular heuristics..
        /// </summary>
        /// <param name="state">The state to apply the action to.</param>
        /// <returns>The new state.</returns>
        public State ApplyTo(State state) => new State(state.Atoms.Except(DeleteList).Union(AddList));
    }
}