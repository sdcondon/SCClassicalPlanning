using FluentAssertions;
using FlUnit;
using SCClassicalPlanning.ExampleDomains.FromAIaMA;
using SCClassicalPlanning.Planning.StateSpaceSearch.Heuristics;
using SCFirstOrderLogic;
using SCFirstOrderLogic.Inference;
using SCFirstOrderLogic.Inference.Resolution;
using System.Numerics;
using static SCClassicalPlanning.ExampleDomains.FromAIaMA.AirCargo;
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
                    InvariantsKB: MakeInvariantsKB(new Sentence[]
                    {
                        Cargo(new Constant("cargo1")),
                        Cargo(new Constant("cargo2")),
                        Plane(new Constant("plane1")),
                        Plane(new Constant("plane2")),
                        Airport(new Constant("airport1")),
                        Airport(new Constant("airport2")),
                        ForAll(A, If(Cargo(A), !Plane(A))),
                        ForAll(A, If(Cargo(A), !Airport(A))),
                        ForAll(A, If(Plane(A), !Cargo(A))),
                        ForAll(A, If(Plane(A), !Airport(A))),
                        ForAll(A, If(Airport(A), !Cargo(A))),
                        ForAll(A, If(Airport(A), !Plane(A))),
                    })),

                new(
                    Problem: BlocksWorld.ExampleProblem,
                    Heuristic: new IgnorePreconditionsGreedySetCover(BlocksWorld.Domain),
                    InvariantsKB: MakeInvariantsKB(new Sentence[]
                    {
                        Block(new Constant("blockA")),
                        Block(new Constant("blockB")),
                        Block(new Constant("blockC")),
                        !Block(Table),
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
                        Block(new Constant("blockB")),
                        Block(new Constant("blockC")),
                        Block(new Constant("blockD")),
                        Block(new Constant("blockE")),
                        !Block(Table),
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
                //(h, kb) => new BackwardStateSpaceSearch_LiftedWithKB(h, kb), // doesnt work yet
                //(h, kb) => new BackwardStateSpaceSearch_LiftedWithoutKB(h), // doesnt work yet
                //(h, kb) => new BackwardStateSpaceSearch_PropositionalWithoutKB(h),
                (h, kb) => new BackwardStateSpaceSearch_PropositionalWithKB(h, kb),
            })
            .AndEachOf(() => new TestCase[]
            {
                new(
                    Problem: AirCargo.ExampleProblem,
                    Heuristic: new IgnorePreconditionsGreedySetCover(AirCargo.Domain),
                    InvariantsKB: MakeInvariantsKB(new Sentence[]
                    {
                        Cargo(new Constant("cargo1")),
                        Cargo(new Constant("cargo2")),
                        Plane(new Constant("plane1")),
                        Plane(new Constant("plane2")),
                        Airport(new Constant("airport1")),
                        Airport(new Constant("airport2")),
                        ForAll(A, If(Cargo(A), !Plane(A))),
                        ForAll(A, If(Cargo(A), !Airport(A))),
                        ForAll(A, If(Plane(A), !Cargo(A))),
                        ForAll(A, If(Plane(A), !Airport(A))),
                        ForAll(A, If(Airport(A), !Cargo(A))),
                        ForAll(A, If(Airport(A), !Plane(A))),
                    })),

                new(
                    Problem: BlocksWorld.ExampleProblem,
                    Heuristic: new IgnorePreconditionsGreedySetCover(BlocksWorld.Domain),
                    InvariantsKB: MakeInvariantsKB(new Sentence[]
                    {
                        Block(new Constant("blockA")),
                        Block(new Constant("blockB")),
                        Block(new Constant("blockC")),
                        !Block(Table),
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
                        Block(new Constant("blockB")),
                        Block(new Constant("blockC")),
                        Block(new Constant("blockD")),
                        Block(new Constant("blockE")),
                        !Block(Table),
                        ForAll(A, B, If(On(A, B), !Clear(B))),
                    })),
            })
            .When((_, makePlanner, tc) => makePlanner(tc.Heuristic, tc.InvariantsKB).CreatePlan(tc.Problem))
            .ThenReturns()
            .And((_, _, tc, pl) => pl.ApplyTo(tc.Problem.InitialState).Satisfies(tc.Problem.Goal).Should().BeTrue())
            .And((cxt, _, tc, pl) => cxt.WriteOutputLine(new PlanFormatter(tc.Problem.Domain).Format(pl)));

        private static IKnowledgeBase MakeInvariantsKB(IEnumerable<Sentence> invariants)
        {
            var resolutionKb = new SimpleResolutionKnowledgeBase(
                new SimpleClauseStore(),
                SimpleResolutionKnowledgeBase.Filters.None,
                SimpleResolutionKnowledgeBase.PriorityComparisons.UnitPreference);

            var invariantKb = new UniqueNamesAxiomisingKnowledgeBase(EqualityAxiomisingKnowledgeBase.CreateAsync(resolutionKb).GetAwaiter().GetResult());
            invariantKb.Tell(invariants);

            return invariantKb;
        }
    }
}
