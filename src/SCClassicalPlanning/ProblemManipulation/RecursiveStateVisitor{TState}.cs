namespace SCClassicalPlanning.ProblemManipulation
{
    /// <summary>
    /// Base class for recursive visitors of <see cref="State"/> instances that reference external visitation state
    /// (as opposed to maintaining state as fields of the visitor).
    /// </summary>
    public abstract class RecursiveStateVisitor<TState> : IStateVisitor<TState>
    {
        /// <summary>
        /// Visits a <see cref="State"/> instance.
        /// The default implementation just visits all of the state's elements.
        /// </summary>
        /// <param name="state">The <see cref="State"/> instance to visit.</param>
        public virtual void Visit(State state, TState visitState)
        {
            foreach (var literal in state.Elements)
            {
                Visit(literal, visitState);
            }
        }

        /// <summary>
        /// Visits a <see cref="Literal"/> instance. 
        /// The default implementation just visits the underlying predicate.
        /// </summary>
        /// <param name="literal">The <see cref="Literal"/> instance to visit.</param>
        public virtual void Visit(Literal literal, TState visitState)
        {
            Visit(literal.Predicate, visitState);
        }

        /// <summary>
        /// Visits a <see cref="Predicate"/> instance. 
        /// The default implementation just visits each of the arguments.
        /// </summary>
        /// <param name="predicate">The <see cref="Predicate"/> instance to visit.</param>
        public virtual void Visit(Predicate predicate, TState visitState)
        {
            foreach (var argument in predicate.Arguments)
            {
                Visit(argument, visitState);
            }
        }

        /// <summary>
        /// Visits a <see cref="Variable"/> instance.
        /// The default implementation doesn't do anything.
        /// </summary>
        /// <param name="variable">The variable reference to visit.</param>
        public virtual void Visit(Variable variable, TState visitState)
        {
        }
    }
}
