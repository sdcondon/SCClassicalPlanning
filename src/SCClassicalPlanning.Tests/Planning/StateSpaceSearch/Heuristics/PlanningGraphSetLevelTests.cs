using FluentAssertions;
using FlUnit;
using SCClassicalPlanning.ExampleDomains.FromAIaMA;
using static SCClassicalPlanning.ProblemCreation.OperableProblemFactory;

namespace SCClassicalPlanning.Planning.StateSpaceSearch.Heuristics
{
    public class PlanningGraphSetLevelTests
    {
        private record TestCase(Problem Problem, State State, OperableGoal Goal, float ExpectedCost);

        public static Test EstimateCost => TestThat
            .GivenEachOf(() => new TestCase[]
            {
                new(
                    Problem: HaveCakeAndEatCakeToo.ExampleProblem,
                    State: HaveCakeAndEatCakeToo.ExampleProblem.InitialState,
                    Goal: HaveCakeAndEatCakeToo.ExampleProblem.Goal,
                    ExpectedCost: 2),
            })
            .When(tc => new PlanningGraphSetLevel(tc.Problem).EstimateCost(tc.State, tc.Goal))
            .ThenReturns()
            .And((tc, rv) => rv.Should().Be(tc.ExpectedCost));
    }
}
