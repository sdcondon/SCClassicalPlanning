using FluentAssertions;
using FlUnit;
using SCFirstOrderLogic;
using static SCClassicalPlanning.ExampleDomains.AirCargo;
using static SCClassicalPlanning.ExampleDomains.Container;
using static SCFirstOrderLogic.SentenceCreation.OperableSentenceFactory;

namespace SCClassicalPlanning
{
    public static class EffectTests
    {
        private static readonly Constant element1 = new(nameof(element1));
        private static readonly Constant element2 = new(nameof(element2));

        private record ApplyToTestCase(State State, Effect Effect, State ExpectedResult);

        public static Test ApplyToBehaviour => TestThat
            .GivenEachOf(() => new ApplyToTestCase[]
            {
                // Adds atom
                new(
                    State: State.Empty,
                    Effect: new(IsPresent(element1)),
                    ExpectedResult: new(IsPresent(element1))),

                // Removes atom
                new(
                    State: new(IsPresent(element1)),
                    Effect: new(!IsPresent(element1)),
                    ExpectedResult: State.Empty),

                // Doesn't add duplicate
                new(
                    State: new(IsPresent(element1)),
                    Effect: new(IsPresent(element1)),
                    ExpectedResult: new(IsPresent(element1))),

                // Doesn't complain about removing non-present element
                new(
                    State: State.Empty,
                    Effect: new(!IsPresent(element1)),
                    ExpectedResult: State.Empty),
            })
            .When(tc => tc.Effect.ApplyTo(tc.State))
            .ThenReturns()
            .And((tc, s) => s.Should().BeEquivalentTo(tc.ExpectedResult));

        private record IsRelevantToTestCase(Goal Goal, Effect Effect, bool ExpectedResult);

        public static Test IsRelevantToBehaviour => TestThat
            .GivenEachOf(() => new IsRelevantToTestCase[]
            {
                // fulfills positive element
                new(
                    Goal: new(IsPresent(element1)),
                    Effect: new(IsPresent(element1)),
                    ExpectedResult: true),

                // fulfills negative element
                new(
                    Goal: new(!IsPresent(element1)),
                    Effect: new(!IsPresent(element1)),
                    ExpectedResult: true),

                // fulfills positive & negative element
                new(
                    Goal: new(!IsPresent(element1) & IsPresent(element2)),
                    Effect: new(!IsPresent(element1) & IsPresent(element2)),
                    ExpectedResult: true),

                // doesn't fulfill any elements
                new(
                    Goal: new(!IsPresent(element1) & IsPresent(element2)),
                    Effect: new(IsPresent(element1) & !IsPresent(element2)),
                    ExpectedResult: false),

                // fulfills positive element, undoes positive element
                new(
                    Goal: new(IsPresent(element1) & IsPresent(element2)),
                    Effect: new(!IsPresent(element1) & IsPresent(element2)),
                    ExpectedResult: false),

                // Variable doesn't confuse matters..
                new(
                    Goal: new(At(new Constant("C2"), new Constant("JFK"))),
                    Effect: new(At(new Constant("C2"), new Constant("JFK")) & !In(new Constant("C2"), P)),
                    ExpectedResult: true),
            })
            .When(tc => tc.Effect.IsRelevantTo(tc.Goal))
            .ThenReturns()
            .And((tc, r) => r.Should().Be(tc.ExpectedResult));

        private record EqualityTestCase(Effect X, Effect Y, bool ExpectedEquality);

        public static Test EqualityBehaviour => TestThat
            .GivenEachOf(() => new EqualityTestCase[]
            {
                new(
                    X: Effect.Empty,
                    Y: new Effect(),
                    ExpectedEquality: true),

                new(
                    X: new Effect(new Predicate("A"), new Predicate("B")),
                    Y: new Effect(new Predicate("A"), new Predicate("B")),
                    ExpectedEquality: true),

                new(
                    X: new Effect(new Predicate("A"), new Predicate("B")),
                    Y: new Effect(new Predicate("B"), new Predicate("A")),
                    ExpectedEquality: true),

                new(
                    X: new Effect(new Predicate("A"), new Predicate("B")),
                    Y: new Effect((Sentence)new Predicate("A")),
                    ExpectedEquality: false),
            })
            .When(tc => (Equality: tc.X.Equals(tc.Y), HashCodeEquality: tc.X.GetHashCode() == tc.Y.GetHashCode()))
            .ThenReturns()
            .And((tc, rv) => rv.Equality.Should().Be(tc.ExpectedEquality))
            .And((tc, rv) => rv.HashCodeEquality.Should().Be(tc.ExpectedEquality));
    }
}