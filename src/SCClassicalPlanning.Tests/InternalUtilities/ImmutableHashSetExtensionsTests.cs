using FluentAssertions;
using FlUnit;
using System.Collections.Immutable;

namespace SCClassicalPlanning.InternalUtilities
{
    public static class ImmutableHashSetExtensionsTests
    {
        private record SetEqualsTestCase(ImmutableHashSet<int> Set1, ImmutableHashSet<int> Set2, bool ExpectedResult);

        public static Test SetEqualsBehaviour => TestThat
            .GivenEachOf(() => new SetEqualsTestCase[]
            {
                new(
                    Set1: ImmutableHashSet.Create(Enumerable.Range(1, 100).ToArray()),
                    Set2: ImmutableHashSet.Create(Enumerable.Range(1, 100).ToArray()),
                    ExpectedResult: true),

                new(
                    Set1: ImmutableHashSet.Create(Enumerable.Range(1, 100).ToArray()),
                    Set2: ImmutableHashSet.Create(Enumerable.Range(1, 100).Reverse().ToArray()),
                    ExpectedResult: true),

                new(
                    Set1: ImmutableHashSet.Create(Enumerable.Range(1, 100).ToArray()),
                    Set2: ImmutableHashSet.Create(Enumerable.Range(1, 99).Append(101).ToArray()),
                    ExpectedResult: false),

                new(
                    Set1: ImmutableHashSet.Create(Enumerable.Range(1, 99).Append(101).ToArray()),
                    Set2: ImmutableHashSet.Create(Enumerable.Range(1, 100).ToArray()),
                    ExpectedResult: false),

                new(
                    Set1: ImmutableHashSet.Create(Enumerable.Range(1, 100).ToArray()),
                    Set2: ImmutableHashSet.Create(Enumerable.Range(1, 99).ToArray()),
                    ExpectedResult: false),

                new(
                    Set1: ImmutableHashSet.Create(Enumerable.Range(1, 99).ToArray()),
                    Set2: ImmutableHashSet.Create(Enumerable.Range(1, 100).ToArray()),
                    ExpectedResult: false),
            })
            .When(tc => tc.Set1.SetEquals<int>(tc.Set2))
            .ThenReturns((tc, rv) => rv.Should().Be(tc.ExpectedResult));
    }
}
