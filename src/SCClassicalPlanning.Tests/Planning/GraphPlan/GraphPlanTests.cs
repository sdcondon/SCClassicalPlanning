using FluentAssertions;
using FlUnit;
using SCClassicalPlanning.ExampleDomains.FromAIaMA;

namespace SCClassicalPlanning.Planning.GraphPlan
{
    public static class GraphPlanTests
    {
        public static Test CreatedPlanValidity => TestThat
            .GivenTestContext()
            .AndEachOf(() => new Problem[]
            {
                SpareTire.ExampleProblem,
                AirCargo.ExampleProblem,
                BlocksWorld.ExampleProblem,
                //BlocksWorld.LargeExampleProblem, // TODO: doesn't work (or indeed terminate). implement termination checks first, then fix this
            })
            .When((_, tc) =>
            {
                var planner = new GraphPlan();
                return planner.CreatePlan(tc);
            })
            .ThenReturns()
            .And((_, tc, pl) => tc.Goal.IsSatisfiedBy(pl.ApplyTo(tc.InitialState)).Should().BeTrue())
            .And((cxt, tc, pl) => cxt.WriteOutputLine(new PlanFormatter(tc.Domain).Format(pl)));
    }
}
