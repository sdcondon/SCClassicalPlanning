using SCFirstOrderLogic;
using static SCFirstOrderLogic.SentenceCreation.OperableSentenceFactory;
using static SCClassicalPlanning.ProblemCreation.OperableProblemFactory;

namespace SCClassicalPlanning.ExampleDomains.AsCode;

/// <summary>
/// The "Air Cargo" example from §10.1.1 of "Artificial Intelligence: A Modern Approach".
/// </summary>
public static class AirCargoDomain
{
    static AirCargoDomain()
    {
        ActionSchemas = MakeActionSchemas();
        ExampleProblem = MakeExampleProblem();
    }

    /// <summary>
    /// Gets the actions that are available in the Air Cargo domain.
    /// </summary>
    public static IQueryable<Action> ActionSchemas { get; }

    /// <summary>
    /// Gets an instance of the customary example problem in this domain.
    /// Consists of two airports, two planes and two pieces of cargo.
    /// In the initial state, plane1 and cargo1 are at airport1; plane2 and cargo2 are at airport2.
    /// The goal is to get cargo2 to airport1 and cargo1 to airport2.
    /// </summary>
    public static Problem ExampleProblem { get; }

    /// <summary>
    /// Constructs an <see cref="OperablePredicate"/> instance for indicating that a given domain element is a piece of cargo.
    /// </summary>
    /// <param name="cargo">A term representing the domain element.</param>
    /// <returns>A new <see cref="OperablePredicate"/> instance for indicating that a given domain element is a piece of cargo.</returns>
    public static OperablePredicate Cargo(Term cargo) => new Predicate(nameof(Cargo), cargo);

    /// <summary>
    /// Constructs an <see cref="OperablePredicate"/> instance for indicating that a given domain element is a plane.
    /// </summary>
    /// <param name="plane">A term representing the domain element.</param>
    /// <returns>A new <see cref="OperablePredicate"/> instance for indicating that a given domain element is a plane.</returns>
    public static OperablePredicate Plane(Term plane) => new Predicate(nameof(Plane), plane);

    /// <summary>
    /// Constructs an <see cref="OperablePredicate"/> instance for indicating that a given domain element is an airport.
    /// </summary>
    /// <param name="airport">A term representing the domain element.</param>
    /// <returns>A new <see cref="OperablePredicate"/> instance for indicating that a given domain element is an airport.</returns>
    public static OperablePredicate Airport(Term airport) => new Predicate(nameof(Airport), airport);

    /// <summary>
    /// Constructs an <see cref="OperablePredicate"/> instance for indicating that a given domain element is "at" another given domain element.
    /// </summary>
    /// <param name="object">A term representing the situated domain element.</param>
    /// <param name="location">A term representing the location.</param>
    /// <returns>A new <see cref="OperablePredicate"/> instance for indicating that a given domain element is "at" another given domain element.</returns>
    public static OperablePredicate At(Term @object, Term location) => new Predicate(nameof(At), @object, location);

    /// <summary>
    /// Constructs an <see cref="OperablePredicate"/> instance for indicating that a given domain element is "in" another given domain element.
    /// </summary>
    /// <param name="object">A term representing the situated domain element.</param>
    /// <param name="container">A term representing the container.</param>
    /// <returns>A new <see cref="OperablePredicate"/> instance for indicating that a given domain element is "in" another given domain element.</returns>
    public static OperablePredicate In(Term @object, Term container) => new Predicate(nameof(In), @object, container);

    /// <summary>
    /// Constructs an <see cref="Action"/> instance that describes loading a given piece of cargo onto a given plane at a given airport.
    /// </summary>
    /// <param name="element">A term representing the piece of cargo.</param>
    /// <param name="element">A term representing the plane.</param>
    /// <param name="element">A term representing the airport.</param>
    /// <returns>A new <see cref="Action"/> instance that describes loading a given piece of cargo onto a given plane at a given airport.</returns>
    public static Action Load(Term cargo, Term plane, Term airport) => new OperableAction(
        identifier: nameof(Load),
        precondition:
            Cargo(cargo)
            & Plane(plane)
            & Airport(airport)
            & At(cargo, airport)
            & At(plane, airport),
        effect:
            !At(cargo, airport)
            & In(cargo, plane));

    /// <summary>
    /// Constructs an <see cref="Action"/> instance that describes unloading a given piece of cargo from a given plane at a given airport.
    /// </summary>
    /// <param name="element">A term representing the piece of cargo.</param>
    /// <param name="element">A term representing the plane.</param>
    /// <param name="element">A term representing the airport.</param>
    /// <returns>A new <see cref="Action"/> instance that describes unloading a given piece of cargo from a given plane at a given airport.</returns>
    public static Action Unload(Term cargo, Term plane, Term airport) => new OperableAction(
        identifier: nameof(Unload),
        precondition:
            Cargo(cargo)
            & Plane(plane)
            & Airport(airport)
            & In(cargo, plane)
            & At(plane, airport),
        effect:
            At(cargo, airport)
            & !In(cargo, plane));

    /// <summary>
    /// Constructs an <see cref="Action"/> instance that describes flying a given plane from a given airport to a given airport.
    /// </summary>
    /// <param name="element">A term representing the plane.</param>
    /// <param name="element">A term representing the origin airport.</param>
    /// <param name="element">A term representing the destination airport.</param>
    /// <returns>A new <see cref="Action"/> instance that describes describes flying a given plane from a given airport to a given airport.</returns>
    public static Action Fly(Term plane, Term origin, Term destination) => new OperableAction(
        identifier: nameof(Fly),
        precondition:
            Plane(plane)
            & Airport(origin)
            & Airport(destination)
            & At(plane, origin)
            & !AreEqual(origin, destination),
        effect:
            !At(plane, origin)
            & At(plane, destination));

    /// <summary>
    /// Creates a new <see cref="Problem"/> instance that refers to this domain.
    /// </summary>
    /// <param name="initialState">The initial state of the problem.</param>
    /// <param name="goal">The initial state of the problem.</param>
    /// <returns>A new <see cref="Problem"/> instance that refers to this domain.</returns>
    public static Problem MakeProblem(IState initialState, Goal goal) => new(initialState, goal, ActionSchemas);

    private static IQueryable<Action> MakeActionSchemas()
    {
        VariableDeclaration cargo = new(nameof(cargo));
        VariableDeclaration plane = new(nameof(plane));
        VariableDeclaration airport = new(nameof(airport));
        VariableDeclaration from = new(nameof(from));
        VariableDeclaration to = new(nameof(to));

        return new[]
        {
            Load(cargo, plane, airport),
            Unload(cargo, plane, airport),
            Fly(plane, from, to),
        }.AsQueryable();
    }

    private static Problem MakeExampleProblem()
    {
        Constant cargo1 = new(nameof(cargo1));
        Constant cargo2 = new(nameof(cargo2));
        Constant plane1 = new(nameof(plane1));
        Constant plane2 = new(nameof(plane2));
        Constant airport1 = new(nameof(airport1));
        Constant airport2 = new(nameof(airport2));

        return MakeProblem(
            initialState: new HashSetState(
                Cargo(cargo1)
                & Cargo(cargo2)
                & Plane(plane1)
                & Plane(plane2)
                & Airport(airport1)
                & AreEqual(airport1, airport1)
                & Airport(airport2)
                & AreEqual(airport2, airport2)
                & At(cargo1, airport1)
                & At(cargo2, airport2)
                & At(plane1, airport1)
                & At(plane2, airport2)),
            goal: new(
                At(cargo2, airport1)
                & At(cargo1, airport2)));
    }
}