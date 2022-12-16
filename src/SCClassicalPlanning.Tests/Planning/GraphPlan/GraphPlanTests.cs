using FluentAssertions;
using FlUnit;
using SCClassicalPlanning.ExampleDomains.FromAIaMA;
using SCFirstOrderLogic;
using static SCClassicalPlanning.ExampleDomains.FromAIaMA.BlocksWorld;

namespace SCClassicalPlanning.Planning.GraphPlan
{
    public static class GraphPlanTests
    {
        public static Test CreatedPlanValidity => TestThat
            .GivenTestContext()
            .AndEachOf(() => new Problem[]
            {
                SpareTire.ExampleProblem,
                //AirCargo.ExampleProblem,
                BlocksWorld.ExampleProblem,
                //BlocksWorld.LargeExampleProblem,
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
