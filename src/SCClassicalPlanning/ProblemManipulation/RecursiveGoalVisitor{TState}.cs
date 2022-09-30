using SCFirstOrderLogic;

namespace SCClassicalPlanning.ProblemManipulation
{
    /// <summary>
    /// Base class for recursive visitors of <see cref="Goal"/> instances that reference external visitation state
    /// (as opposed to maintaining state as fields of the visitor).
    /// </summary>
    public abstract class RecursiveGoalVisitor<TState> : IGoalVisitor<TState>
    {
        /// <summary>
        /// Visits a <see cref="Goal"/> instance.
        /// The default implementation just visits all of the goal's elements.
        /// </summary>
        /// <param name="goal">The <see cref="Goal"/> instance to visit.</param>
        public virtual void Visit(Goal goal, TState visitState)
        {
            foreach (var literal in goal.Elements)
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
        /// Visits a <see cref="Term"/> instance.
        /// The default implementation doesn't do anything.
        /// </summary>
        /// <param name="term">The term to visit.</param>
        public virtual void Visit(Term term, TState visitState)
        {
            switch (term)
            {
                case Constant constant:
                    Visit(constant, visitState);
                    break;
                case VariableReference variableReference:
                    Visit(variableReference, visitState);
                    break;
                default:
                    // NB: FUNCTIONS UNSUPPORTED
                    throw new ArgumentException("Unsupported term type", nameof(term));
            };
        }

        /// <summary>
        /// Visits a <see cref="Constant"/> instance.
        /// The default implementation doesn't do anything.
        /// </summary>
        /// <param name="constant">The constant to visit.</param>
        public virtual void Visit(Constant constant, TState visitState)
        {
        }

        /// <summary>
        /// Visits a <see cref="VariableReference"/> instance.
        /// The default implementation doesn't do anything.
        /// </summary>
        /// <param name="variable">The variable reference to visit.</param>
        public virtual void Visit(VariableReference variableReference, TState visitState)
        {
        }
    }
}
