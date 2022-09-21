using SCFirstOrderLogic;
using System.Collections.ObjectModel;

namespace SCClassicalPlanningAlternatives.UsingSCFirstOrderLogic
{
    /// <summary>
    /// Interface for containers of information about a domain.
    /// <para/>
    /// A domain defines the aspects that are common to of all problems that occur within it.
    /// </summary>
    public sealed class Domain
    {
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
