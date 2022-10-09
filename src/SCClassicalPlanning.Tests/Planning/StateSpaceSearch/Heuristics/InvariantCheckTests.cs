using FluentAssertions;
using FlUnit;
using SCClassicalPlanning.ExampleDomains.FromAIaMA;
using SCClassicalPlanning.Planning.StateSpaceSearch.Heuristics;
using SCFirstOrderLogic;
using SCFirstOrderLogic.Inference;
using SCFirstOrderLogic.Inference.Resolution;
using static SCClassicalPlanning.ExampleDomains.FromAIaMA.AirCargo;
using static SCClassicalPlanning.ExampleDomains.FromAIaMA.BlocksWorld;
using static SCClassicalPlanning.ProblemCreation.OperableProblemFactory;
using static SCFirstOrderLogic.SentenceCreation.OperableSentenceFactory;

namespace SCClassicalPlanning.Planning.StateSpaceSearch
{
    public static class InvariantCheckTests
    {
        private static readonly Constant blockA = new(nameof(blockA));
        private static readonly Constant blockB = new(nameof(blockB));
        private static readonly Constant blockC = new(nameof(blockC));

        private static readonly State BlocksWorldInitialState = new(
            Block(blockA)
            & Equal(blockA, blockA)
            & Block(blockB)
            & Equal(blockB, blockB)
            & Block(blockC)
            & Equal(blockC, blockC)
            & On(blockA, Table)
            & On(blockB, Table)
            & On(blockC, blockA)
            & Clear(blockB)
            & Clear(blockC));

        private record TestCase(IEnumerable<Sentence> Invariants, State State, OperableGoal Goal, float ExpectedCost);

        public static Test EstimateCostBehaviour => TestThat
            .GivenTestContext()
            .AndEachOf(() => new TestCase[]
            {
                new TestCase(
                    Invariants: new Sentence[] { Block(blockA), ForAll(A, B, If(On(A, B), !Clear(B))) },
                    State: BlocksWorldInitialState,
                    Goal: Block(Table), // Fine - invariants don't rule this out
                    ExpectedCost: 0),

                new TestCase(
                    Invariants: new Sentence[] { Block(blockA), ForAll(A, B, If(On(A, B), !Clear(B))) },
                    State: BlocksWorldInitialState,
                    Goal: !Block(blockA), // Contradicts Block(blockA)
                    ExpectedCost: float.PositiveInfinity),

                new TestCase(
                    Invariants: new Sentence[] { Block(blockA), ForAll(A, B, If(On(A, B), !Clear(B))) },
                    State: BlocksWorldInitialState,
                    Goal: On(blockA, blockB) & Clear(blockB), // Nope - violates on/clear relationship
                    ExpectedCost: float.PositiveInfinity),

                new TestCase(
                    Invariants: new Sentence[] { Block(blockA), ForAll(A, B, If(On(A, B), !Clear(B))) },
                    State: BlocksWorldInitialState,
                    Goal: On(blockB, blockA) & Clear(blockB), // Fine
                    ExpectedCost: 0)
            })
            .When((_, tc) =>
            {
                var kb = new SimpleResolutionKnowledgeBase(
                    new SimpleClauseStore(),
                    SimpleResolutionKnowledgeBase.Filters.None,
                    SimpleResolutionKnowledgeBase.PriorityComparisons.UnitPreference);
                kb.Tell(tc.Invariants);

                return new InvariantCheck(kb, (s, g) => 0).EstimateCost(tc.State, tc.Goal);
            })
            .ThenReturns()
            .And((_, tc, rv) => rv.Should().Be(tc.ExpectedCost));
    }
}
