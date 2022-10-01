using FluentAssertions;
using FlUnit;
using SCFirstOrderLogic;
using static SCClassicalPlanning.ExampleDomains.Container;
using static SCFirstOrderLogic.SentenceCreation.OperableSentenceFactory;

namespace SCClassicalPlanning
{
    public static class ActionTests
    {
        private static readonly OperableVariableReference element1 = new VariableReference(nameof(element1));
        private static readonly OperableVariableReference element2 = new VariableReference(nameof(element2));

        private record IsApplicableToTestCase(State initialState, Action action, bool expectedResult);

        public static Test IsApplicableToBehaviour => TestThat
            .GivenEachOf(() => new IsApplicableToTestCase[]
            {
                // Positive - positive precondition
                new(
                    initialState: new(IsPresent(element1)),
                    action: Remove(element1),
                    expectedResult: true),

                // Positive - negative precondition
                new(
                    initialState: State.Empty,
                    action: Add(element1),
                    expectedResult: true),

                // Negative - positive precondition
                new(
                    initialState: new(IsPresent(element1)),
                    action: Add(element1),
                    expectedResult: false),

                // Negative - negative precondition
                new(
                    initialState: State.Empty,
                    action: Remove(element1),
                    expectedResult: false),
            })
            .When(tc => tc.action.IsApplicableTo(tc.initialState))
            .ThenReturns()
            .And((tc, r) => r.Should().Be(tc.expectedResult));

        private record ApplyToTestCase(State initialState, Action action, State expectedState);

        public static Test ApplyToBehaviour => TestThat
            .GivenEachOf(() => new ApplyToTestCase[]
            {
                // Adds atom
                new(
                    initialState: State.Empty,
                    action: Add(element1),
                    expectedState: new(IsPresent(element1))),

                // Removes atom
                new(
                    initialState: new(IsPresent(element1)),
                    action: Remove(element1),
                    expectedState: State.Empty),

                // Doesn't add duplicate
                new(
                    initialState: new(IsPresent(element1)),
                    action: Add(element1),
                    expectedState: new(IsPresent(element1))),

                // Doesn't complain about removing non-present element
                new(
                    initialState: State.Empty,
                    action: Remove(element1),
                    expectedState: State.Empty),
            })
            .When(tc => tc.action.ApplyTo(tc.initialState))
            .ThenReturns()
            .And((tc, s) => s.Should().BeEquivalentTo(tc.expectedState));

        private record RegressTestCase(Goal goal, Action action, Goal expectedResult);

        public static Test RegressBehaviour => TestThat
            .GivenEachOf(() => new RegressTestCase[]
            {
                // Adds atom
                new(
                    goal: new(IsPresent(element1)),
                    action: Add(element1),
                    expectedResult: new(!IsPresent(element1))),

                // Removes atom
                new(
                    goal: new(!IsPresent(element1)),
                    action: Remove(element1),
                    expectedResult: new(IsPresent(element1))),

                // Swap
                new(
                    goal: new(!IsPresent(element1) & IsPresent(element2)),
                    action: Swap(element1, element2),
                    expectedResult: new(IsPresent(element1) & !IsPresent(element2))),

                // Swap - partial 1
                new(
                    goal: new(!IsPresent(element1)),
                    action: Swap(element1, element2),
                    expectedResult: new(IsPresent(element1) & !IsPresent(element2))),

                // Swap - partial 2
                new(
                    goal: new(IsPresent(element2)),
                    action: Swap(element1, element2),
                    expectedResult: new(IsPresent(element1) & !IsPresent(element2)))
            })
            .When(tc => tc.action.Regress(tc.goal))
            .ThenReturns()
            .And((tc, s) => s.Should().BeEquivalentTo(tc.expectedResult));
    }
}