using SCFirstOrderLogic;
using static SCFirstOrderLogic.SentenceCreation.OperableSentenceFactory;
using static SCClassicalPlanning.ProblemCreation.OperableProblemFactory;

namespace SCClassicalPlanning.ExampleDomains.AsCode;

/// <summary>
/// The "one at a time" variant "Air Cargo" example from §10.3.3 of "Artificial Intelligence: A Modern Approach"
/// that is used as an illustrative example while explaining termination of the GraphPlan algorithm.  
/// </summary>
public static class AirCargoOneAtATime
{
    static AirCargoOneAtATime()
    {
        Cargo1 = new(nameof(Cargo1));
        Cargo2 = new(nameof(Cargo2));
        Cargo3 = new(nameof(Cargo3));
        Plane = new(nameof(Plane));
        Airport1 = new(nameof(Airport1));
        Airport2 = new(nameof(Airport2));

        VariableDeclaration cargo = new(nameof(cargo));
        VariableDeclaration plane = new(nameof(plane));
        VariableDeclaration airport = new(nameof(airport));
        VariableDeclaration from = new(nameof(from));
        VariableDeclaration to = new(nameof(to));

        Domain domain = new(new Action[]
        {
            Load(cargo, plane, airport),
            Unload(cargo, plane, airport),
            Fly(plane, from, to),
        });

        Problem = new(
            domain: domain,
            initialState: new(
                IsCargo(Cargo1)
                & IsCargo(Cargo2)
                & IsCargo(Cargo3)
                & IsPlane(Plane)
                & IsAirport(Airport1)
                & AreEqual(Airport1, Airport1)
                & IsAirport(Airport2)
                & AreEqual(Airport2, Airport2)
                & IsAt(Cargo1, Airport1)
                & IsAt(Cargo2, Airport1)
                & IsAt(Cargo3, Airport1)
                & IsAt(Plane, Airport1)),
            goal: new(
                IsAt(Cargo1, Airport2)
                & IsAt(Cargo2, Airport2)
                & IsAt(Cargo3, Airport2)));
    }

    public static Problem Problem { get; }

    public static Constant Cargo1 { get; }

    public static Constant Cargo2 { get; }

    public static Constant Cargo3 { get; }

    public static Constant Plane { get; }

    public static Constant Airport1 { get; }

    public static Constant Airport2 { get; }

    public static OperablePredicate IsCargo(Term cargo) => new Predicate(nameof(IsCargo), cargo);
    public static OperablePredicate IsPlane(Term plane) => new Predicate(nameof(IsPlane), plane);
    public static OperablePredicate IsAirport(Term airport) => new Predicate(nameof(IsAirport), airport);
    public static OperablePredicate IsAt(Term @object, Term location) => new Predicate(nameof(IsAt), @object, location);
    public static OperablePredicate IsIn(Term @object, Term container) => new Predicate(nameof(IsIn), @object, container);
    public static OperablePredicate AreEqual(Term x, Term y) => new Predicate(EqualityIdentifier.Instance, x, y);

    public static Action Load(Term cargo, Term plane, Term airport) => new OperableAction(
        identifier: nameof(Load),
        precondition:
            IsCargo(cargo)
            & IsPlane(plane)
            & IsAirport(airport)
            & IsAt(cargo, airport)
            & IsAt(plane, airport)
            // Note that with our model, goals must be conjunctions of literals.
            // In particular, we can't state, "∀x, ¬IsIn(x, plane)" as a precondition.
            // So, the "one at a time" constraint needs to know about all the bits of cargo:
            & !IsIn(Cargo1, plane)
            & !IsIn(Cargo2, plane)
            & !IsIn(Cargo3, plane),
        effect:
            !IsAt(cargo, airport)
            & IsIn(cargo, plane));

    public static Action Unload(Term cargo, Term plane, Term airport) => new OperableAction(
        identifier: nameof(Unload),
        precondition:
            IsCargo(cargo)
            & IsPlane(plane)
            & IsAirport(airport)
            & IsIn(cargo, plane)
            & IsAt(plane, airport),
        effect:
            IsAt(cargo, airport)
            & !IsIn(cargo, plane));

    public static Action Fly(Term plane, Term from, Term to) => new OperableAction(
        identifier: nameof(Fly),
        precondition:
            IsPlane(plane)
            & IsAirport(from)
            & IsAirport(to)
            & !AreEqual(from, to)
            & IsAt(plane, from),
        effect:
            !IsAt(plane, from)
            & IsAt(plane, to));
}