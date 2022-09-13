using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCAutomatedPlanning.Classical.Planners
{
    /// <summary>
    /// Interface for types that can create plans if given an initial state, goal state and set of possible actions.
    /// </summary>
    public interface IPlanner
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="initialState">The initial state of the problem.</param>
        /// <param name="goalState">The goal state of the problem.</param>
        /// <returns></returns>
        Task<IPlan> CreatePlanAsync(State initialState, State goalState);

        /// <summary>
        /// Gets the collection of available actions.
        /// <para/>
        /// NB: This means planners are mutable. Perhaps worth revisiting - IPlanner and IMutablePlanner at some point..
        /// /// </summary>
        ICollection<Action> Actions { get; }
    }
}
