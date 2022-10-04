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

        private record ApplyToTestCase(State initialState, Effect effect, State expectedState);

        public static Test ApplyToBehaviour => TestThat
            .GivenEachOf(() => new ApplyToTestCase[]
            {
                // Adds atom
                new(
                    initialState: State.Empty,
                    effect: new(IsPresent(element1)),
                    expectedState: new(IsPresent(element1))),

                // Removes atom
                new(
                    initialState: new(IsPresent(element1)),
                    effect: new(!IsPresent(element1)),
                    expectedState: State.Empty),

                // Doesn't add duplicate
                new(
                    initialState: new(IsPresent(element1)),
                    effect: new(IsPresent(element1)),
                    expectedState: new(IsPresent(element1))),

                // Doesn't complain about removing non-present element
                new(
                    initialState: State.Empty,
                    effect: new(!IsPresent(element1)),
                    expectedState: State.Empty),
            })
            .When(tc => tc.effect.ApplyTo(tc.initialState))
            .ThenReturns()
            .And((tc, s) => s.Should().BeEquivalentTo(tc.expectedState));

        private record IsRelevantToTestCase(Goal goal, Effect effect, bool expectedResult);

        public static Test IsRelevantToBehaviour => TestThat
            .GivenEachOf(() => new IsRelevantToTestCase[]
            {
                // fulfills positive element
                new(
                    goal: new(IsPresent(element1)),
                    effect: new(IsPresent(element1)),
                    expectedResult: true),

                // fulfills negative element
                new(
                    goal: new(!IsPresent(element1)),
                    effect: new(!IsPresent(element1)),
                    expectedResult: true),

                // fulfills positive & negative element
                new(
                    goal: new(!IsPresent(element1) & IsPresent(element2)),
                    effect: new(!IsPresent(element1) & IsPresent(element2)),
                    expectedResult: true),

                // doesn't fulfill any elements
                new(
                    goal: new(!IsPresent(element1) & IsPresent(element2)),
                    effect: new(IsPresent(element1) & !IsPresent(element2)),
                    expectedResult: false),

                // fulfills positive element, undoes positive element
                new(
                    goal: new(IsPresent(element1) & IsPresent(element2)),
                    effect: new(!IsPresent(element1) & IsPresent(element2)),
                    expectedResult: false),

                // Variable doesn't confuse matters..
                new(
                    goal: new(At(new Constant("C2"), new Constant("JFK"))),
                    effect: new(At(new Constant("C2"), new Constant("JFK")) & !In(new Constant("C2"), P)),
                    expectedResult: true),
            })
            .When(tc => tc.effect.IsRelevantTo(tc.goal))
            .ThenReturns()
            .And((tc, r) => r.Should().Be(tc.expectedResult));

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