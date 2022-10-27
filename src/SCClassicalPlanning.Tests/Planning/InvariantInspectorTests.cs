using FluentAssertions;
using FlUnit;
using SCFirstOrderLogic;
using SCFirstOrderLogic.Inference;
using SCFirstOrderLogic.Inference.Resolution;
using static SCClassicalPlanning.ExampleDomains.FromAIaMA.BlocksWorld;
using static SCClassicalPlanning.ProblemCreation.OperableProblemFactory;
using static SCFirstOrderLogic.SentenceCreation.OperableSentenceFactory;

namespace SCClassicalPlanning.Planning.StateSpaceSearch.Heuristics
{
    public static class InvariantInspectorTests
    {
        private static readonly Constant blockA = new(nameof(blockA));
        private static readonly Constant blockB = new(nameof(blockB));

        private record TestCase(IEnumerable<Sentence> Invariants, OperableGoal Goal, bool ExpectedResult);

        public static Test EstimateCostBehaviour => TestThat
            .GivenEachOf(() => new TestCase[]
            {
                new TestCase(
                    Invariants: new Sentence[] { Block(blockA), ForAll(A, B, If(On(A, B), !Clear(B))) },
                    Goal: Goal.Empty, // Fine
                    ExpectedResult: false),

                new TestCase(
                    Invariants: new Sentence[] { Block(blockA), ForAll(A, B, If(On(A, B), !Clear(B))) },
                    Goal: Block(Table), // Fine
                    ExpectedResult: false),

                new TestCase(
                    Invariants: new Sentence[] { Block(blockA), ForAll(A, B, If(On(A, B), !Clear(B))) },
                    Goal: !Block(blockA), // Violates Block(blockA)
                    ExpectedResult: true),

                new TestCase(
                    Invariants: new Sentence[] { Block(blockA), ForAll(A, B, If(On(A, B), !Clear(B))) },
                    Goal: On(blockA, blockB) & Clear(blockB), // Violates on/clear relationship
                    ExpectedResult: true),

                new TestCase(
                    Invariants: new Sentence[] { Block(blockA), ForAll(A, B, If(On(A, B), !Clear(B))) },
                    Goal: On(blockB, blockA) & Clear(blockB), // Fine
                    ExpectedResult: false),
            })
            .When(tc =>
            {
                var kb = new SimpleResolutionKnowledgeBase(
                    new SimpleClauseStore(),
                    SimpleResolutionKnowledgeBase.Filters.None,
                    SimpleResolutionKnowledgeBase.PriorityComparisons.None); // No point in unitpreference, 'cos query is *all* unit clauses..
                kb.Tell(tc.Invariants);

                return InvariantInspector.IsGoalPrecludedByInvariants(tc.Goal, kb);
            })
            .ThenReturns()
            .And((tc, rv) => rv.Should().Be(tc.ExpectedResult));
    }
}
