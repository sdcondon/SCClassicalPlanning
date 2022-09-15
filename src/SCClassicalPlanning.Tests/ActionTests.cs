using FluentAssertions;
using FlUnit;
using static SCClassicalPlanning.ExampleProblems.Container;

namespace SCClassicalPlanning
{
    public static class ActionTests
    {
        private record IsApplicableToTestCase(State initialState, Action action, bool expectedResult);

        public static Test IsApplicableToBehaviour => TestThat
            .GivenEachOf(() => new IsApplicableToTestCase[]
            {
                // Positive - positive precond
                new(
                    initialState: IsPresent(Element1),
                    action: Remove(Element1),
                    expectedResult: true),

                // Positive - explicit negative
                new(
                    initialState: !IsPresent(Element1),
                    action: Add(Element1),
                    expectedResult: true),

                // Positive - implicit negative
                new(
                    initialState: State.Empty,
                    action: Add(Element1),
                    expectedResult: true),

                // Negative - Positive precond
                new(
                    initialState: IsPresent(Element1),
                    action: Add(Element1),
                    expectedResult: false),

                // Negative - explicit negative
                new(
                    initialState: !IsPresent(Element1),
                    action: Remove(Element1),
                    expectedResult: false),

                // Negative - implicit negative
                new(
                    initialState: State.Empty,
                    action: Remove(Element1),
                    expectedResult: false),
            })
            .When(tc => tc.action.ApplyTo(tc.initialState))
            .ThenReturns()
            .And((tc, r) => r.Should().Be(tc.expectedResult));

        private record ApplyToTestCase(State initialState, Action action, State expectedState);

        public static Test ApplyToBehaviour => TestThat
            .GivenEachOf(() => new ApplyToTestCase[]
            {
                // Adds atom
                new(
                    initialState: State.Empty,
                action: Add(Element1),
                    expectedState: IsPresent(Element1)),

                // Removes atom
                new(
                initialState: IsPresent(Element1),
                    action: Remove(Element1),
                    expectedState: State.Empty),

                // Doesn't add duplicate
                new(
                initialState: IsPresent(Element1),
                action: Add(Element1),
                    expectedState: IsPresent(Element1)),

                // Doesn't complain about removing non-present element
                new(
                initialState: State.Empty,
                    action: Remove(Element1),
                    expectedState: State.Empty),
            })
            .When(tc => tc.action.ApplyTo(tc.initialState))
            .ThenReturns()
            .And((tc, s) => s.Should().BeEquivalentTo(tc.expectedState));
    }
}