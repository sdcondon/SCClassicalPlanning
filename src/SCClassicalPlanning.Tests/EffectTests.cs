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