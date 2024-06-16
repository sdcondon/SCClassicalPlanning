using FluentAssertions;
using FlUnit;
using SCClassicalPlanning.ExampleDomains.AsCode;
using SCClassicalPlanning.Planning.StateAndGoalSpace.CostStrategies;

namespace SCClassicalPlanning.Planning.StateAndGoalSpace;

public static class StateSpaceAStarPlannerTests
{
    public static Test CreatedPlanValidity => TestThat
        .GivenTestContext()
        .AndEachOf(() => new Problem[]
        {
            AirCargoDomain.ExampleProblem,
            BlocksWorldDomain.ExampleProblem,
            BlocksWorldDomain.LargeExampleProblem,
            SpareTireDomain.ExampleProblem,
        })
        .When((_, problem) =>
        {
            var costStrategy = new IgnorePreconditionsGreedySetCover(problem.ActionSchemas);
            var planner = new StateSpaceAStarPlanner(costStrategy);
            return planner.CreatePlan(problem);
        })
        .ThenReturns()
        .And((_, pr, pl) => pl.ApplyTo(pr.InitialState).Satisfies(pr.EndGoal).Should().BeTrue())
        .And((cxt, pr, pl) => cxt.WriteOutputLine(new PlanFormatter(pr).Format(pl)));
}
