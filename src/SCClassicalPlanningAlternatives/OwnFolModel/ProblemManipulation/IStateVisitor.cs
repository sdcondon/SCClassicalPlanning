namespace SCClassicalPlanningAlternatives.OwnFolModel.ProblemManipulation
{
    /// <summary>
    /// Interface for visitors of <see cref="State"/> instances.
    /// </summary>
    public interface IStateVisitor
    {
        /// <summary>
        /// Visits a <see cref="State"/> instance.
        /// </summary>
        /// <param name="state">The <see cref="State"/> instance to visit.</param>
        void Visit(State state);

        /// <summary>
        /// Visits a <see cref="Literal"/> instance.
        /// </summary>
        /// <param name="literal">The <see cref="Literal"/> instance to visit.</param>
        void Visit(Literal literal);

        /// <summary>
        /// Visits a <see cref="Predicate"/> instance. 
        /// </summary>
        /// <param name="predicate">The <see cref="Predicate"/> instance to visit.</param>
        void Visit(Predicate predicate);
    }
}
