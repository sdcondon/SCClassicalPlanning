using FluentAssertions;
using FlUnit;
using SCClassicalPlanning.ExampleDomains.FromAIaMA;
using SCClassicalPlanning.Planning.Search.CostStrategies;
using SCFirstOrderLogic;
using SCFirstOrderLogic.Inference;
using SCFirstOrderLogic.Inference.Resolution;
using static SCClassicalPlanning.ExampleDomains.FromAIaMA.AirCargo;
using static SCClassicalPlanning.ExampleDomains.FromAIaMA.BlocksWorld;
using static SCFirstOrderLogic.SentenceCreation.OperableSentenceFactory;

namespace SCClassicalPlanning.Planning.Search
{
    public static class GoalSpaceAStarPlannerTests
    {
        private record TestCase(Problem Problem, ICostStrategy Strategy, IKnowledgeBase InvariantsKB);

        public static Test CreatedPlanValidity => TestThat
            .GivenTestContext()
            .AndEachOf(() => new TestCase[]
            {
                new(
                    Problem: AirCargo.ExampleProblem,
                    Strategy: new IgnorePreconditionsGreedySetCover(AirCargo.Domain),
                    InvariantsKB: MakeResolutionKB(new Sentence[]
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
                    Strategy: new IgnorePreconditionsGreedySetCover(BlocksWorld.Domain),
                    InvariantsKB: MakeResolutionKB(new Sentence[]
                    {
                        Block(new Constant("blockA")),
                        Block(new Constant("blockB")),
                        Block(new Constant("blockC")),
                        !Block(Table),
                        ForAll(A, B, If(On(A, B), !Clear(B))),
                    })),

                new(
                    Problem: SpareTire.ExampleProblem,
                    Strategy: new IgnorePreconditionsGreedySetCover(SpareTire.Domain),
                    InvariantsKB: MakeResolutionKB(Array.Empty<Sentence>())),

                new(
                    Problem: BlocksWorld.LargeExampleProblem,
                    Strategy: new IgnorePreconditionsGreedySetCover(BlocksWorld.Domain),
                    InvariantsKB: MakeResolutionKB(new Sentence[]
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
                var planner = new GoalSpaceAStarPlanner(tc.Strategy);
                return planner.CreatePlanAsync(tc.Problem).GetAwaiter().GetResult();
            })
            .ThenReturns()
            .And((_, tc, pl) => pl.ApplyTo(tc.Problem.InitialState).Satisfies(tc.Problem.Goal).Should().BeTrue())
            .And((cxt, tc, pl) => cxt.WriteOutputLine(new PlanFormatter(tc.Problem.Domain).Format(pl)));

        public static Test CreatedPlanValidity_AlternativeImplementations => TestThat
            .GivenTestContext()
            .AndEachOf(() => new TestCase[]
            {
                new(
                    Problem: AirCargo.ExampleProblem,
                    Strategy: new IgnorePreconditionsGreedySetCover(AirCargo.Domain),
                    InvariantsKB: MakeResolutionKB(new Sentence[]
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
                    Strategy: new IgnorePreconditionsGreedySetCover(BlocksWorld.Domain),
                    InvariantsKB: MakeResolutionKB(new Sentence[]
                    {
                        Block(new Constant("blockA")),
                        Block(new Constant("blockB")),
                        Block(new Constant("blockC")),
                        !Block(Table),
                        ForAll(A, B, If(On(A, B), !Clear(B))),
                    })),

                new(
                    Problem: SpareTire.ExampleProblem,
                    Strategy: new IgnorePreconditionsGreedySetCover(SpareTire.Domain),
                    InvariantsKB: MakeResolutionKB(Array.Empty<Sentence>())),

                new(
                    Problem: BlocksWorld.LargeExampleProblem,
                    Strategy: new IgnorePreconditionsGreedySetCover(BlocksWorld.Domain),
                    InvariantsKB: MakeResolutionKB(new Sentence[]
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
            .AndEachOf(() => new Func<TestCase, IPlanner>[]
            {
                tc => new GoalSpaceAStarPlanner_PropositionalWithoutKB(tc.Strategy),
                //tc => new GoalSpaceAStarPlanner_PropositionalWithKB(tc.Strategy, tc.InvariantsKB),
                //tc => new GoalSpaceAStarPlanner_LiftedWithoutKB(tc.Strategy), // doesnt work yet
                //tc => new GoalSpaceAStarPlanner_LiftedWithKB(tc.Strategy, tc.InvariantsKB), // doesnt work yet
            })
            .When((_, tc, makePlanner) => makePlanner(tc).CreatePlan(tc.Problem))
            .ThenReturns()
            .And((_, tc, _, pl) => pl.ApplyTo(tc.Problem.InitialState).Satisfies(tc.Problem.Goal).Should().BeTrue())
            .And((cxt, tc, _, pl) => cxt.WriteOutputLine(new PlanFormatter(tc.Problem.Domain).Format(pl)));

        private static IKnowledgeBase MakeResolutionKB(IEnumerable<Sentence> invariants)
        {
            var resolutionKb = new ResolutionKnowledgeBase(new DelegateResolutionStrategy(
                new HashSetClauseStore(),
                DelegateResolutionStrategy.Filters.None,
                DelegateResolutionStrategy.PriorityComparisons.UnitPreference));

            var invariantKb = new UniqueNamesAxiomisingKnowledgeBase(EqualityAxiomisingKnowledgeBase.CreateAsync(resolutionKb).GetAwaiter().GetResult());
            invariantKb.Tell(invariants);

            return invariantKb;
        }
    }
}
