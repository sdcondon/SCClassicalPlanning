using static SCClassicalPlanning.ProblemCreation.CommonVariableDeclarations; // For A-Z variable declarations

namespace SCClassicalPlanning.ExampleDomains
{
    /// <summary>
    /// The "Air Cargo" example from section 10.1.1 of "Artificial Intelligence: A Modern Approach".
    /// </summary>
    public class AirCargo
    {
        /// <summary>
        /// Gets a <see cref="SCClassicalPlanning.Domain"/ instance that encapsulates Air Cargo domain.
        /// </summary>
        public static Domain Domain { get; } = new Domain(
            predicates: new Predicate[]
            {
                Cargo(C),
                Plane(P),
                Airport(A),
                In(C, P),
                At(C, A),
            },
            actions: new Action[]
            {
                Load(C, P, A),
                Unload(C, P, A),
                Fly(P, F, T),
            });

        public static Predicate Cargo(Variable cargo) => new Predicate(nameof(Cargo), cargo);
        public static Predicate Plane(Variable plane) => new Predicate(nameof(Plane), plane);
        public static Predicate Airport(Variable airport) => new Predicate(nameof(Airport), airport);
        public static Predicate At(Variable @object, Variable location) => new Predicate(nameof(At), @object, location);
        public static Predicate In(Variable @object, Variable container) => new Predicate(nameof(In), @object, container);

        public static Action Load(Variable cargo, Variable plane, Variable airport) => new(
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

        public static Action Unload(Variable cargo, Variable plane, Variable airport) => new(
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

        public static Action Fly(Variable plane, Variable from, Variable to) => new(
            identifier: nameof(Fly),
            precondition:
                At(plane, from)
                & Plane(plane)
                & Airport(from)
                & Airport(to),
            effect:
                !At(plane, from)
                & At(plane, to));
    }
}