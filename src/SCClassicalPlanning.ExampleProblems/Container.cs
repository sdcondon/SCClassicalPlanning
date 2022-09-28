using SCFirstOrderLogic;
using static SCFirstOrderLogic.SentenceCreation.OperableSentenceFactory;

namespace SCClassicalPlanning.ExampleDomains
{
    /// <summary>
    /// Incredibly simple domain, used for tests.
    /// </summary>
    public static class Container
    {
        /// <summary>
        /// Gets a <see cref="OwnFolModel.Domain"/ instance that encapsulates the "Container" domain.
        /// </summary>
        public static Domain Domain { get; } = new Domain(
            predicates: new Predicate[]
            {
                IsPresent(X),
            },
            actions: new Action[]
            {
                Add(A),
                Remove(R),
                Swap(R, A),
            });

        public static OperablePredicate IsPresent(Term @object) => new Predicate(nameof(IsPresent), @object);

        public static Action Add(Term @object) => new(
            identifier: nameof(Add),
            precondition: new State(!IsPresent(@object)),
            effect: new State(IsPresent(@object)));

        public static Action Remove(Term @object) => new(
            identifier: nameof(Remove),
            precondition: new State(IsPresent(@object)),
            effect: new State(!IsPresent(@object)));

        public static Action Swap(Term remove, Term add) => new(
            identifier: nameof(Swap),
            precondition: new State(IsPresent(remove) & !IsPresent(add)),
            effect: new State(!IsPresent(remove) & IsPresent(add)));
    }
}