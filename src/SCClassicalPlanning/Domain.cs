using SCClassicalPlanning.Planning;
using SCFirstOrderLogic;
using SCFirstOrderLogic.SentenceManipulation;
using SCFirstOrderLogic.SentenceManipulation.Unification;
using System.Collections.ObjectModel;

namespace SCClassicalPlanning
{
    /// <summary>
    /// Container for information about a domain.
    /// <para/>
    /// A domain defines the aspects that are common to of all problems that occur within it.
    /// Specifically, the <see cref="Action"/>s available within it.
    /// </summary>
    public class Domain
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Domain"/> class.
        /// </summary>
        /// <param name="actions">The set of actions that exist within the domain.</param>
        public Domain(IList<Action> actions)
        {
            Actions = new ReadOnlyCollection<Action>(actions);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Domain"/> class.
        /// </summary>
        /// <param name="actions">The set of actions that exist within the domain.</param>
        public Domain(params Action[] actions) : this((IList<Action>)actions) { }

        /// <summary>
        /// Gets the set of actions that exist within the domain.
        /// </summary>
        public ReadOnlyCollection<Action> Actions { get; }

#if false
TODO: more useful than predicates..
        /// <summary>
        /// Gets the set of constants that exist within the domain
        /// </summary>
        public ReadOnlyCollection<Constant> Constants { get; }
#endif
    }
}
