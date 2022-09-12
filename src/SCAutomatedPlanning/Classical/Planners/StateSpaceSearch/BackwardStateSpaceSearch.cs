using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCAutomatedPlanning.Classical.Planners.StateSpaceSearch
{
    /// <summary>
    /// A simple implementation of <see cref="IPlanner"/> that carries out a backward search of
    /// the state space to create plans.
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
