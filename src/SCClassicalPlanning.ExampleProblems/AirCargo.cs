using SCAutomatedPlanning.Classical;
using static SCAutomatedPlanning.Classical.StateCreation.OperableStateFactory;
using Action = SCAutomatedPlanning.Classical.Action;

namespace SCClassicalPlanning.ExampleProblems
{
    /// <summary>
    /// The "Air Cargo" example from section 10.1.1 of "Artificial Intelligence: A Modern Approach".
    /// </summary>
    public class AirCargo
    {
        /// <summary>
        /// Gets the available actions.
        /// </summary>
        public static IEnumerable<Action> Actions { get; } = new Action[]
        {
            Load(C, P, A),
            Unload(C, P, A),
            Fly(P, F, T),
        };

        public static Variable Cargo1 { get; } = new(nameof(Cargo1));
        public static Variable Cargo2 { get; } = new(nameof(Cargo2));
        public static Variable Plane1 { get; } = new(nameof(Plane1));
        public static Variable Plane2 { get; } = new(nameof(Plane2));
        public static Variable JFK { get; } = new(nameof(JFK));
        public static Variable SFO { get; } = new(nameof(SFO));

        /// <summary>
        /// Gets the implicit state of the world, that will never change as the result of actions.
        /// </summary>
        public static OperableState ImplicitState => Cargo(Cargo1) & Cargo(Cargo2) & Plane(Plane1) & Plane(Plane2) & Airport(JFK) & Airport(SFO);

        public static OperableAtom At(Variable @object, Variable location) => new Literal(nameof(At), @object, location);
        public static OperableAtom In(Variable @object, Variable container) => new Literal(nameof(In), @object, container);
        public static OperableAtom Cargo(Variable cargo) => new Literal(nameof(Cargo), cargo);
        public static OperableAtom Plane(Variable plane) => new Literal(nameof(Plane), plane);

        public static OperableAtom Airport(Variable airport) => new Literal(nameof(Airport), airport);

        public static Action Load(Variable cargo, Variable plane, Variable airport) => new(
            symbol: nameof(Load),
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
            symbol: nameof(Unload),
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
            symbol: nameof(Fly),
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