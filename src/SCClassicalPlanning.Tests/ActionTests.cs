using FluentAssertions;
using FlUnit;
using SCFirstOrderLogic;
using static SCClassicalPlanning.ExampleDomains.AsCode.Container;

namespace SCClassicalPlanning;

public static class ActionTests
{
    private static readonly Constant element1 = new(nameof(element1));
    private static readonly Constant element2 = new(nameof(element2));

    private record IsApplicableToTestCase(HashSetState State, Action Action, bool ExpectedResult);

    public static Test IsApplicableToBehaviour => TestThat
        .GivenEachOf<IsApplicableToTestCase>(() => (new IsApplicableToTestCase[]
        {
            new( // Positive - positive precondition
                State: new(IsPresent(element1)),
                Action: Remove(element1),
                ExpectedResult: true),

            new( // Positive - negative precondition
                State: HashSetState.Empty,
                Action: Add(element1),
                ExpectedResult: true),

            new( // Negative - positive precondition
                State: new(IsPresent(element1)),
                Action: Add(element1),
                ExpectedResult: false),

            new( // Negative - negative precondition
                State: HashSetState.Empty,
                Action: Remove(element1),
                ExpectedResult: false),
        }))
        .When(tc => tc.Action.IsApplicableTo(tc.State))
        .ThenReturns()
        .And((tc, r) => r.Should().Be(tc.ExpectedResult));

    private record ApplyToTestCase(HashSetState State, Action Action, HashSetState ExpectedState);

    public static Test ApplyToBehaviour => TestThat
        .GivenEachOf<ApplyToTestCase>(() => (new ApplyToTestCase[]
        {
            new( // Adds atom
                State: HashSetState.Empty,
                Action: Add(element1),
                ExpectedState: new(IsPresent(element1))),

            new( // Removes atom
                State: new(IsPresent(element1)),
                Action: Remove(element1),
                ExpectedState: HashSetState.Empty),

            new( // Doesn't add duplicate
                State: new(IsPresent(element1)),
                Action: Add(element1),
                ExpectedState: new(IsPresent(element1))),

            new( // Doesn't complain about removing non-present element
                State: HashSetState.Empty,
                Action: Remove(element1),
                ExpectedState: HashSetState.Empty),
        }))
        .When(tc => tc.Action.ApplyTo(tc.State))
        .ThenReturns()
        .And((tc, s) => s.Should().BeEquivalentTo(tc.ExpectedState));

    private record IsRelevantToTestCase(Goal Goal, Action Action, bool ExpectedResult);

    public static Test IsRelevantToBehaviour => TestThat
        .GivenEachOf(() => new IsRelevantToTestCase[]
        {
            new( // fulfills positive element
                Goal: new(IsPresent(element1)),
                Action: Add(element1),
                ExpectedResult: true),

            new( // fulfills negative element
                Goal: new(!IsPresent(element1)),
                Action: Remove(element1),
                ExpectedResult: true),

            new( // fulfills positive & negative element
                Goal: new(!IsPresent(element1) & IsPresent(element2)),
                Action: Swap(element1, element2),
                ExpectedResult: true),

            new( // fulfills positive element, undoes positive element
                Goal: new(IsPresent(element1) & IsPresent(element2)),
                Action: Swap(element1, element2),
                ExpectedResult: false),
        })
        .When(tc => tc.Action.IsRelevantTo(tc.Goal))
        .ThenReturns()
        .And((tc, r) => r.Should().Be(tc.ExpectedResult));

    private record RegressTestCase(Goal Goal, Action Action, Goal ExpectedResult);

    public static Test RegressBehaviour => TestThat
        .GivenEachOf(() => new RegressTestCase[]
        {
            new( // Adds atom
                Goal: new(IsPresent(element1)),
                Action: Add(element1),
                ExpectedResult: new(!IsPresent(element1))),

            new( // Removes atom
                Goal: new(!IsPresent(element1)),
                Action: Remove(element1),
                ExpectedResult: new(IsPresent(element1))),

            new( // Swap
                Goal: new(!IsPresent(element1) & IsPresent(element2)),
                Action: Swap(element1, element2),
                ExpectedResult: new(IsPresent(element1) & !IsPresent(element2))),

            new( // Swap - partial 1
                Goal: new(!IsPresent(element1)),
                Action: Swap(element1, element2),
                ExpectedResult: new(IsPresent(element1) & !IsPresent(element2))),

            new( // Swap - partial 2
                Goal: new(IsPresent(element2)),
                Action: Swap(element1, element2),
                ExpectedResult: new(IsPresent(element1) & !IsPresent(element2)))
        })
        .When(tc => tc.Action.Regress(tc.Goal))
        .ThenReturns()
        .And((tc, s) => s.Should().BeEquivalentTo(tc.ExpectedResult));
}