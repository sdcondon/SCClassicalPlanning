using FluentAssertions;
using FlUnit;
using SCFirstOrderLogic;
using SCFirstOrderLogic.Inference;
using SCFirstOrderLogic.Inference.Resolution;
using static SCClassicalPlanning.ExampleDomains.FromAIaMA.BlocksWorld;
using static SCClassicalPlanning.ProblemCreation.OperableProblemFactory;
using static SCFirstOrderLogic.SentenceCreation.OperableSentenceFactory;

namespace SCClassicalPlanning.Planning.Utilities
{
    public static class InvariantInspectorTests
    {
        private static readonly Constant blockA = new(nameof(blockA));
        private static readonly Constant blockB = new(nameof(blockB));

        private record TestCase(IEnumerable<Sentence> Invariants, OperableGoal Goal, bool ExpectedResult);

        public static Test IsGoalPrecludedByInvariantsBehaviour => TestThat
            .GivenEachOf(() => new TestCase[]
            {
                new TestCase( // Empty goal - always fine
                    Invariants: new Sentence[] { Block(blockA), ForAll(A, B, If(On(A, B), !Clear(B))) },
                    Goal: Goal.Empty,
                    ExpectedResult: false),

                new TestCase( // Element always true - fine
                    Invariants: new Sentence[] { Block(blockA), ForAll(A, B, If(On(A, B), !Clear(B))) },
                    Goal: Block(Table),
                    ExpectedResult: false),

                new TestCase( // Violates Block(blockA)
                    Invariants: new Sentence[] { Block(blockA), ForAll(A, B, If(On(A, B), !Clear(B))) },
                    Goal: !Block(blockA),
                    ExpectedResult: true),

                new TestCase( // Violates on/clear relationship
                    Invariants: new Sentence[] { Block(blockA), ForAll(A, B, If(On(A, B), !Clear(B))) },
                    Goal: On(blockA, blockB) & Clear(blockB),
                    ExpectedResult: true),

                new TestCase( // on/clear relationship is relevant but not violated - fine
                    Invariants: new Sentence[] { Block(blockA), ForAll(A, B, If(On(A, B), !Clear(B))) },
                    Goal: On(blockB, blockA) & Clear(blockB),
                    ExpectedResult: false),
            })
            .When(tc =>
            {
                var kb = new SimpleResolutionKnowledgeBase(
                    new SimpleClauseStore(),
                    SimpleResolutionKnowledgeBase.Filters.None,
                    SimpleResolutionKnowledgeBase.PriorityComparisons.None); // No point in unitpreference, 'cos query is *all* unit clauses..
                kb.Tell(tc.Invariants);

                return new InvariantInspector(kb).IsGoalPrecludedByInvariants(tc.Goal);
            })
            .ThenReturns()
            .And((tc, rv) => rv.Should().Be(tc.ExpectedResult));
    }
}
