using SCFirstOrderLogic;
using SCFirstOrderLogic.SentenceManipulation;
using System.Collections.ObjectModel;

namespace SCClassicalPlanning
{
    /// <summary>
    /// Container for information about a domain.
    /// <para/>
    /// A domain defines the aspects that are common to of all problems that occur within it.
    /// Specifically, the <see cref="Predicate"/>s relevant to it and the <see cref="Action"/>s available within it.
    /// </summary>
    public class Domain
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Domain"/> class.
        /// </summary>
        /// <param name="actions">The set of actions that exist within the domain.</param>
        /// <param name="predicates">The set of predicates that exist within the domain.</param>
        public Domain(IList<Action> actions, IList<Predicate> predicates)
        {
            Actions = new ReadOnlyCollection<Action>(actions);
            Predicates = new ReadOnlyCollection<Predicate>(predicates);
        }

        /// <summary>
        /// Gets the set of actions that exist within the domain.
        /// </summary>
        public ReadOnlyCollection<Action> Actions { get; }

        /// <summary>
        /// Gets the set of predicates that exist within the domain.
        /// </summary>
        public ReadOnlyCollection<Predicate> Predicates { get; }

#if false
        /// <summary>
        /// Gets the set of constants that exist within the domain
        /// </summary>
        public ReadOnlyCollection<Constant> Constants { get; }

        /// <summary>
        /// TODO: NOT YET IMPLEMENTED.. CURRENTLY THROWS AN EXCEPTION
        /// <para/>
        /// Gets the variable substitution that must be made to convert the schema (with the matching identifier)
        /// in the <see cref="Actions"/> collection to a given action.
        /// <para/>
        /// Intended to be useful for succinct output of plan steps. We don't want to "bloat" our action model with this
        /// information (planners won't and shouldn't care what the original variable name was), but it is useful when
        /// producing human-readable information.
        /// </summary>
        /// <returns>A <see cref="VariableSubstitution"/> that maps the variable as defined in the schema to the term referred to in the action.</returns>
        public VariableSubstitution GetBinding(Action action)
        {
            throw new NotImplementedException();
        }
#endif
    }
}
