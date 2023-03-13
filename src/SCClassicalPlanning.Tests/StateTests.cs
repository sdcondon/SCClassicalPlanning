using FluentAssertions;
using FlUnit;
using SCFirstOrderLogic;
using SCFirstOrderLogic.SentenceManipulation;
using static SCClassicalPlanning.ExampleDomains.AsCode.AirCargo;
using static SCClassicalPlanning.ExampleDomains.AsCode.Container;
using static SCFirstOrderLogic.SentenceCreation.OperableSentenceFactory;

namespace SCClassicalPlanning
{
    public static class StateTests
    {
        private static readonly Constant element1 = new(nameof(element1));
        private static readonly Constant element2 = new(nameof(element2));

        private static readonly Constant cargo1 = new(nameof(cargo1));
        private static readonly Constant plane1 = new(nameof(plane1));
        private static readonly Constant airport1 = new(nameof(airport1));

        private record ApplyTestCase(State State, Effect Effect, State ExpectedResult);

        public static Test ApplyBehaviour => TestThat
            .GivenEachOf(() => new ApplyTestCase[]
            {
                new( // Adds atom
                    State: State.Empty,
                    Effect: new(IsPresent(element1)),
                    ExpectedResult: new(IsPresent(element1))),

                new( // Removes atom
                    State: new(IsPresent(element1)),
                    Effect: new(!IsPresent(element1)),
                    ExpectedResult: State.Empty),

                new( // Doesn't add duplicate
                    State: new(IsPresent(element1)),
                    Effect: new(IsPresent(element1)),
                    ExpectedResult: new(IsPresent(element1))),

                new( // Doesn't complain about removing non-present element
                    State: State.Empty,
                    Effect: new(!IsPresent(element1)),
                    ExpectedResult: State.Empty),
            })
            .When(tc => tc.State.Apply(tc.Effect))
            .ThenReturns()
            .And((tc, s) => s.Should().BeEquivalentTo(tc.ExpectedResult));

        private record SatisfiesTestCase(State State, Goal Goal, bool ExpectedResult);

        public static Test IsSatisfiedByBehaviour => TestThat
            .GivenEachOf(() => new SatisfiesTestCase[]
            {
                new( // satisfied positive element
                    State: new(IsPresent(element1)),
                    Goal: new(IsPresent(element1)),
                    ExpectedResult: true),

                new( // satisfied negative element, present irrelevant element
                    State: new(IsPresent(element2)),
                    Goal: new(!IsPresent(element1)),
                    ExpectedResult: true),

                new( // satisfied positive & negative element
                    State: new(IsPresent(element2)),
                    Goal: new(!IsPresent(element1) & IsPresent(element2)),
                    ExpectedResult: true),

                new( // satisfied positive element, unsatisfied negative element
                    State: new(IsPresent(element1) & IsPresent(element2)),
                    Goal: new(IsPresent(element1) & !IsPresent(element2)),
                    ExpectedResult: false),

                new( // satisfied negative element, unsatisfied positive element
                    State: State.Empty,
                    Goal: new(IsPresent(element1) & !IsPresent(element2)),
                    ExpectedResult: false),
            })
            .When(tc => tc.State.Satisfies(tc.Goal))
            .ThenReturns()
            .And((tc, r) => r.Should().Be(tc.ExpectedResult));

        private record GetSatisfyingSubstitutionsTestCase(State State, Goal Goal, IEnumerable<VariableSubstitution> ExpectedResult);

        public static Test GetSatisfyingSubstitutionsBehaviour => TestThat
            .GivenEachOf(() => new GetSatisfyingSubstitutionsTestCase[]
            {
                new( // satisfied positive element - ground
                    State: new(IsPresent(element1)),
                    Goal: new(IsPresent(element1)),
                    ExpectedResult: new VariableSubstitution[]
                    {
                        new()
                    }),

                new( // satisfied positive element - variable
                    State: new(IsPresent(element1)),
                    Goal: new(IsPresent(E)),
                    ExpectedResult: new VariableSubstitution[]
                    {
                        new(new Dictionary<VariableReference, Term>() { [E] = element1 })
                    }),

                new( // satisfied positive element, unsatisfied negative element
                    State: new(In(cargo1, plane1) & At(plane1, airport1)),
                    Goal: new(In(cargo1, P) & !At(P, airport1)),
                    ExpectedResult: Array.Empty<VariableSubstitution>()),
            })
            .When(tc => tc.State.GetSatisfyingSubstitutions(tc.Goal))
            .ThenReturns()
            .And((tc, r) => r.Should().BeEquivalentTo(tc.ExpectedResult));

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
