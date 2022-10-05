﻿using SCFirstOrderLogic;

namespace SCClassicalPlanning.ProblemManipulation
{
    /// <summary>
    /// Interface for visitors of <see cref="Literal"/> instances.
    /// </summary>
    /// <typeparam name="TState">The type of state that this visitor works with.</typeparam>
    public interface ILiteralVisitor<TState>
    {
        /// <summary>
        /// Visits a <see cref="Literal"/> instance.
        /// </summary>
        /// <param name="literal">The <see cref="Literal"/> instance to visit.</param>
        /// <param name="visitationState">A reference to the state of this visitation.</param>
        void Visit(Literal literal, TState visitationState);

        /// <summary>
        /// Visits a <see cref="Predicate"/> instance. 
        /// </summary>
        /// <param name="predicate">The <see cref="Predicate"/> instance to visit.</param>
        /// <param name="visitationState">A reference to the state of this visitation.</param>
        void Visit(Predicate predicate, TState visitationState);
    }
}