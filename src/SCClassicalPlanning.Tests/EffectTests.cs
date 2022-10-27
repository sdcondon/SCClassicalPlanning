using FluentAssertions;
using FlUnit;
using SCFirstOrderLogic;
using static SCClassicalPlanning.ExampleDomains.FromAIaMA.AirCargo;
using static SCClassicalPlanning.ExampleDomains.Container;
using static SCFirstOrderLogic.SentenceCreation.OperableSentenceFactory;

namespace SCClassicalPlanning
{
    public static class EffectTests
    {
        private static readonly Constant element1 = new(nameof(element1));
        private static readonly Constant element2 = new(nameof(element2));

        private record IsRelevantToTestCase(Goal Goal, Effect Effect, bool ExpectedResult);

        public static Test IsRelevantToBehaviour => TestThat
            .GivenEachOf(() => new IsRelevantToTestCase[]
            {
                new( // fulfills positive element
                    Goal: new(IsPresent(element1)),
                    Effect: new(IsPresent(element1)),
                    ExpectedResult: true),

                new( // fulfills negative element
                    Goal: new(!IsPresent(element1)),
                    Effect: new(!IsPresent(element1)),
                    ExpectedResult: true),

                new( // fulfills positive & negative element
                    Goal: new(!IsPresent(element1) & IsPresent(element2)),
                    Effect: new(!IsPresent(element1) & IsPresent(element2)),
                    ExpectedResult: true),

                new( // doesn't fulfill any elements
                    Goal: new(!IsPresent(element1) & IsPresent(element2)),
                    Effect: new(IsPresent(element1) & !IsPresent(element2)),
                    ExpectedResult: false),

                new( // fulfills positive element, undoes positive element
                    Goal: new(IsPresent(element1) & IsPresent(element2)),
                    Effect: new(!IsPresent(element1) & IsPresent(element2)),
                    ExpectedResult: false),

                new( // Variable doesn't confuse matters..
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