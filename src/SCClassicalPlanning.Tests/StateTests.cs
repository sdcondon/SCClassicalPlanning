using FluentAssertions;
using FlUnit;
using SCFirstOrderLogic;

namespace SCClassicalPlanning
{
    public static class StateTests
    {
        private record EqualityTestCase(State X, State Y, bool ExpectedEquality);

        public static Test EqualityBehaviour => TestThat
            .GivenEachOf(() => new EqualityTestCase[]
            {
                new(
                    X: State.Empty,
                    Y: new State(),
                    ExpectedEquality: true),

                new(
                    X: new State(new Predicate("A"), new Predicate("B")),
                    Y: new State(new Predicate("A"), new Predicate("B")),
                    ExpectedEquality: true),

                new(
                    X: new State(new Predicate("A"), new Predicate("B")),
                    Y: new State(new Predicate("B"), new Predicate("A")),
                    ExpectedEquality: true),

                new(
                    X: new State(new Predicate("A"), new Predicate("B")),
                    Y: new State(new Predicate("A")),
                    ExpectedEquality: false),
            })
            .When(tc => (Equality: tc.X.Equals(tc.Y), HashCodeEquality: tc.X.GetHashCode() == tc.Y.GetHashCode()))
            .ThenReturns()
            .And((tc, rv) => rv.Equality.Should().Be(tc.ExpectedEquality))
            .And((tc, rv) => rv.HashCodeEquality.Should().Be(tc.ExpectedEquality));
    }
}
