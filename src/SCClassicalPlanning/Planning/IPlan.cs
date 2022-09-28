﻿using SCClassicalPlanning;

namespace SCClassicalPlanning.Planning
{
    /// <summary>
    /// Interface for types encapsulating a plan of action.
    /// </summary>
    public interface IPlan
    {
        /// <summary>
        /// Gets the steps of the plan.
        /// </summary>
        public IReadOnlyCollection<Action> Steps { get; } 
    }
}
