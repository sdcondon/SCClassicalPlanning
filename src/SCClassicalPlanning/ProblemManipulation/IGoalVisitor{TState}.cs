﻿using SCFirstOrderLogic;

namespace SCClassicalPlanning.ProblemManipulation
{
    /// <summary>
    /// Interface for visitors of <see cref="Goal"/> instances.
    /// </summary>
    /// <typeparam name="TState">The type of state that this visitor works with.</typeparam>
    public interface IGoalVisitor<TState>
    {
        /// <summary>
        /// Visits a <see cref="Goal"/> instance.
        /// </summary>
        /// <param name="goal">The <see cref="Goal"/> instance to visit.</param>
        /// <param name="visitationState">A reference to the state of this visitation.</param>
        void Visit(Goal goal, TState visitationState);
    }
}