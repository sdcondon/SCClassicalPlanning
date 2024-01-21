using FluentAssertions;
using FlUnit;
using SCClassicalPlanning.ExampleDomains.AsCode;
using static SCClassicalPlanning.ProblemCreation.OperableProblemFactory;

namespace SCClassicalPlanning.Planning.StateAndGoalSpace.CostStrategies;

public class PlanningGraphSetLevelTests
{
    private record TestCase(Problem Problem, float ExpectedCost);

    public static Test EstimateCost => TestThat
        .GivenEachOf(() => new TestCase[]
        {
            new(
                Problem: HaveCakeAndEatCakeToo.ExampleProblem,
                ExpectedCost: 2),
        })
        .When(tc => new PlanningGraphSetLevel(tc.Problem.Domain).EstimateCost(tc.Problem.InitialState, tc.Problem.Goal))
        .ThenReturns()
        .And((tc, rv) => rv.Should().Be(tc.ExpectedCost));
}
