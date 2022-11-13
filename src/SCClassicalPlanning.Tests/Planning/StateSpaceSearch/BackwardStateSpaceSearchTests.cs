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
        private record TestCase(Problem Problem, IHeuristic Heuristic, IKnowledgeBase InvariantsKB);

        public static Test CreatedPlanValidity => TestThat
            .GivenTestContext()
            .AndEachOf(() => new TestCase[]
            {
                new(
                    Problem: AirCargo.ExampleProblem,
                    Heuristic: new IgnorePreconditionsGreedySetCover(AirCargo.Domain),
                    InvariantsKB: MakeInvariantsKB(Array.Empty<Sentence>())),

                new(
                    Problem: BlocksWorld.ExampleProblem,
                    Heuristic: new IgnorePreconditionsGreedySetCover(BlocksWorld.Domain),
                    InvariantsKB: MakeInvariantsKB(new Sentence[]
                    {
                        // TODO: slicker support for unique names assumption worth looking into at some point.. 
                        Block(new Constant("blockA")),
                        Equal(new Constant("blockA"), new Constant("blockA")),
                        !Equal(new Constant("blockA"), new Constant("blockB")),
                        !Equal(new Constant("blockA"), new Constant("blockC")),
                        Block(new Constant("blockB")),
                        !Equal(new Constant("blockB"), new Constant("blockA")),
                        Equal(new Constant("blockB"), new Constant("blockB")),
                        !Equal(new Constant("blockB"), new Constant("blockC")),
                        Block(new Constant("blockC")),
                        !Equal(new Constant("blockC"), new Constant("blockA")),
                        !Equal(new Constant("blockC"), new Constant("blockB")),
                        Equal(new Constant("blockC"), new Constant("blockC")),
                        ForAll(A, B, If(On(A, B), !Clear(B))),
                    })),

                new(
                    Problem: SpareTire.ExampleProblem,
                    Heuristic: new IgnorePreconditionsGreedySetCover(SpareTire.Domain),
                    InvariantsKB: MakeInvariantsKB(Array.Empty<Sentence>())),

                new(
                    Problem: BlocksWorld.LargeExampleProblem,
                    Heuristic: new IgnorePreconditionsGreedySetCover(BlocksWorld.Domain),
                    InvariantsKB: MakeInvariantsKB(new Sentence[]
                    {
                        Block(new Constant("blockA")),
                        Equal(new Constant("blockA"), new Constant("blockA")),
                        !Equal(new Constant("blockA"), new Constant("blockB")),
                        !Equal(new Constant("blockA"), new Constant("blockC")),
                        !Equal(new Constant("blockA"), new Constant("blockD")),
                        !Equal(new Constant("blockA"), new Constant("blockE")),
                        Block(new Constant("blockB")),
                        !Equal(new Constant("blockB"), new Constant("blockA")),
                        Equal(new Constant("blockB"), new Constant("blockB")),
                        !Equal(new Constant("blockB"), new Constant("blockC")),
                        !Equal(new Constant("blockB"), new Constant("blockD")),
                        !Equal(new Constant("blockB"), new Constant("blockE")),
                        Block(new Constant("blockC")),
                        !Equal(new Constant("blockC"), new Constant("blockA")),
                        !Equal(new Constant("blockC"), new Constant("blockB")),
                        Equal(new Constant("blockC"), new Constant("blockC")),
                        !Equal(new Constant("blockC"), new Constant("blockD")),
                        !Equal(new Constant("blockC"), new Constant("blockE")),
                        Block(new Constant("blockD")),
                        !Equal(new Constant("blockD"), new Constant("blockA")),
                        !Equal(new Constant("blockD"), new Constant("blockB")),
                        !Equal(new Constant("blockD"), new Constant("blockC")),
                        Equal(new Constant("blockD"), new Constant("blockD")),
                        !Equal(new Constant("blockD"), new Constant("blockE")),
                        Block(new Constant("blockE")),
                        !Equal(new Constant("blockE"), new Constant("blockA")),
                        !Equal(new Constant("blockE"), new Constant("blockB")),
                        !Equal(new Constant("blockE"), new Constant("blockC")),
                        !Equal(new Constant("blockE"), new Constant("blockD")),
                        Equal(new Constant("blockE"), new Constant("blockE")),
                        ForAll(A, B, If(On(A, B), !Clear(B))),
                    })),
            })
            .When((_, tc) =>
            {
                var planner = new BackwardStateSpaceSearch(tc.Heuristic);
                return planner.CreatePlanAsync(tc.Problem).GetAwaiter().GetResult();
            })
            .ThenReturns()
            .And((_, tc, pl) => pl.ApplyTo(tc.Problem.InitialState).Satisfies(tc.Problem.Goal).Should().BeTrue())
            .And((cxt, tc, pl) => cxt.WriteOutputLine(new PlanFormatter(tc.Problem.Domain).Format(pl)));

        public static Test CreatedPlanValidity_AlternativeImplementations => TestThat
            .GivenTestContext()
            .AndEachOf(() => new Func<IHeuristic, IKnowledgeBase, IPlanner>[]
            {
                //(h, kb) => new BackwardStateSpaceSearch_LiftedWithoutKB(h), // too slow to be workable
                (h, kb) => new BackwardStateSpaceSearch_PropositionalWithoutKB(h),
                (h, kb) => new BackwardStateSpaceSearch_PropositionalWithKB(h, kb), // Why is this so slow - it should be faster? Interrogable planning processes will help diagnose. 
            })
            .AndEachOf(() => new TestCase[]
            {
                new(
                    Problem: AirCargo.ExampleProblem,
                    Heuristic: new IgnorePreconditionsGreedySetCover(AirCargo.Domain),
                    InvariantsKB: MakeInvariantsKB(Array.Empty<Sentence>())),

                new(
                    Problem: BlocksWorld.ExampleProblem,
                    Heuristic: new IgnorePreconditionsGreedySetCover(BlocksWorld.Domain),
                    InvariantsKB: MakeInvariantsKB(new Sentence[]
                    {
                        // TODO: slicker support for unique names assumption worth looking into at some point.. 
                        Block(new Constant("blockA")),
                        Equal(new Constant("blockA"), new Constant("blockA")),
                        !Equal(new Constant("blockA"), new Constant("blockB")),
                        !Equal(new Constant("blockA"), new Constant("blockC")),
                        !Equal(new Constant("blockA"), Table),
                        Block(new Constant("blockB")),
                        !Equal(new Constant("blockB"), new Constant("blockA")),
                        Equal(new Constant("blockB"), new Constant("blockB")),
                        !Equal(new Constant("blockB"), new Constant("blockC")),
                        !Equal(new Constant("blockB"), Table),
                        Block(new Constant("blockC")),
                        !Equal(new Constant("blockC"), new Constant("blockA")),
                        !Equal(new Constant("blockC"), new Constant("blockB")),
                        Equal(new Constant("blockC"), new Constant("blockC")),
                        !Equal(new Constant("blockC"), Table),
                        !Equal(Table, new Constant("blockA")),
                        !Equal(Table, new Constant("blockB")),
                        !Equal(Table, new Constant("blockC")),
                        !Equal(Table, new Constant("blockD")),
                        ForAll(A, B, If(On(A, B), !Clear(B))),
                    })),

                new(
                    Problem: SpareTire.ExampleProblem,
                    Heuristic: new IgnorePreconditionsGreedySetCover(SpareTire.Domain),
                    InvariantsKB: MakeInvariantsKB(Array.Empty<Sentence>())),

                new(
                    Problem: BlocksWorld.LargeExampleProblem,
                    Heuristic: new IgnorePreconditionsGreedySetCover(BlocksWorld.Domain),
                    InvariantsKB: MakeInvariantsKB(new Sentence[]
                    {
                        Block(new Constant("blockA")),
                        Equal(new Constant("blockA"), new Constant("blockA")),
                        !Equal(new Constant("blockA"), new Constant("blockB")),
                        !Equal(new Constant("blockA"), new Constant("blockC")),
                        !Equal(new Constant("blockA"), new Constant("blockD")),
                        !Equal(new Constant("blockA"), new Constant("blockE")),
                        Block(new Constant("blockB")),
                        !Equal(new Constant("blockB"), new Constant("blockA")),
                        Equal(new Constant("blockB"), new Constant("blockB")),
                        !Equal(new Constant("blockB"), new Constant("blockC")),
                        !Equal(new Constant("blockB"), new Constant("blockD")),
                        !Equal(new Constant("blockB"), new Constant("blockE")),
                        Block(new Constant("blockC")),
                        !Equal(new Constant("blockC"), new Constant("blockA")),
                        !Equal(new Constant("blockC"), new Constant("blockB")),
                        Equal(new Constant("blockC"), new Constant("blockC")),
                        !Equal(new Constant("blockC"), new Constant("blockD")),
                        !Equal(new Constant("blockC"), new Constant("blockE")),
                        Block(new Constant("blockD")),
                        !Equal(new Constant("blockD"), new Constant("blockA")),
                        !Equal(new Constant("blockD"), new Constant("blockB")),
                        !Equal(new Constant("blockD"), new Constant("blockC")),
                        Equal(new Constant("blockD"), new Constant("blockD")),
                        !Equal(new Constant("blockD"), new Constant("blockE")),
                        Block(new Constant("blockE")),
                        !Equal(new Constant("blockE"), new Constant("blockA")),
                        !Equal(new Constant("blockE"), new Constant("blockB")),
                        !Equal(new Constant("blockE"), new Constant("blockC")),
                        !Equal(new Constant("blockE"), new Constant("blockD")),
                        Equal(new Constant("blockE"), new Constant("blockE")),
                        ForAll(A, B, If(On(A, B), !Clear(B))),
                    })),
            })
            .When((_, makePlanner, tc) => makePlanner(tc.Heuristic, tc.InvariantsKB).CreatePlan(tc.Problem))
            .ThenReturns()
            .And((_, _, tc, pl) => pl.ApplyTo(tc.Problem.InitialState).Satisfies(tc.Problem.Goal).Should().BeTrue())
            .And((cxt, _, tc, pl) => cxt.WriteOutputLine(new PlanFormatter(tc.Problem.Domain).Format(pl)));

        private static IKnowledgeBase MakeInvariantsKB(IEnumerable<Sentence> invariants)
        {
            var invariantKb = new SimpleResolutionKnowledgeBase(
                new SimpleClauseStore(),
                SimpleResolutionKnowledgeBase.Filters.None,
                SimpleResolutionKnowledgeBase.PriorityComparisons.UnitPreference);
            invariantKb.Tell(invariants);

            return invariantKb;
        }
    }
}
