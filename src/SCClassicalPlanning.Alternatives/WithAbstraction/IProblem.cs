namespace SCClassicalPlanningAlternatives.WithAbstraction
{
    /// <summary>
    /// 
    /// </summary>
    public interface IProblem
    {
        /// <summary>
        /// Gets the domain in which this problem resides.
        /// </summary>
        public IDomain Domain { get; }

        /// <summary>
        /// Gets the objects that exist in the problem.
        /// </summary>
        public IReadOnlyCollection<IVariable> Objects { get; }

        /// <summary>
        /// Gets the initial state of the problem.
        /// </summary>
        public IState InitialState { get; }

        /// <summary>
        /// Gets the initial state of the problem.
        /// </summary>
        public IState GoalState { get; }
    }
}
