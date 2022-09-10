namespace SCAutomatedPlanning.Classical
{
    /// <summary>
    /// Container for an action schema.
    /// </summary>
    public class Action
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Action"/> class.
        /// </summary>
        /// <param name="symbol">The symbol for the action</param>
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
        /// Gets a value indicating whether the action is applicable in a givne state.
        /// </summary>
        /// <param name="state"></param>
        /// <returns>A value indicating whether the action is applicable in a givne state.</returns>
        public bool IsApplicableIn(State state)
        {
            return false;
        }
    }
}