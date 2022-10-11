namespace SCClassicalPlanning.Planning.StateSpaceSearch
{
    /// <summary>
    /// Interface for state space search heuristic implementations. That is, types that can estimate the
    /// cost of getting from a given state to a state that satisfies a given goal.
    /// </summary>
    public interface IHeuristic
    {
        /// <summary>
        /// Estimates the cost of getting from a given state to a state that satisfies a given goal.
        /// </summary>
        /// <param name="state">The start state.</param>
        /// <param name="goal">The goal to be satisfied.</param>
        /// <returns>The estimated cost.</returns>
        float EstimateCost(State state, Goal goal);
    }
}
