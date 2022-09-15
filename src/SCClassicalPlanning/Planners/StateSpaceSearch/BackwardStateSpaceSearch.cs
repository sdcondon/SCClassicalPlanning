namespace SCClassicalPlanning.Planners.StateSpaceSearch
{
    /// <summary>
    /// A simple implementation of <see cref="IPlanner"/> that carries out a backward search of
    /// the state space to create plans. See section 10.2.2 of "Artificial Intelligence: A Modern
    /// Approach" for more on this.
    /// </summary>
    public class BackwardStateSpaceSearch : IPlanner
    {
        /// <inheritdoc />
        public ICollection<Action> Actions => throw new NotImplementedException();

        /// <inheritdoc />
        public Task<IPlan> CreatePlanAsync(State initialState, State goalState)
        {
            throw new NotImplementedException();
        }
    }
}
