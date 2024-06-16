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
                Problem: HaveCakeAndEatCakeTooDomain.ExampleProblem,
                ExpectedCost: 1),
        })
        .When(tc => new PlanningGraphLevelSum(tc.Problem.ActionSchemas).EstimateCost(tc.Problem.InitialState, tc.Problem.EndGoal))
        .ThenReturns()
        .And((tc, rv) => rv.Should().Be(tc.ExpectedCost));
}
