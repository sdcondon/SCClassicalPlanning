using FluentAssertions;
using FlUnit;
using SCClassicalPlanning._TestUtilities;
using SCFirstOrderLogic;
using static SCClassicalPlanning.ExampleDomains.AsCode.BlocksWorldDomain;
using static SCFirstOrderLogic.SentenceCreation.OperableSentenceFactory;
using static SCClassicalPlanning.ProblemCreation.OperableProblemFactory;
using SCFirstOrderLogic.Inference;
using SCFirstOrderLogic.Inference.Resolution;

namespace SCClassicalPlanning.Planning.Utilities;

public static class InvariantInspectorTests
{
    private static readonly Constant blockA = new(nameof(blockA));
    private static readonly Constant blockB = new(nameof(blockB));

    private record TestCase(IKnowledgeBase KnowledgeBase, OperableGoal Goal, bool ExpectedResult);

    public static Test IsGoalPrecludedByInvariantsBehaviour => TestThat
        .GivenEachOf(() => new TestCase[]
        {
            new TestCase( // Empty goal
                KnowledgeBase: new ResolutionKnowledgeBase(new DelegateResolutionStrategy(
                    new HashSetClauseStore(),
                    DelegateResolutionStrategy.Filters.None,
                    DelegateResolutionStrategy.PriorityComparisons.UnitPreference)),
                Goal: Goal.Empty,
                ExpectedResult: false),

            new TestCase( // Trivial goal, invariants not violated
                KnowledgeBase: new ResolutionKnowledgeBase(new DelegateResolutionStrategy(
                    new HashSetClauseStore(new Sentence[]
                    {
                        Block(blockA)
                    }),
                    DelegateResolutionStrategy.Filters.None,
                    DelegateResolutionStrategy.PriorityComparisons.UnitPreference)),
                Goal: Block(blockA),
                ExpectedResult: false),

            new TestCase( // Trivial goal, invariant violated
                KnowledgeBase: new ResolutionKnowledgeBase(new DelegateResolutionStrategy(
                    new HashSetClauseStore(new Sentence[]
                    {
                        !Block(Table)
                    }),
                    DelegateResolutionStrategy.Filters.None,
                    DelegateResolutionStrategy.PriorityComparisons.UnitPreference)),
                Goal: Block(Table),
                ExpectedResult: true),

            new TestCase( // Non-trivial goal - invariants not violated
                KnowledgeBase: new ResolutionKnowledgeBase(new DelegateResolutionStrategy(
                    new HashSetClauseStore(new Sentence[]
                    {
                        ForAll(X, Y, If(On(X, Y), !Clear(Y))),
                    }),
                    DelegateResolutionStrategy.Filters.None,
                    DelegateResolutionStrategy.PriorityComparisons.UnitPreference)),
                Goal: On(blockB, blockA) & Clear(blockB),
                ExpectedResult: false),

            new TestCase( // Non-trivial goal - invariant violated
                KnowledgeBase: new ResolutionKnowledgeBase(new DelegateResolutionStrategy(
                    new HashSetClauseStore(new Sentence[]
                    {
                        ForAll(X, Y, If(On(X, Y), !Clear(Y))),
                    }),
                    DelegateResolutionStrategy.Filters.None,
                    DelegateResolutionStrategy.PriorityComparisons.UnitPreference)),
                Goal: On(blockA, blockB) & Clear(blockB),
                ExpectedResult: true),

            new TestCase( // Non-trivial goal - with variables
                KnowledgeBase: new ResolutionKnowledgeBase(new DelegateResolutionStrategy(
                    new HashSetClauseStore(new Sentence[]
                    {
                        ForAll(X, Y, If(On(X, Y), !Clear(Y))),
                    }),
                    DelegateResolutionStrategy.Filters.None,
                    DelegateResolutionStrategy.PriorityComparisons.UnitPreference)),
                Goal: On(X, Y) & Clear(Y),
                ExpectedResult: true),
        })
        .When(tc =>
        {
            return new InvariantInspector(tc.KnowledgeBase).IsGoalPrecludedByInvariants(tc.Goal);
        })
        .ThenReturns()
        .And((tc, rv) => rv.Should().Be(tc.ExpectedResult));
}
