using FluentAssertions;
using FlUnit;
using SCClassicalPlanning.ExampleDomains.FromAIaMA;
using SCFirstOrderLogic;
using SCFirstOrderLogic.Inference;
using SCFirstOrderLogic.Inference.Resolution;
using static SCClassicalPlanning.ExampleDomains.FromAIaMA.BlocksWorld;
using static SCClassicalPlanning.ProblemCreation.OperableProblemFactory;
using static SCFirstOrderLogic.SentenceCreation.OperableSentenceFactory;

namespace SCClassicalPlanning.Planning.StateSpaceSearch.Heuristics
{
    public static class GoalInvariantCheckTests
    {
        private static readonly Constant blockA = new(nameof(blockA));
        private static readonly Constant blockB = new(nameof(blockB));

        private record TestCase(IEnumerable<Sentence> Invariants, OperableState State, OperableGoal Goal, float ExpectedCost);

        public static Test EstimateCostBehaviour => TestThat
            .GivenTestContext()
            .AndEachOf(() => new TestCase[]
            {
                new TestCase(
                    Invariants: new Sentence[] { Block(blockA), ForAll(A, B, If(On(A, B), !Clear(B))) },
                    State: BlocksWorld.ExampleProblem.InitialState,
                    Goal: Goal.Empty, // Fine
                    ExpectedCost: 0),

                new TestCase(
                    Invariants: new Sentence[] { Block(blockA), ForAll(A, B, If(On(A, B), !Clear(B))) },
                    State: BlocksWorld.ExampleProblem.InitialState,
                    Goal: Block(Table), // Fine
                    ExpectedCost: 0),

                new TestCase(
                    Invariants: new Sentence[] { Block(blockA), ForAll(A, B, If(On(A, B), !Clear(B))) },
                    State: BlocksWorld.ExampleProblem.InitialState,
                    Goal: !Block(blockA), // Violates Block(blockA)
                    ExpectedCost: float.PositiveInfinity),

                new TestCase(
                    Invariants: new Sentence[] { Block(blockA), ForAll(A, B, If(On(A, B), !Clear(B))) },
                    State: BlocksWorld.ExampleProblem.InitialState,
                    Goal: On(blockA, blockB) & Clear(blockB), // Violates on/clear relationship
                    ExpectedCost: float.PositiveInfinity),

                new TestCase(
                    Invariants: new Sentence[] { Block(blockA), ForAll(A, B, If(On(A, B), !Clear(B))) },
                    State: BlocksWorld.ExampleProblem.InitialState,
                    Goal: On(blockB, blockA) & Clear(blockB), // Fine
                    ExpectedCost: 0),
            })
            .When((_, tc) =>
            {
                var kb = new SimpleResolutionKnowledgeBase(
                    new SimpleClauseStore(),
                    SimpleResolutionKnowledgeBase.Filters.None,
                    SimpleResolutionKnowledgeBase.PriorityComparisons.None); // No point in unitpreference, 'cos query is *all* unit clauses..
                kb.Tell(tc.Invariants);

                return new GoalInvariantCheck(kb, new DelegateHeuristic((s, g) => 0)).EstimateCost(tc.State, tc.Goal);
            })
            .ThenReturns()
            .And((_, tc, rv) => rv.Should().Be(tc.ExpectedCost));
    }
}
