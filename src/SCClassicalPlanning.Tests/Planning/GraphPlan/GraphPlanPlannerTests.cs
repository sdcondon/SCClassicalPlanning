using FluentAssertions;
using FlUnit;
using SCClassicalPlanning.ExampleDomains.FromAIaMA;

namespace SCClassicalPlanning.Planning.GraphPlan
{
    public static class GraphPlanPlannerTests
    {
        public static Test CreatedPlanValidity => TestThat
            .GivenTestContext()
            .AndEachOf(() => new Problem[]
            {
                SpareTire.ExampleProblem,
                AirCargo.ExampleProblem,
                BlocksWorld.ExampleProblem,
                // BlocksWorld.LargeExampleProblem, // TODO: doesn't work (or indeed terminate) yet. make planningtask more interrogable, then fix.
                // AirCargoOneAtATime.Problem, // TODO: doesn't work yet. make planningtask more interrogable, then fix.
            })
            .When((_, tc) => new GraphPlanPlanner().CreatePlan(tc))
            .ThenReturns()
            .And((_, tc, pl) => tc.Goal.IsSatisfiedBy(pl.ApplyTo(tc.InitialState)).Should().BeTrue())
            .And((cxt, tc, pl) => cxt.WriteOutputLine(new PlanFormatter(tc.Domain).Format(pl)));

        public static Test Termination => TestThat
            .GivenEachOf(() => new Problem[]
            {
                BlocksWorld.UnsolvableExampleProblem,
            })
            .When(p =>
            {
                // NB: Yes, in theory a flaky test if the machine is running really slowly for whatever
                // reason. Given that we're testing "does this thing terminate" there's not really much
                // of an alternative.
                var timeoutToken = new CancellationTokenSource(TimeSpan.FromSeconds(2)).Token;
                return new GraphPlanPlanner().CreatePlan(p, timeoutToken);
            })
            .ThenThrows()
            .And((_, ex) => ex.Should().BeOfType<InvalidOperationException>());
    }
}
