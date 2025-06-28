using FluentAssertions;
using FlUnit;
using SCFirstOrderLogic;
using SCFirstOrderLogic.SentenceManipulation.VariableManipulation;
using static SCClassicalPlanning.ExampleDomains.AsCode.AirCargoDomain;
using static SCClassicalPlanning.ExampleDomains.AsCode.ContainerDomain;
using static SCFirstOrderLogic.SentenceCreation.OperableSentenceFactory;

namespace SCClassicalPlanning;

public static class HashSetStateTests
{
    private static readonly Function element1 = new(nameof(element1));
    private static readonly Function element2 = new(nameof(element2));

    private static readonly Function cargo1 = new(nameof(cargo1));
    private static readonly Function plane1 = new(nameof(plane1));
    private static readonly Function airport1 = new(nameof(airport1));

    private record ApplyTestCase(HashSetState State, Effect Effect, HashSetState ExpectedResult);

    public static Test ApplyBehaviour => TestThat
        .GivenEachOf(() => new ApplyTestCase[]
        {
            new( // Adds atom
                State: HashSetState.Empty,
                Effect: new(IsPresent(element1)),
                ExpectedResult: new(IsPresent(element1))),

            new( // Removes atom
                State: new(IsPresent(element1)),
                Effect: new(!IsPresent(element1)),
                ExpectedResult: HashSetState.Empty),

            new( // Doesn't add duplicate
                State: new(IsPresent(element1)),
                Effect: new(IsPresent(element1)),
                ExpectedResult: new(IsPresent(element1))),

            new( // Doesn't complain about removing non-present element
                State: HashSetState.Empty,
                Effect: new(!IsPresent(element1)),
                ExpectedResult: HashSetState.Empty),
        })
        .When(tc => tc.State.Apply(tc.Effect))
        .ThenReturns()
        .And((tc, s) => s.Should().BeEquivalentTo(tc.ExpectedResult));

    private record MeetsTestCase(HashSetState State, Goal Goal, bool ExpectedResult);

    public static Test MeetsBehaviour => TestThat
        .GivenEachOf(() => new MeetsTestCase[]
        {
            new( // met positive element
                State: new(IsPresent(element1)),
                Goal: new(IsPresent(element1)),
                ExpectedResult: true),

            new( // met negative element, present irrelevant element
                State: new(IsPresent(element2)),
                Goal: new(!IsPresent(element1)),
                ExpectedResult: true),

            new( // met positive & negative element
                State: new(IsPresent(element2)),
                Goal: new(!IsPresent(element1) & IsPresent(element2)),
                ExpectedResult: true),

            new( // met positive element, unmet negative element
                State: new(IsPresent(element1) & IsPresent(element2)),
                Goal: new(IsPresent(element1) & !IsPresent(element2)),
                ExpectedResult: false),

            new( // met negative element, unmet positive element
                State: HashSetState.Empty,
                Goal: new(IsPresent(element1) & !IsPresent(element2)),
                ExpectedResult: false),
        })
        .When(tc => tc.State.Meets(tc.Goal))
        .ThenReturns()
        .And((tc, r) => r.Should().Be(tc.ExpectedResult));

    private record GetSubstitutionsToMeetTestCase(HashSetState State, Goal Goal, IEnumerable<VariableSubstitution> ExpectedResult);

    public static Test GetSubstitutionsToMeetBehaviour => TestThat
        .GivenEachOf(() => new GetSubstitutionsToMeetTestCase[]
        {
            new( // met positive element - ground
                State: new(IsPresent(element1)),
                Goal: new(IsPresent(element1)),
                ExpectedResult: new VariableSubstitution[]
                {
                    new()
                }),

            new( // met positive element - variable
                State: new(IsPresent(element1)),
                Goal: new(IsPresent(E)),
                ExpectedResult: new VariableSubstitution[]
                {
                    new(new Dictionary<VariableReference, Term>() { [E] = element1 })
                }),

            new( // met positive element, unmet negative element
                State: new(In(cargo1, plane1) & At(plane1, airport1)),
                Goal: new(In(cargo1, P) & !At(P, airport1)),
                ExpectedResult: Array.Empty<VariableSubstitution>()),
        })
        .When(tc => tc.State.GetSubstitutionsToMeet(tc.Goal))
        .ThenReturns()
        .And((tc, r) => r.Should().BeEquivalentTo(tc.ExpectedResult));

    private record EqualityTestCase(HashSetState X, HashSetState Y, bool ExpectedEquality);

    public static Test EqualityBehaviour => TestThat
        .GivenEachOf(() => new EqualityTestCase[]
        {
            new(
                X: HashSetState.Empty,
                Y: new HashSetState(),
                ExpectedEquality: true),

            new(
                X: new HashSetState(new Predicate("A"), new Predicate("B")),
                Y: new HashSetState(new Predicate("A"), new Predicate("B")),
                ExpectedEquality: true),

            new(
                X: new HashSetState(new Predicate("A"), new Predicate("B")),
                Y: new HashSetState(new Predicate("B"), new Predicate("A")),
                ExpectedEquality: true),

            new(
                X: new HashSetState(new Predicate("A"), new Predicate("B")),
                Y: new HashSetState(new Predicate("A")),
                ExpectedEquality: false),
        })
        .When(tc => (Equality: tc.X.Equals(tc.Y), HashCodeEquality: tc.X.GetHashCode() == tc.Y.GetHashCode()))
        .ThenReturns()
        .And((tc, rv) => rv.Equality.Should().Be(tc.ExpectedEquality))
        .And((tc, rv) => rv.HashCodeEquality.Should().Be(tc.ExpectedEquality));
}
