using FluentAssertions;
using FlUnit;
using SCClassicalPlanning.ExampleDomains;
using SCFirstOrderLogic;
using static SCClassicalPlanning.ExampleDomains.Container;

namespace SCClassicalPlanning
{
    public static class ProblemTests
    {
        private static readonly Constant element1 = new(nameof(element1));
        private static readonly Constant element2 = new(nameof(element2));

        private static readonly Problem containerProblem = new Problem(Container.Domain, new[] { element1, element2 }, State.Empty, Goal.Empty);

        private record GetApplicableActionsTestCase(Problem Problem, State State, Action[] ExpectedResult);

        public static Test GetApplicableActionsBehaviour => TestThat
            .GivenEachOf(() => new GetApplicableActionsTestCase[]
            {
                new(
                    Problem: containerProblem,
                    State: State.Empty,
                    ExpectedResult: new[] { Add(element1), Add(element2) }),

                new(
                    Problem: containerProblem,
                    State: new(IsPresent(element1)),
                    ExpectedResult: new[] { Remove(element1), Add(element2), Swap(element1, element2) }),

                new(
                    Problem: containerProblem,
                    State: new(IsPresent(element1) & IsPresent(element2)),
                    ExpectedResult: new[] { Remove(element1), Remove(element2) }),
            })
            .When(tc => tc.Problem.GetApplicableActions(tc.State))
            .ThenReturns()
            .And((tc, r) => r.Should().BeEquivalentTo(tc.ExpectedResult));

        private record GetRelevantActionsTestCase(Problem Problem, Goal Goal, Action[] ExpectedResult);

        public static Test GetRelevantActionsBehaviour => TestThat
            .GivenEachOf(() => new GetRelevantActionsTestCase[]
            {
                new(
                    Problem: containerProblem,
                    Goal: new(IsPresent(element1)),
                    ExpectedResult: new[] { Add(element1), Swap(element2, element1) }),

                new(
                    Problem: containerProblem,
                    Goal: new(IsPresent(element1) & IsPresent(element2)),
                    ExpectedResult: new[] { Add(element1), Add(element2) }),

                new(
                    Problem: containerProblem,
                    Goal: new(IsPresent(element1) & !IsPresent(element2)),
                    ExpectedResult: new[] { Add(element1), Remove(element2), Swap(element2, element1) }),
            })
            .When(tc => tc.Problem.GetRelevantActions(tc.Goal))
            .ThenReturns()
            .And((tc, r) => r.Should().BeEquivalentTo(tc.ExpectedResult));
    }
}