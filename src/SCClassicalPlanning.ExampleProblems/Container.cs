using static SCClassicalPlanning.ProblemCreation.CommonVariableDeclarations;

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

        public static Predicate IsPresent(Variable @object) => new(nameof(IsPresent), @object);

        public static Action Add(Variable @object) => new(
            identifier: nameof(Add),
            precondition: !IsPresent(@object),
            effect: IsPresent(@object));

        public static Action Remove(Variable @object) => new(
            identifier: nameof(Remove),
            precondition: IsPresent(@object),
            effect: !IsPresent(@object));

        public static Action Swap(Variable remove, Variable add) => new(
            identifier: nameof(Swap),
            precondition: IsPresent(remove) & !IsPresent(add),
            effect: !IsPresent(remove) & IsPresent(add));
    }
}