using SCFirstOrderLogic;
using static SCClassicalPlanning.ProblemCreation.OperableProblemFactory;
using static SCFirstOrderLogic.SentenceCreation.OperableSentenceFactory;

namespace SCClassicalPlanning.ExampleDomains
{
    /// <summary>
    /// Incredibly simple domain, used for tests.
    /// </summary>
    public static class Container
    {
        /// <summary>
        /// Gets a <see cref="SCClassicalPlanning.Domain"/ instance that encapsulates the "Container" domain.
        /// </summary>
        public static Domain Domain { get; } = MakeDomain();

        public static OperablePredicate IsPresent(Term @object) => new Predicate(nameof(IsPresent), @object);

        public static Action Add(Term @object) => new OperableAction(
            identifier: nameof(Add),
            precondition: !IsPresent(@object),
            effect: IsPresent(@object));

        public static Action Remove(Term @object) => new OperableAction(
            identifier: nameof(Remove),
            precondition: IsPresent(@object),
            effect: !IsPresent(@object));

        public static Action Swap(Term remove, Term add) => new OperableAction(
            identifier: nameof(Swap),
            precondition: IsPresent(remove) & !IsPresent(add),
            effect: !IsPresent(remove) & IsPresent(add));

        /// <summary>
        /// Creates a new <see cref="Problem"/> instance that refers to this domain.
        /// </summary>
        /// <param name="initialState">The initial state of the problem.</param>
        /// <param name="goal">The initial state of the problem.</param>
        /// <returns>A new <see cref="Problem"/> instance that refers to this domain.</returns>
        public static Problem MakeProblem(State initialState, Goal goal) => new Problem(Domain, initialState, goal);

        // NB: This is in its own method so that we can run tests against domain construction.
        internal static Domain MakeDomain()
        {
            return new Domain(new Action[]
            {
                Add(A),
                Remove(R),
                Swap(R, A),
            });
        }
    }
}