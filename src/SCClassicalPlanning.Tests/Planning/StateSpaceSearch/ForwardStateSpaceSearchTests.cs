using FluentAssertions;
using FlUnit;
using SCClassicalPlanning.ExampleDomains.FromAIaMA;
using SCClassicalPlanning.Planning.StateSpaceSearch.Heuristics;

namespace SCClassicalPlanning.Planning.StateSpaceSearch
{
    public static class ForwardStateSpaceSearchTests
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
                var planner = new ForwardStateSpaceSearch(heuristic);
                return planner.CreatePlanAsync(problem).GetAwaiter().GetResult();
            })
            .ThenReturns()
            .And((_, pr, pl) => pl.ApplyTo(pr.InitialState).Satisfies(pr.Goal).Should().BeTrue())
            .And((cxt, pr, pl) => cxt.WriteOutputLine(new PlanFormatter(pr.Domain).Format(pl)));
    }
}
