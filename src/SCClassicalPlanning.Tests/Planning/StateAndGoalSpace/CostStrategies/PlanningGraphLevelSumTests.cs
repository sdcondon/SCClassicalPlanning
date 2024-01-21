using FluentAssertions;
using FlUnit;
using SCClassicalPlanning.ExampleDomains.AsCode;

namespace SCClassicalPlanning.Planning.StateAndGoalSpace.CostStrategies;

public class PlanningGraphLevelSumTesrs
{
    private record TestCase(Problem Problem, float ExpectedCost);

    public static Test EstimateCost => TestThat
        .GivenEachOf(() => new TestCase[]
        {
            new(
                Problem: HaveCakeAndEatCakeToo.ExampleProblem,
                ExpectedCost: 1),
        })
        .When(tc => new PlanningGraphLevelSum(tc.Problem.Domain).EstimateCost(tc.Problem.InitialState, tc.Problem.Goal))
        .ThenReturns()
        .And((tc, rv) => rv.Should().Be(tc.ExpectedCost));
}
