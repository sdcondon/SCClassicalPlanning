using SCFirstOrderLogic;
using static SCClassicalPlanning.ProblemCreation.OperableProblemFactory;
using static SCFirstOrderLogic.SentenceCreation.OperableSentenceFactory;

namespace SCClassicalPlanning.ExampleDomains.AsCode;

/// <summary>
/// The "have cake and eat cake too" domain used to demonstrate planning graphs in AIaMA.
/// </summary>
public class HaveCakeAndEatCakeTooDomain
{
    static HaveCakeAndEatCakeTooDomain()
    {
        ActionSchemas = new[]
        {
            Eat(C),
            Bake(C)
        }.AsQueryable();

        Constant cake = new(nameof(cake));

        ExampleProblem = MakeProblem(
            initialState: new HashSetState(Have(cake)),
            endGoal: new(Have(cake) & Eaten(cake)));
    }

    /// <summary>
    /// Gets the actions that are available in the domain.
    /// </summary>
    public static IQueryable<Action> ActionSchemas { get; }

    /// <summary>
    /// Gets an instance of the example problem for this domain.
    /// In the initial state, we Have(cake).
    /// The goal is to Have(cake) and Eaten(cake).
    /// </summary>
    public static Problem ExampleProblem { get; }

    public static OperablePredicate Have(Term @object) => new Predicate(nameof(Have), @object);

    public static OperablePredicate Eaten(Term @object) => new Predicate(nameof(Eaten), @object);

    public static Action Eat(Term @object) => new OperableAction(
        identifier: nameof(Eat),
        precondition: Have(@object),
        effect: !Have(@object) & Eaten(@object));

    public static Action Bake(Term @object) => new OperableAction(
        identifier: nameof(Bake),
        precondition: !Have(@object),
        effect: Have(@object));

    /// <summary>
    /// Creates a new <see cref="Problem"/> instance that refers to this domain.
    /// </summary>
    /// <param name="initialState">The initial state of the problem.</param>
    /// <param name="endGoal">The end goal of the problem.</param>
    /// <returns>A new <see cref="Problem"/> instance that refers to this domain.</returns>
    public static Problem MakeProblem(IState initialState, Goal endGoal) => new(initialState, endGoal, ActionSchemas);
}
