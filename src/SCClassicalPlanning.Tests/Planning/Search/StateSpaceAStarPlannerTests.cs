﻿using FluentAssertions;
using FlUnit;
using SCClassicalPlanning.ExampleDomains.FromAIaMA;
using SCClassicalPlanning.Planning.Search.CostStrategies;

namespace SCClassicalPlanning.Planning.Search
{
    public static class StateSpaceAStarPlannerTests
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
                var costStrategy = new IgnorePreconditionsGreedySetCover(problem.Domain);
                var planner = new StateSpaceAStarPlanner(costStrategy);
                return planner.CreatePlan(problem);
            })
            .ThenReturns()
            .And((_, pr, pl) => pl.ApplyTo(pr.InitialState).Satisfies(pr.Goal).Should().BeTrue())
            .And((cxt, pr, pl) => cxt.WriteOutputLine(new PlanFormatter(pr.Domain).Format(pl)));
    }
}
