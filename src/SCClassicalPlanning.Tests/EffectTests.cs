using FluentAssertions;
using FlUnit;
using SCFirstOrderLogic;
using static SCClassicalPlanning.ExampleDomains.AsCode.AirCargo;
using static SCClassicalPlanning.ExampleDomains.AsCode.Container;
using static SCFirstOrderLogic.SentenceCreation.OperableSentenceFactory;

namespace SCClassicalPlanning;

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
                X: new Effect(new(new("A")), new(new("B"))),
                Y: new Effect(new(new("A")), new(new("B"))),
                ExpectedEquality: true),

            new(
                X: new Effect(new(new("A")), new(new("B"))),
                Y: new Effect(new(new("B")), new(new("A"))),
                ExpectedEquality: true),

            new(
                X: new Effect(new(new("A")), new(new("B"))),
                Y: new Effect(new Predicate("A")),
                ExpectedEquality: false),
        })
        .When(tc => (Equality: tc.X.Equals(tc.Y), HashCodeEquality: tc.X.GetHashCode() == tc.Y.GetHashCode()))
        .ThenReturns()
        .And((tc, rv) => rv.Equality.Should().Be(tc.ExpectedEquality))
        .And((tc, rv) => rv.HashCodeEquality.Should().Be(tc.ExpectedEquality));
}