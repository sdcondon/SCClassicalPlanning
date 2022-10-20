using FluentAssertions;
using FlUnit;
using SCClassicalPlanning.ExampleDomains.FromAIaMA;
using SCClassicalPlanning.Planning.StateSpaceSearch.Heuristics;
using SCFirstOrderLogic;
using static SCClassicalPlanning.ExampleDomains.FromAIaMA.BlocksWorld;

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
                MakeBigBlocksWorldProblem(),
                SpareTire.ExampleProblem,
            })
            .When((_, problem) =>
            {
                var heuristic = new IgnorePreconditionsGreedySetCover(problem);
                var planner = new ForwardStateSpaceSearch(heuristic);
                return planner.CreatePlanAsync(problem).GetAwaiter().GetResult();
            })
            .ThenReturns()
            .And((_, pr, pl) => pr.Goal.IsSatisfiedBy(pl.ApplyTo(pr.InitialState)).Should().BeTrue())
            .And((cxt, pr, pl) => cxt.WriteOutputLine(new PlanFormatter(pr.Domain).Format(pl)));

        private static Problem MakeBigBlocksWorldProblem()
        {
            Constant blockA = new(nameof(blockA));
            Constant blockB = new(nameof(blockB));
            Constant blockC = new(nameof(blockC));
            Constant blockD = new(nameof(blockD));
            Constant blockE = new(nameof(blockE));

            return BlocksWorld.MakeProblem(
                initialState: new(
                    Block(blockA)
                    & Equal(blockA, blockA)
                    & Block(blockB)
                    & Equal(blockB, blockB)
                    & Block(blockC)
                    & Equal(blockC, blockC)
                    & Block(blockD)
                    & Equal(blockD, blockD)
                    & Block(blockE)
                    & Equal(blockE, blockE)
                    & On(blockA, Table)
                    & On(blockB, Table)
                    & On(blockC, blockA)
                    & On(blockD, blockB)
                    & On(blockE, Table)
                    & Clear(blockD)
                    & Clear(blockE)
                    & Clear(blockC)),
                goal: new(
                    On(blockA, blockB)
                    & On(blockB, blockC)
                    & On(blockC, blockD)
                    & On(blockD, blockE)));
        }
    }
}
