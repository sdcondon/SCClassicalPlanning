using FluentAssertions;
using FlUnit;
using SCFirstOrderLogic;

namespace SCClassicalPlanning
{
    public static class GoalTests
    {
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