using SCFirstOrderLogic;
using static SCClassicalPlanning.ProblemCreation.OperableProblemFactory;
using static SCFirstOrderLogic.SentenceCreation.OperableSentenceFactory;

namespace SCClassicalPlanning.ExampleDomains.AsCode;

/// <summary>
/// The "Spare Tire" example from §10.1.2 of "Artificial Intelligence: A Modern Approach".
/// </summary>
public static class SpareTireDomain
{
    static SpareTireDomain()
    {
        ActionSchemas = MakeActionSchemas();

        ExampleProblem = MakeProblem(
            initialState: new HashSetState(
                IsTire(Spare)
                & IsTire(Flat)
                & IsAt(Flat, Axle)
                & IsAt(Spare, Trunk)),
            goal: new(
                IsAt(Spare, Axle)));
    }

    /// <summary>
    /// Gets the actions that are available in the "Spare Tire" domain.
    /// </summary>
    public static IQueryable<Action> ActionSchemas { get; }

    /// <summary>
    /// Gets an instance of the customary example problem in this domain.
    /// In the initial state, the Spare (is a tire and) is in the Trunk, and the Flat (is a tire and) is on the Axle.
    /// The goal is to get the Spare onto the Axle.
    /// </summary>
    public static Problem ExampleProblem { get; }

    public static Constant Spare { get; } = new(nameof(Spare));
    public static Constant Flat { get; } = new(nameof(Flat));
    public static Constant Ground { get; } = new(nameof(Ground));
    public static Constant Axle { get; } = new(nameof(Axle));
    public static Constant Trunk { get; } = new(nameof(Trunk));

    public static OperablePredicate IsTire(Term tire) => new Predicate(nameof(IsTire), tire);

    public static OperablePredicate IsAt(Term item, Term location) => new Predicate(nameof(IsAt), item, location);

    public static Action Remove(Term @object, Term location) => new OperableAction(
        identifier: nameof(Remove),
        precondition: IsAt(@object, location),
        effect: !IsAt(@object, location) & IsAt(@object, Ground));

    public static Action PutOn(Term tire) => new OperableAction(
        identifier: nameof(PutOn),
        precondition: IsTire(tire) & IsAt(tire, Ground) & !IsAt(Flat, Axle),
        effect: !IsAt(tire, Ground) & IsAt(tire, Axle));

    public static Action LeaveOvernight() => new OperableAction(
        identifier: nameof(LeaveOvernight),
        precondition: Goal.Empty,
        effect:
            !IsAt(Spare, Ground)
            & !IsAt(Spare, Axle)
            & !IsAt(Spare, Trunk)
            & !IsAt(Flat, Ground)
            & !IsAt(Flat, Axle)
            & !IsAt(Flat, Trunk));

    /// <summary>
    /// Creates a new <see cref="Problem"/> instance that refers to this domain.
    /// </summary>
    /// <param name="initialState">The initial state of the problem.</param>
    /// <param name="goal">The initial state of the problem.</param>
    /// <returns>A new <see cref="Problem"/> instance that refers to this domain.</returns>
    public static Problem MakeProblem(IState initialState, Goal goal) => new(initialState, goal, ActionSchemas);

    private static IQueryable<Action> MakeActionSchemas()
    {
        return new[]
        {
            Remove(Var("object"), Var("location")),
            PutOn(Var("tire")),
            LeaveOvernight(),
        }.AsQueryable();
    }
}
