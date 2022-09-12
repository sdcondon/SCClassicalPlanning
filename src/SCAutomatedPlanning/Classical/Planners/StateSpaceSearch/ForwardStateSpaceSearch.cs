using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCAutomatedPlanning.Classical.Planners.StateSpaceSearch
{
    public class ForwardStateSpaceSearch : IPlanner
    {
        public ICollection<Action> Actions => throw new NotImplementedException();

        public Task<IPlan> CreatePlanAsync(State initialState, State goalState)
        {
            throw new NotImplementedException();
        }
    }
}
