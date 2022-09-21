namespace SCClassicalPlanningAlternatives.WithAbstraction
{
    /// <summary>
    /// Interface for containers of information about a domain.
    /// <para/>
    /// A domain defines the aspects that are common to of all problems that occur within it.
    /// </summary>
    public interface IDomain
    {
        /// <summary>
        /// Gets the set of predicates that exist within the domain.
        /// </summary>
        public IReadOnlyCollection<IPredicate> Predicates { get; }

        /// <summary>
        /// Gets the set of actions that exist within the domain.
        /// </summary>
        public IReadOnlyCollection<IAction> Actions { get; }
    }
}
