namespace SCAutomatedPlanning.Classical
{
    /// <summary>
    /// Container for information about a particular classical planning problem.
    /// </summary>
    public class Problem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Problem"/> class.
        /// </summary>
        /// <param name="initialState">The initial state of the problem.</param>
        /// <param name="goalState">The goal state of the problem.</param>
        /// <param name="availableActions">The available actions for the problem.</param>
        public Problem(State initialState, State goalState, ICollection<Action> availableActions)
        {
            InitialState = initialState;
            GoalState = goalState;
            AvailableActions = availableActions; // copy me?
        }

        /// <summary>
        /// Gets the initial state of the problem.
        /// </summary>
        public State InitialState { get; }

        /// <summary>
        /// Gets the goal state of the problem.
        /// </summary>
        public State GoalState { get; }

        /// <summary>
        /// Gets the available actions for the problem.
        /// </summary>
        public ICollection<Action> AvailableActions { get; }
    }
}
