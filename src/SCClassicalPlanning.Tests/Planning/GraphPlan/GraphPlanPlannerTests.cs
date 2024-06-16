using FluentAssertions;
using FlUnit;
using SCClassicalPlanning.ExampleDomains.AsCode;

namespace SCClassicalPlanning.Planning.GraphPlan;

public static class GraphPlanPlannerTests
{
    public static Test CreatedPlanValidity => TestThat
        .GivenTestContext()
        .AndEachOf(() => new Problem[]
        {
            SpareTireDomain.ExampleProblem,
            AirCargoDomain.ExampleProblem,
            BlocksWorldDomain.ExampleProblem,
            BlocksWorldDomain.LargeExampleProblem,
            AirCargoOneAtATimeDomain.Problem,
        })
        .When((_, tc) => new GraphPlanPlanner().CreatePlan(tc))
        .ThenReturns()
        .And((_, tc, pl) => tc.EndGoal.IsSatisfiedBy(pl.ApplyTo(tc.InitialState)).Should().BeTrue())
        .And((cxt, tc, pl) => cxt.WriteOutputLine(new PlanFormatter(tc).Format(pl)));

    public static Test Termination => TestThat
        .GivenEachOf(() => new Problem[]
        {
            BlocksWorldDomain.UnsolvableExampleProblem,
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
