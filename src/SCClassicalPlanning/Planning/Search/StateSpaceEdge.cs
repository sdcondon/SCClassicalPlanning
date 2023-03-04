using SCGraphTheory;

namespace SCClassicalPlanning.Planning.Search
{
    /// <summary>
    /// Represents an edge in the state space of a planning problem.
    /// </summary>
    // NB: three ref-valued fields puts this on the verge of being too large for a struct (24 bytes on a 64-bit system).
    // Probably worth comparing performance with a class-based graph at some point, but meh, it'll do for now.
    public readonly struct StateSpaceEdge : IEdge<StateSpaceNode, StateSpaceEdge>
    {
        /// <summary>
        /// The problem whose state space this edge is a member of.
        /// </summary>
        public readonly Problem Problem;

        /// <summary>
        /// The state represented by the node that this edge connects from.
        /// </summary>
        public readonly State FromState;

        /// <summary>
        /// The action represented by this egde.
        /// </summary>
        public readonly Action Action;

        /// <summary>
        /// Initialises a new instance of the <see cref="StateSpaceEdge"/> struct.
        /// </summary>
        /// <param name="problem">The problem whose state space this edge is a member of.</param>
        /// <param name="fromState">The state represented by the node that this edge connects from.</param>
        /// <param name="action">The action represented by this egde.</param>
        public StateSpaceEdge(Problem problem, State fromState, Action action)
        {
            Problem = problem;
            FromState = fromState;
            Action = action;
        }

        /// <inheritdoc />
        public StateSpaceNode From => new(Problem, FromState);

        /// <inheritdoc />
        public StateSpaceNode To => new(Problem, Action.ApplyTo(FromState));

        /// <inheritdoc />
        public override string ToString() => new PlanFormatter(Problem.Domain).Format(Action);
    }
}
