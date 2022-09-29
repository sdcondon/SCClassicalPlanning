using SCFirstOrderLogic;
using static SCFirstOrderLogic.SentenceCreation.OperableSentenceFactory;

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

        public static OperablePredicate Cargo(Term cargo) => new Predicate(nameof(Cargo), cargo);
        public static OperablePredicate Plane(Term plane) => new Predicate(nameof(Plane), plane);
        public static OperablePredicate Airport(Term airport) => new Predicate(nameof(Airport), airport);
        public static OperablePredicate At(Term @object, Term location) => new Predicate(nameof(At), @object, location);
        public static OperablePredicate In(Term @object, Term container) => new Predicate(nameof(In), @object, container);

        public static Action Load(Term cargo, Term plane, Term airport) => new(
            identifier: nameof(Load),
            precondition: new State(
                At(cargo, airport)
                & At(plane, airport)
                & Cargo(cargo)
                & Plane(plane)
                & Airport(airport)),
            effect: new Effect(
                !At(cargo, airport)
                & In(cargo, plane)));

        public static Action Unload(Term cargo, Term plane, Term airport) => new(
            identifier: nameof(Unload),
            precondition: new State(
                In(cargo, plane)
                & At(plane, airport)
                & Cargo(cargo)
                & Plane(plane)
                & Airport(airport)),
            effect: new Effect(
                At(cargo, airport)
                & !In(cargo, plane)));

        public static Action Fly(Term plane, Term from, Term to) => new(
            identifier: nameof(Fly),
            precondition: new State(
                At(plane, from)
                & Plane(plane)
                & Airport(from)
                & Airport(to)),
            effect: new Effect(
                !At(plane, from)
                & At(plane, to)));
    }
}