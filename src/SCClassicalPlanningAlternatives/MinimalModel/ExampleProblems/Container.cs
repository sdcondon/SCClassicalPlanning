using static SCClassicalPlanningAlternatives.MinimalModel.ProblemCreation.CommonVariableDeclarations;

namespace SCClassicalPlanningAlternatives.MinimalModel.ExampleProblems
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

        public static Variable Element1 { get; } = new(nameof(Element1));

        public static Variable Element2 { get; } = new(nameof(Element2));

        public static Literal IsPresent(Variable @object) => new(nameof(IsPresent), @object);

        public static Action Add(Variable @object) => new(
            symbol: nameof(Add),
            precondition: !IsPresent(@object),
            effect: IsPresent(@object));

        public static Action Remove(Variable @object) => new(
            symbol: nameof(Remove),
            precondition: IsPresent(@object),
            effect: !IsPresent(@object));

        public static Action Swap(Variable remove, Variable add) => new(
            symbol: nameof(Swap),
            precondition: IsPresent(remove) & !IsPresent(add),
            effect: !IsPresent(remove) & IsPresent(add));
    }
}