using FluentAssertions;
using FlUnit;
using SCFirstOrderLogic;
using static SCClassicalPlanning.ExampleDomains.Container;

namespace SCClassicalPlanning
{
    public static class ActionTests
    {
        private static readonly Constant element1 = new(nameof(element1));
        private static readonly Constant element2 = new(nameof(element2));

        private record IsApplicableToTestCase(State State, Action Action, bool ExpectedResult);

        public static Test IsApplicableToBehaviour => TestThat
            .GivenEachOf<IsApplicableToTestCase>(() => (new IsApplicableToTestCase[]
            {
                // Positive - positive precondition
                new(
                    State: new(IsPresent(element1)),
                    Action: Remove(element1),
                    ExpectedResult: true),

                // Positive - negative precondition
                new(
                    State: State.Empty,
                    Action: Add(element1),
                    ExpectedResult: true),

                // Negative - positive precondition
                new(
                    State: new(IsPresent(element1)),
                    Action: Add(element1),
                    ExpectedResult: false),

                // Negative - negative precondition
                new(
                    State: State.Empty,
                    Action: Remove(element1),
                    ExpectedResult: false),
            }))
            .When(tc => tc.Action.IsApplicableTo(tc.State))
            .ThenReturns()
            .And((tc, r) => r.Should().Be(tc.ExpectedResult));

        private record ApplyToTestCase(State State, Action Action, State ExpectedState);

        public static Test ApplyToBehaviour => TestThat
            .GivenEachOf<ApplyToTestCase>(() => (new ApplyToTestCase[]
            {
                // Adds atom
                new(
                    State: State.Empty,
                    Action: Add(element1),
                    ExpectedState: new(IsPresent(element1))),

                // Removes atom
                new(
                    State: new(IsPresent(element1)),
                    Action: Remove(element1),
                    ExpectedState: State.Empty),

                // Doesn't add duplicate
                new(
                    State: new(IsPresent(element1)),
                    Action: Add(element1),
                    ExpectedState: new(IsPresent(element1))),

                // Doesn't complain about removing non-present element
                new(
                    State: State.Empty,
                    Action: Remove(element1),
                    ExpectedState: State.Empty),
            }))
            .When(tc => tc.Action.ApplyTo(tc.State))
            .ThenReturns()
            .And((tc, s) => s.Should().BeEquivalentTo(tc.ExpectedState));

        private record IsRelevantToTestCase(Goal Goal, Action Action, bool ExpectedResult);

        public static Test IsRelevantToBehaviour => TestThat
            .GivenEachOf(() => new IsRelevantToTestCase[]
            {
                // fulfills positive element
                new(
                    Goal: new(IsPresent(element1)),
                    Action: Add(element1),
                    ExpectedResult: true),

                // fulfills negative element
                new(
                    Goal: new(!IsPresent(element1)),
                    Action: Remove(element1),
                    ExpectedResult: true),

                // fulfills positive & negative element
                new(
                    Goal: new(!IsPresent(element1) & IsPresent(element2)),
                    Action: Swap(element1, element2),
                    ExpectedResult: true),

                // fulfills positive element, undoes positive element
                new(
                    Goal: new(IsPresent(element1) & IsPresent(element2)),
                    Action: Swap(element1, element2),
                    ExpectedResult: false),
            })
            .When(tc => tc.Action.IsRelevantTo(tc.Goal))
            .ThenReturns()
            .And((tc, r) => r.Should().Be(tc.ExpectedResult));

        private record RegressTestCase(Goal Goal, Action Action, Goal ExpectedResult);

        public static Test RegressBehaviour => TestThat
            .GivenEachOf(() => new RegressTestCase[]
            {
                // Adds atom
                new(
                    Goal: new(IsPresent(element1)),
                    Action: Add(element1),
                    ExpectedResult: new(!IsPresent(element1))),

                // Removes atom
                new(
                    Goal: new(!IsPresent(element1)),
                    Action: Remove(element1),
                    ExpectedResult: new(IsPresent(element1))),

                // Swap
                new(
                    Goal: new(!IsPresent(element1) & IsPresent(element2)),
                    Action: Swap(element1, element2),
                    ExpectedResult: new(IsPresent(element1) & !IsPresent(element2))),

                // Swap - partial 1
                new(
                    Goal: new(!IsPresent(element1)),
                    Action: Swap(element1, element2),
                    ExpectedResult: new(IsPresent(element1) & !IsPresent(element2))),

                // Swap - partial 2
                new(
                    Goal: new(IsPresent(element2)),
                    Action: Swap(element1, element2),
                    ExpectedResult: new(IsPresent(element1) & !IsPresent(element2)))
            })
            .When(tc => tc.Action.Regress(tc.Goal))
            .ThenReturns()
            .And((tc, s) => s.Should().BeEquivalentTo(tc.ExpectedResult));
    }
}