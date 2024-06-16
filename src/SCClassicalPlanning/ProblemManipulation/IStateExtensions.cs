using SCFirstOrderLogic;
using SCFirstOrderLogic.SentenceManipulation;

namespace SCClassicalPlanning.ProblemManipulation;

public static class IStateExtensions
{
    public static IEnumerable<Constant> GetAllConstants(this IState state)
    {
        return state.Elements.SelectMany(p => ConstantFinder.GetAllConstants(p)).Distinct();
    }

    public static IEnumerable<Predicate> GetAllPredicates(this IQueryable<Action> actions)
    {
        return actions.SelectMany(a => PredicateFinder.GetAllPredicates(a)).Distinct();
    }

    /// <summary>
    /// Utility class to find <see cref="Constant"/> instances within the elements of a <see cref="IState"/>, and add them to a given <see cref="HashSet{T}"/>.
    /// </summary>
    private class ConstantFinder : RecursiveSentenceVisitor<HashSet<Constant>>
    {
        private static readonly ConstantFinder _instance = new();

        private ConstantFinder() { }

        public static IEnumerable<Constant> GetAllConstants(Predicate predicate)
        {
            HashSet<Constant> result = new();
            _instance.Visit(predicate, result);
            return result;
        }

        /// <inheritdoc/>
        public override void Visit(Constant constant, HashSet<Constant> constants) => constants.Add(constant);
    }

    private class PredicateFinder : RecursiveActionVisitor<HashSet<Predicate>>
    {
        private static readonly PredicateFinder _instance = new();

        private PredicateFinder() { }

        public static IEnumerable<Predicate> GetAllPredicates(Action action)
        {
            HashSet<Predicate> result = new();
            _instance.Visit(action, result);
            return result;
        }

        public override void Visit(Predicate predicate, HashSet<Predicate> predicates)
        {
            // Standardise the arguments so that we unify all occurences of the 'same' predicate (with the same symbol and same number of arguments)
            // Yeah, could be safer, but realistically not going to have a predicate with this many parameters..
            // Ultimately, it might be a nice quality-of-life improvement to keep variable names as-is if its appropriate (e.g. if its the same
            // in all copies of this predicate) - but can come back to that.
            var standardisedParameters = Enumerable.Range(0, predicate.Arguments.Count).Select(i => new VariableReference(((char)('A' + i)).ToString())).ToArray();
            predicates.Add(new Predicate(predicate.Identifier, standardisedParameters));
        }
    }
}
