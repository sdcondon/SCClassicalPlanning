using FluentAssertions;
using FlUnit;
using SCClassicalPlanning.ExampleDomains.FromAIaMA;
using SCClassicalPlanning.Planning.Search.Heuristics;

namespace SCClassicalPlanning.Planning.Search
{
    public static class StateSpaceSearchTests
    {
        public static Test CreatedPlanValidity => TestThat
            .GivenTestContext()
            .AndEachOf(() => new Problem[]
            {
                AirCargo.ExampleProblem,
                BlocksWorld.ExampleProblem,
                BlocksWorld.LargeExampleProblem,
                SpareTire.ExampleProblem,
            })
            .When((_, problem) =>
            {
                var heuristic = new IgnorePreconditionsGreedySetCover(problem.Domain);
                var planner = new StateSpaceSearch(heuristic);
                return planner.CreatePlan(problem);
            })
            .ThenReturns()
            .And((_, pr, pl) => pl.ApplyTo(pr.InitialState).Satisfies(pr.Goal).Should().BeTrue())
            .And((cxt, pr, pl) => cxt.WriteOutputLine(new PlanFormatter(pr.Domain).Format(pl)));
    }
}
