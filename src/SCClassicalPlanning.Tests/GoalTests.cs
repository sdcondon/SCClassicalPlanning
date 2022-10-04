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

        private record IsSatisfiedByTestCase(Goal Goal, State State, bool ExpectedResult);

        public static Test IsSatisfiedByBehaviour => TestThat
            .GivenEachOf(() => new IsSatisfiedByTestCase[]
            {
                // satisfied positive element
                new(
                    Goal: new(IsPresent(element1)),
                    State: new(IsPresent(element1)),
                    ExpectedResult: true),

                // satisfied negative element, present irrelevant element
                new(
                    Goal: new(!IsPresent(element1)),
                    State: new(IsPresent(element2)),
                    ExpectedResult: true),

                // satisfied positive & negative element
                new(
                    Goal: new(!IsPresent(element1) & IsPresent(element2)),
                    State: new(IsPresent(element2)),
                    ExpectedResult: true),

                // satisfied positive element, unsatisfied negative element
                new(
                    Goal: new(IsPresent(element1) & !IsPresent(element2)),
                    State: new(IsPresent(element1) & IsPresent(element2)),
                    ExpectedResult: false),

                // satisfied negative element, unsatisfied positive element
                new(
                    Goal: new(IsPresent(element1) & !IsPresent(element2)),
                    State: State.Empty,
                    ExpectedResult: false),
            })
            .When(tc => tc.Goal.IsSatisfiedBy(tc.State))
            .ThenReturns()
            .And((tc, r) => r.Should().Be(tc.ExpectedResult));

        private record EqualityTestCase(Goal X, Goal Y, bool ExpectedEquality);

        public static Test EqualityBehaviour => TestThat
            .GivenEachOf(() => new EqualityTestCase[]
            {
                new(
                    X: Goal.Empty,
                    Y: new Goal(),
                    ExpectedEquality: true),

                new(
                    X: new Goal(new Predicate("A"), new Predicate("B")),
                    Y: new Goal(new Predicate("A"), new Predicate("B")),
                    ExpectedEquality: true),

                new(
                    X: new Goal(new Predicate("A"), new Predicate("B")),
                    Y: new Goal(new Predicate("B"), new Predicate("A")),
                    ExpectedEquality: true),

                new(
                    X: new Goal(new Predicate("A"), new Predicate("B")),
                    Y: new Goal((Sentence)new Predicate("A")),
                    ExpectedEquality: false),
            })
            .When(tc => (Equality: tc.X.Equals(tc.Y), HashCodeEquality: tc.X.GetHashCode() == tc.Y.GetHashCode()))
            .ThenReturns()
            .And((tc, rv) => rv.Equality.Should().Be(tc.ExpectedEquality))
            .And((tc, rv) => rv.HashCodeEquality.Should().Be(tc.ExpectedEquality));
    }
}