using FluentAssertions;
using FlUnit;
using SCFirstOrderLogic;
using static SCClassicalPlanning.ExampleDomains.AsCode.BlocksWorldDomain;
using static SCFirstOrderLogic.SentenceCreation.OperableSentenceFactory;
using static SCClassicalPlanning.ProblemCreation.OperableProblemFactory;
using SCFirstOrderLogic.Inference.Resolution;

namespace SCClassicalPlanning.Planning.Utilities;

public static class InvariantInspectorTests
{
    private static readonly Constant blockA = new(nameof(blockA));
    private static readonly Constant blockB = new(nameof(blockB));

    private record TestCase(OperableGoal Goal, IEnumerable<Sentence> Knowledge, bool ExpectedResult);

    public static Test IsGoalPrecludedByInvariantsBehaviour => TestThat
        .GivenEachOf(() => new TestCase[]
        {
            new( // Empty goal
                Goal: Goal.Empty,
                Knowledge: [],
                ExpectedResult: false),

            new( // Trivial goal, invariants not violated
                Goal: Block(blockA),
                Knowledge:
                [
                    Block(blockA)
                ],
                ExpectedResult: false),

            new( // Trivial goal, invariant violated
                Goal: Block(Table),
                Knowledge:
                [
                    !Block(Table)
                ],
                ExpectedResult: true),

            new( // Non-trivial goal - invariants not violated
                Goal: On(blockB, blockA) & Clear(blockB),
                Knowledge:
                [
                    ForAll(X, Y, If(On(X, Y), !Clear(Y))),
                ],
                ExpectedResult: false),

            new( // Non-trivial goal - invariant violated
                Goal: On(blockA, blockB) & Clear(blockB),
                Knowledge:
                [
                    ForAll(X, Y, If(On(X, Y), !Clear(Y))),
                ],
                ExpectedResult: true),

            new( // Non-trivial goal - with variables
                Goal: On(X, Y) & Clear(Y),
                Knowledge:
                [
                    ForAll(X, Y, If(On(X, Y), !Clear(Y))),
                ],
                ExpectedResult: true),
        })
        .When(tc =>
        {
            var kb = new ResolutionKnowledgeBase(new DelegateResolutionStrategy(
                new HashSetClauseStore(tc.Knowledge.Select(s => (Sentence)s)),
                DelegateResolutionStrategy.Filters.None,
                DelegateResolutionStrategy.PriorityComparisons.UnitPreference));

            return new InvariantInspector(kb).IsGoalPrecludedByInvariants(tc.Goal);
        })
        .ThenReturns()
        .And((tc, rv) => rv.Should().Be(tc.ExpectedResult));
}
