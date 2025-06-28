using FluentAssertions;
using FlUnit;
using SCFirstOrderLogic;
using static SCClassicalPlanning.ExampleDomains.AsCode.AirCargoDomain;
using static SCClassicalPlanning.ExampleDomains.AsCode.ContainerDomain;
using static SCFirstOrderLogic.SentenceCreation.OperableSentenceFactory;

namespace SCClassicalPlanning;

public static class GoalTests
{
    private static readonly Function element1 = new(nameof(element1));
    private static readonly Function element2 = new(nameof(element2));

    private record IsRelevantTestCase(Goal Goal, Effect Effect, bool ExpectedResult);

    public static Test IsRelevantBehaviour => TestThat
        .GivenEachOf(() => new IsRelevantTestCase[]
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
                Goal: new(At(new Function("C2"), new Function("JFK"))),
                Effect: new(At(new Function("C2"), new Function("JFK")) & !In(new Function("C2"), P)),
                ExpectedResult: true),
        })
        .When(tc => tc.Goal.IsRelevant(tc.Effect))
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
                X: new Goal(new(new("A")), new(new("B"))),
                Y: new Goal(new(new("A")), new(new("B"))),
                ExpectedEquality: true),

            new(
                X: new Goal(new(new("A")), new(new("B"))),
                Y: new Goal(new(new("B")), new(new("A"))),
                ExpectedEquality: true),

            new(
                X: new Goal(new(new("A")), new(new("B"))),
                Y: new Goal(new Predicate("A")),
                ExpectedEquality: false),
        })
        .When(tc => (Equality: tc.X.Equals(tc.Y), HashCodeEquality: tc.X.GetHashCode() == tc.Y.GetHashCode()))
        .ThenReturns()
        .And((tc, rv) => rv.Equality.Should().Be(tc.ExpectedEquality))
        .And((tc, rv) => rv.HashCodeEquality.Should().Be(tc.ExpectedEquality));
}