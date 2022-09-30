using System.Collections.ObjectModel;

namespace SCClassicalPlanningAlternatives.OwnFolModel
{
    /// <summary>
    /// Container of information about a planning domain.
    /// <para/>
    /// A domain defines the aspects that are common to of all problems that occur within it.
    /// </summary>
    public sealed class Domain
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Domain"/> class.
        /// </summary>
        /// <param name="predicates">The set of predicates that exist within the domain.</param>
        /// <param name="actions">The set of actions that exist within the domain.</param>
        public Domain(IList<Predicate> predicates, IList<Action> actions)
        {
            Predicates = new ReadOnlyCollection<Predicate>(predicates);
            Actions = new ReadOnlyCollection<Action>(actions);
        }

        /// <summary>
        /// Gets the set of predicates that exist within the domain.
        /// </summary>
        public IReadOnlyCollection<Predicate> Predicates { get; }

        /// <summary>
        /// Gets the set of actions that exist within the domain.
        /// </summary>
        public IReadOnlyCollection<Action> Actions { get; }
    }
}
