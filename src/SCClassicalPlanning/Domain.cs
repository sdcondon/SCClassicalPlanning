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
                GoalPredicateFinder.Instance.Visit(action.Precondition, predicates);
                EffectPredicateFinder.Instance.Visit(action.Effect, predicates);
            }

            Predicates = new ReadOnlyCollection<Predicate>(predicates.ToList());

            var constants = new HashSet<Constant>();
            foreach (var action in actions)
            {
                GoalConstantFinder.Instance.Visit(action.Precondition, constants);
                EffectConstantFinder.Instance.Visit(action.Effect, constants);
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

        private class GoalPredicateFinder : RecursiveGoalVisitor<HashSet<Predicate>>
        {
            public static GoalPredicateFinder Instance { get; } = new();

            public override void Visit(Predicate predicate, HashSet<Predicate> predicates)
            {
                predicates.Add((Predicate)new VariableChanger().ApplyTo(predicate));
            }
        }

        private class EffectPredicateFinder : RecursiveEffectVisitor<HashSet<Predicate>>
        {
            public static EffectPredicateFinder Instance { get; } = new();

            public override void Visit(Predicate predicate, HashSet<Predicate> predicates)
            {
                predicates.Add((Predicate)new VariableChanger().ApplyTo(predicate));
            }
        }

        private class VariableChanger : RecursiveSentenceTransformation
        {
            // Yeah, could be safer, but realistically not going to have a predicate with this many parameters..
            private readonly IEnumerator<string> symbols = Enumerable.Range(0, 26).Select(i => ((char)('A' + i)).ToString()).GetEnumerator();

            public override Term ApplyTo(Term term)
            {
                // Ultimately, it might be a nice quality-of-life improvement to
                // keep variable names as-is if its appropriate (e.g. if its the same
                // in all copies of this predicate) - but can come back to that.
                symbols.MoveNext();
                return new VariableReference(symbols.Current);
            }
        }

        private class GoalConstantFinder : RecursiveGoalVisitor<HashSet<Constant>>
        {
            public static GoalConstantFinder Instance { get; } = new();

            public override void Visit(Constant constant, HashSet<Constant> constants) => constants.Add(constant);
        }

        private class EffectConstantFinder : RecursiveEffectVisitor<HashSet<Constant>>
        {
            public static EffectConstantFinder Instance { get; } = new();

            public override void Visit(Constant constant, HashSet<Constant> constants) => constants.Add(constant);
        }
    }
}
