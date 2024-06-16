using FluentAssertions;
using FlUnit;
using SCClassicalPlanning.ExampleDomains.AsCode;
using SCFirstOrderLogic;
using SCFirstOrderLogic.Inference;
using SCFirstOrderLogic.Inference.Resolution;
using static SCClassicalPlanning.ExampleDomains.AsCode.BlocksWorldDomain;
using static SCClassicalPlanning.ProblemCreation.OperableProblemFactory;
using static SCFirstOrderLogic.SentenceCreation.OperableSentenceFactory;

namespace SCClassicalPlanning.Planning.StateAndGoalSpace.CostStrategies;

public static class GoalInvariantCheckTests
{
    private static readonly Constant blockA = new(nameof(blockA));
    private static readonly Constant blockB = new(nameof(blockB));

    private record TestCase(IEnumerable<Sentence> Invariants, IState State, OperableGoal Goal, float ExpectedCost);

    public static Test EstimateCostBehaviour => TestThat
        .GivenEachOf(() => new TestCase[]
        {
            new TestCase(
                Invariants: new Sentence[] { Block(blockA), ForAll(A, B, If(On(A, B), !Clear(B))) },
                State: BlocksWorldDomain.ExampleProblem.InitialState,
                Goal: Goal.Empty, // Fine
                ExpectedCost: 0),

            new TestCase(
                Invariants: new Sentence[] { Block(blockA), ForAll(A, B, If(On(A, B), !Clear(B))) },
                State: BlocksWorldDomain.ExampleProblem.InitialState,
                Goal: Block(Table), // Fine
                ExpectedCost: 0),

            new TestCase(
                Invariants: new Sentence[] { Block(blockA), ForAll(A, B, If(On(A, B), !Clear(B))) },
                State: BlocksWorldDomain.ExampleProblem.InitialState,
                Goal: !Block(blockA), // Violates Block(blockA)
                ExpectedCost: float.PositiveInfinity),

            new TestCase(
                Invariants: new Sentence[] { Block(blockA), ForAll(A, B, If(On(A, B), !Clear(B))) },
                State: BlocksWorldDomain.ExampleProblem.InitialState,
                Goal: On(blockA, blockB) & Clear(blockB), // Violates on/clear relationship
                ExpectedCost: float.PositiveInfinity),

            new TestCase(
                Invariants: new Sentence[] { Block(blockA), ForAll(A, B, If(On(A, B), !Clear(B))) },
                State: BlocksWorldDomain.ExampleProblem.InitialState,
                Goal: On(blockB, blockA) & Clear(blockB), // Fine
                ExpectedCost: 0),
        })
        .When(tc =>
        {
            var kb = new ResolutionKnowledgeBase(new DelegateResolutionStrategy(
                new HashSetClauseStore(),
                DelegateResolutionStrategy.Filters.None,
                DelegateResolutionStrategy.PriorityComparisons.None)); // No point in unitpreference, 'cos query is *all* unit clauses..
            kb.Tell(tc.Invariants);

            var nullStrategy = new DelegateCostStrategy(a => 0f, (s, g) => 0f);
            return new GoalInvariantCheck(kb, nullStrategy).EstimateCost(tc.State, tc.Goal);
        })
        .ThenReturns()
        .And((tc, rv) => rv.Should().Be(tc.ExpectedCost));
}
