namespace SCClassicalPlanning.Planning.StateSpaceSearch.Heuristics
{
    /// <summary>
    /// Heuristic implementation that just invokes a given delegate.
    /// </summary>
    public class DelegateHeuristic : IHeuristic
    {
        private readonly Func<State, Goal, float> estimateCost;

        /// <summary>
        /// Initialises a new instance of the <see cref="DelegateHeuristic"/> class.
        /// </summary>
        /// <param name="estimateCost">The delegate to invoke.</param>
        public DelegateHeuristic(Func<State, Goal, float> estimateCost) => this.estimateCost = estimateCost;

        /// <inheritdoc/>
        public float EstimateCost(State state, Goal goal) => estimateCost(state, goal);
    }
}
