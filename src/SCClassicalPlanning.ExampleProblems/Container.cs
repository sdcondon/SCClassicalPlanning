using System.Reflection.Metadata;
using static SCClassicalPlanning.StateCreation.OperableStateFactory;

namespace SCClassicalPlanning.ExampleProblems
{
    /// <summary>
    /// Incredibly simple domain, used for tests.
    /// </summary>
    public static class Container
    {
        /// <summary>
        /// Gets the available actions.
        /// </summary>
        public static IEnumerable<Action> ActionSchemas { get; } = new Action[]
        {
            Add(A),
            Remove(R),
            Swap(R, A),
        };

        public static Constant Element1 { get; } = new(nameof(Element1));

        public static Constant Element2 { get; } = new(nameof(Element2));

        public static OperableLiteral IsPresent(Term @object) => new Predicate(nameof(IsPresent), @object);

        public static Action Add(Term @object) => new(
            symbol: nameof(Add),
            precondition: !IsPresent(@object),
            effect: IsPresent(@object));

        public static Action Remove(Term @object) => new(
            symbol: nameof(Remove),
            precondition: IsPresent(@object),
            effect: !IsPresent(@object));

        public static Action Swap(Term remove, Term add) => new(
            symbol: nameof(Swap),
            precondition: IsPresent(remove) & !IsPresent(add),
            effect: !IsPresent(remove) & IsPresent(add));
    }
}