using FluentAssertions;
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
        private record TestCase(Problem Problem, Func<State, Goal, float> Heuristic);

        public static Test CreatedPlanValidity => TestThat
            .GivenTestContext()
            .AndEachOf(() => new TestCase[]
            {
                new(
                    Problem: AirCargo.ExampleProblem,
                    Heuristic: MakeInvariantCheckingHeuristic(
                        AirCargo.ExampleProblem,
                        Array.Empty<Sentence>())),

                new(
                    Problem: BlocksWorld.ExampleProblem,
                    Heuristic: MakeInvariantCheckingHeuristic(
                        BlocksWorld.ExampleProblem,
                        new Sentence[] { ForAll(A, B, If(On(A, B), !Clear(B))), })),

                new(
                    Problem: SpareTire.ExampleProblem,
                    Heuristic: MakeInvariantCheckingHeuristic(
                        SpareTire.ExampleProblem,
                        Array.Empty<Sentence>())),

                new(
                    Problem: MakeBigBlocksWorldProblem(),
                    Heuristic: MakeInvariantCheckingHeuristic(
                        MakeBigBlocksWorldProblem(),
                        new Sentence[] { ForAll(A, B, If(On(A, B), !Clear(B))), })),
            })
            .When((_, tc) =>
            {
                var planner = new BackwardStateSpaceSearch(tc.Heuristic);
                return planner.CreatePlanAsync(tc.Problem).GetAwaiter().GetResult();
            })
            .ThenReturns()
            .And((_, tc, pl) => tc.Problem.Goal.IsSatisfiedBy(pl.ApplyTo(tc.Problem.InitialState)).Should().BeTrue())
            .And((cxt, tc, pl) => cxt.WriteOutputLine(new PlanFormatter(tc.Problem.Domain).Format(pl)));

        private static Func<State, Goal, float> MakeInvariantCheckingHeuristic(Problem problem, IEnumerable<Sentence> invariants)
        {
            var invariantKb = new SimpleResolutionKnowledgeBase(
                new SimpleClauseStore(),
                SimpleResolutionKnowledgeBase.Filters.None,
                SimpleResolutionKnowledgeBase.PriorityComparisons.UnitPreference);
            invariantKb.Tell(invariants);

            var innerHeuristic = new IgnorePreconditionsGreedySetCover(problem).EstimateCost;

            return new GoalInvariantCheck(invariantKb, innerHeuristic).EstimateCost;
        }

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
