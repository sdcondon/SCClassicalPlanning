﻿using FluentAssertions;
using FlUnit;
using SCClassicalPlanning.ExampleDomains.FromAIaMA;
using SCClassicalPlanning.Planning.StateSpaceSearch.Heuristics;
using SCFirstOrderLogic;
using SCFirstOrderLogic.Inference;
using SCFirstOrderLogic.Inference.Resolution;
using static SCClassicalPlanning.ExampleDomains.FromAIaMA.BlocksWorld;
using static SCFirstOrderLogic.SentenceCreation.OperableSentenceFactory;

namespace SCClassicalPlanning.Planning.StateSpaceSearch
{
    public static class BackwardStateSpaceSearchTests
    {
        private record TestCase(Problem Problem, IHeuristic Heuristic);

        public static Test CreatedPlanValidity => TestThat
            .GivenTestContext()
            .AndEachOf(() => new TestCase[]
            {
                new(
                    Problem: AirCargo.ExampleProblem,
                    Heuristic: MakeInvariantCheckingHeuristic(
                        AirCargo.Domain,
                        Array.Empty<Sentence>())),

                new(
                    Problem: BlocksWorld.ExampleProblem,
                    Heuristic: MakeInvariantCheckingHeuristic(
                        BlocksWorld.Domain,
                        new Sentence[] { ForAll(A, B, If(On(A, B), !Clear(B))), })),

                new(
                    Problem: SpareTire.ExampleProblem,
                    Heuristic: MakeInvariantCheckingHeuristic(
                        SpareTire.Domain,
                        Array.Empty<Sentence>())),

                new(
                    Problem: BlocksWorld.LargeExampleProblem,
                    Heuristic: MakeInvariantCheckingHeuristic(
                        BlocksWorld.Domain,
                        new Sentence[] { ForAll(A, B, If(On(A, B), !Clear(B))), })),
            })
            .When((_, tc) =>
            {
                var planner = new BackwardStateSpaceSearch(tc.Heuristic);
                return planner.CreatePlanAsync(tc.Problem).GetAwaiter().GetResult();
            })
            .ThenReturns()
            .And((_, tc, pl) => pl.ApplyTo(tc.Problem.InitialState).Satisfies(tc.Problem.Goal).Should().BeTrue())
            .And((cxt, tc, pl) => cxt.WriteOutputLine(new PlanFormatter(tc.Problem.Domain).Format(pl)));

        private static IHeuristic MakeInvariantCheckingHeuristic(Domain domain, IEnumerable<Sentence> invariants)
        {
            var invariantKb = new SimpleResolutionKnowledgeBase(
                new SimpleClauseStore(),
                SimpleResolutionKnowledgeBase.Filters.None,
                SimpleResolutionKnowledgeBase.PriorityComparisons.UnitPreference);
            invariantKb.Tell(invariants);

            var innerHeuristic = new IgnorePreconditionsGreedySetCover(domain);

            return new GoalInvariantCheck(invariantKb, innerHeuristic);
        }
    }
}
