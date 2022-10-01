using FluentAssertions;
using FlUnit;
using SCFirstOrderLogic;
using static SCClassicalPlanning.ExampleDomains.Container;

namespace SCClassicalPlanning
{
    public static class GoalTests
    {
        private static readonly Constant element1 = new(nameof(element1));
        private static readonly Constant element2 = new(nameof(element2));

        private record IsSatisfiedByTestCase(Goal goal, State state, bool expectedResult);

        public static Test IsSatisfiedByBehaviour => TestThat
            .GivenEachOf(() => new IsSatisfiedByTestCase[]
            {
                // satisfied positive element
                new(
                    goal: new(IsPresent(element1)),
                    state: new(IsPresent(element1)),
                    expectedResult: true),

                // satisfied negative element, present irrelevant element
                new(
                    goal: new(!IsPresent(element1)),
                    state: new(IsPresent(element2)),
                    expectedResult: true),

                // satisfied positive & negative element
                new(
                    goal: new(!IsPresent(element1) & IsPresent(element2)),
                    state: new(IsPresent(element2)),
                    expectedResult: true),

                // satisfied positive element, unsatisfied negative element
                new(
                    goal: new(IsPresent(element1) & !IsPresent(element2)),
                    state: new(IsPresent(element1) & IsPresent(element2)),
                    expectedResult: false),

                // satisfied negative element, unsatisfied positive element
                new(
                    goal: new(IsPresent(element1) & !IsPresent(element2)),
                    state: State.Empty,
                    expectedResult: false),
            })
            .When(tc => tc.goal.IsSatisfiedBy(tc.state))
            .ThenReturns()
            .And((tc, r) => r.Should().Be(tc.expectedResult));
    }
}