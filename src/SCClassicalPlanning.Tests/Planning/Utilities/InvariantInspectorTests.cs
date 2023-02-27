using FluentAssertions;
using FlUnit;
using SCClassicalPlanning._TestUtilities;
using SCFirstOrderLogic;
using static SCClassicalPlanning.ExampleDomains.FromAIaMA.BlocksWorld;
using static SCClassicalPlanning.ProblemCreation.OperableProblemFactory;

namespace SCClassicalPlanning.Planning.Utilities
{
    public static class InvariantInspectorTests
    {
        private static readonly Constant blockA = new(nameof(blockA));
        private static readonly Constant blockB = new(nameof(blockB));

        private record TestCase(Dictionary<Sentence, bool> KnowledgeBaseResponses, OperableGoal Goal, bool ExpectedResult);

        public static Test IsGoalPrecludedByInvariantsBehaviour => TestThat
            .GivenEachOf(() => new TestCase[]
            {
                new TestCase( // Empty goal
                    KnowledgeBaseResponses: new(),
                    Goal: Goal.Empty,
                    ExpectedResult: false),

                new TestCase( // Trivial goal, invariants not violated
                    KnowledgeBaseResponses: new() { [!Block(blockA)] = false },
                    Goal: Block(blockA),
                    ExpectedResult: false),

                new TestCase( // Trivial goal, invariant violated
                    KnowledgeBaseResponses: new() { [!Block(Table)] = true },
                    Goal: Block(Table),
                    ExpectedResult: true),

                new TestCase( // Non-trivial goal - invariants not violated
                    KnowledgeBaseResponses: new() { [!(On(blockB, blockA) & Clear(blockB))] = false },
                    Goal: On(blockB, blockA) & Clear(blockB),
                    ExpectedResult: false),

                new TestCase( // Non-trivial goal - invariant violated
                    KnowledgeBaseResponses: new() { [!(On(blockA, blockB) & Clear(blockB))] = true },
                    Goal: On(blockA, blockB) & Clear(blockB),
                    ExpectedResult: true),
            })
            .When(tc =>
            {
                var kb = new MockKnowledgeBase(tc.KnowledgeBaseResponses);
                return new InvariantInspector(kb).IsGoalPrecludedByInvariants(tc.Goal);
            })
            .ThenReturns()
            .And((tc, rv) => rv.Should().Be(tc.ExpectedResult));
    }
}
