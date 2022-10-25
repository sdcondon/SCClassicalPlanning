using SCClassicalPlanning.ProblemManipulation;
using SCFirstOrderLogic;
using SCFirstOrderLogic.SentenceManipulation;
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
        public Domain(IList<Action> actions) // TODO: perhaps allow for the specification of additional constants?
        {
            Actions = new ReadOnlyCollection<Action>(actions);

            var predicates = new HashSet<Predicate>();
            foreach (var action in actions)
            {
                PredicateFinder.Instance.Visit(action, predicates);
            }

            Predicates = new ReadOnlyCollection<Predicate>(predicates.ToList());

            var constants = new HashSet<Constant>();
            foreach (var action in actions)
            {
                ConstantFinder.Instance.Visit(action, constants);
            }

            Constants = new ReadOnlyCollection<Constant>(constants.ToList());
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

        /// <summary>
        /// Gets the set of predicates that exist within the domain.
        /// </summary>
        public ReadOnlyCollection<Predicate> Predicates { get; }

        /// <summary>
        /// Gets the set of constants that exist within the domain
        /// </summary>
        public ReadOnlyCollection<Constant> Constants { get; }

        private class PredicateFinder : RecursiveActionVisitor<HashSet<Predicate>>
        {
            public static PredicateFinder Instance { get; } = new();

            public override void Visit(Predicate predicate, HashSet<Predicate> predicates)
            {
                // Standardise the arguments so that we unify all occurences of the same predicate (with the same symbol and same number of args)
                // Yeah, could be safer, but realistically not going to have a predicate with this many parameters..
                // Ultimately, it might be a nice quality-of-life improvement to keep variable names as-is if its appropriate (e.g. if its the same
                // in all copies of this predicate) - but can come back to that.
                var standardisedParameters = Enumerable.Range(0, predicate.Arguments.Count).Select(i => new VariableReference(((char)('A' + i)).ToString())).ToArray();
                predicates.Add(new Predicate(predicate.Symbol, standardisedParameters));
            }
        }

        private class ConstantFinder : RecursiveActionVisitor<HashSet<Constant>>
        {
            public static ConstantFinder Instance { get; } = new();

            public override void Visit(Constant constant, HashSet<Constant> constants) => constants.Add(constant);
        }
    }
}
