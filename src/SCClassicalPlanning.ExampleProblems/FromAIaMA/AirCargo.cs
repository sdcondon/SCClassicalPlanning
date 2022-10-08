using SCFirstOrderLogic;
using static SCFirstOrderLogic.SentenceCreation.OperableSentenceFactory;
using static SCClassicalPlanning.ProblemCreation.OperableProblemFactory;

namespace SCClassicalPlanning.ExampleDomains.FromAIaMA
{
    /// <summary>
    /// The "Air Cargo" example from section 10.1.1 of "Artificial Intelligence: A Modern Approach".
    /// </summary>
    public static class AirCargo
    {
        static AirCargo()
        {
            VariableDeclaration cargo = new(nameof(cargo));
            VariableDeclaration plane = new(nameof(plane));
            VariableDeclaration airport = new(nameof(airport));
            VariableDeclaration from = new(nameof(from));
            VariableDeclaration to = new(nameof(to));

            Domain = new Domain(new Action[]
            {
                Load(cargo, plane, airport),
                Unload(cargo, plane, airport),
                Fly(plane, from, to),
            });
        }

        /// <summary>
        /// Gets a <see cref="SCClassicalPlanning.Domain"/ instance that encapsulates Air Cargo domain.
        /// </summary>
        public static Domain Domain { get; }

        public static OperablePredicate Cargo(Term cargo) => new Predicate(nameof(Cargo), cargo);
        public static OperablePredicate Plane(Term plane) => new Predicate(nameof(Plane), plane);
        public static OperablePredicate Airport(Term airport) => new Predicate(nameof(Airport), airport);
        public static OperablePredicate At(Term @object, Term location) => new Predicate(nameof(At), @object, location);
        public static OperablePredicate In(Term @object, Term container) => new Predicate(nameof(In), @object, container);

        public static Action Load(Term cargo, Term plane, Term airport) => new OperableAction(
            identifier: nameof(Load),
            precondition:
                At(cargo, airport)
                & At(plane, airport)
                & Cargo(cargo)
                & Plane(plane)
                & Airport(airport),
            effect:
                !At(cargo, airport)
                & In(cargo, plane));

        public static Action Unload(Term cargo, Term plane, Term airport) => new OperableAction(
            identifier: nameof(Unload),
            precondition:
                In(cargo, plane)
                & At(plane, airport)
                & Cargo(cargo)
                & Plane(plane)
                & Airport(airport),
            effect:
                At(cargo, airport)
                & !In(cargo, plane));

        public static Action Fly(Term plane, Term from, Term to) => new OperableAction(
            identifier: nameof(Fly),
            precondition:
                At(plane, from)
                & Plane(plane)
                & Airport(from)
                & Airport(to),
                // & !Equal(from, to),
            effect:
                !At(plane, from)
                & At(plane, to));
    }
}