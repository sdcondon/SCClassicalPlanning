using BenchmarkDotNet.Attributes;
using SCClassicalPlanning.ExampleDomains.FromAIaMA;
using SCClassicalPlanning.Planning;
using SCClassicalPlanning.Planning.Search;
using SCFirstOrderLogic;
using SCFirstOrderLogic.Inference;
using static SCFirstOrderLogic.SentenceCreation.OperableSentenceFactory;
using static SCClassicalPlanning.ExampleDomains.FromAIaMA.BlocksWorld;
using SCFirstOrderLogic.Inference.Resolution;
using SCClassicalPlanning.Planning.Search.CostStrategies;
using SCClassicalPlanning.Planning.GraphPlan;

namespace SCClassicalPlanning.Benchmarks.Planning
{
    [MemoryDiagnoser]
    [InProcess]
    public class PlannerBenchmarks
    {
        public record TestCase(string Label, Problem Problem, ICostStrategy CostStrategy, IKnowledgeBase InvariantsKB)
        {
            public override string ToString() => Label;
        }

        public static IEnumerable<TestCase> TestCases { get; } = new TestCase[]
        {
            new(
                Label: "Air Cargo",
                Problem: AirCargo.ExampleProblem,
                CostStrategy: new IgnorePreconditionsGreedySetCover(AirCargo.Domain),
                InvariantsKB: MakeInvariantsKB(Array.Empty<Sentence>())),

            new(
                Label: "Blocks - Small",
                Problem: BlocksWorld.ExampleProblem,
                CostStrategy: new IgnorePreconditionsGreedySetCover(BlocksWorld.Domain),
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
                Label: "Spare Tire",
                Problem: SpareTire.ExampleProblem,
                CostStrategy: new IgnorePreconditionsGreedySetCover(SpareTire.Domain),
                InvariantsKB: MakeInvariantsKB(Array.Empty<Sentence>())),

            ////new(
            ////    Label: "Blocks - Large",
            ////    Problem: BlocksWorld.LargeExampleProblem,
            ////    Strategy: new IgnorePreconditionsGreedySetCover(BlocksWorld.Domain),
            ////    InvariantsKB: MakeInvariantsKB(new Sentence[]
            ////    {
            ////        Block(new Constant("blockA")),
            ////        Equal(new Constant("blockA"), new Constant("blockA")),
            ////        !Equal(new Constant("blockA"), new Constant("blockB")),
            ////        !Equal(new Constant("blockA"), new Constant("blockC")),
            ////        !Equal(new Constant("blockA"), new Constant("blockD")),
            ////        !Equal(new Constant("blockA"), new Constant("blockE")),
            ////        Block(new Constant("blockB")),
            ////        !Equal(new Constant("blockB"), new Constant("blockA")),
            ////        Equal(new Constant("blockB"), new Constant("blockB")),
            ////        !Equal(new Constant("blockB"), new Constant("blockC")),
            ////        !Equal(new Constant("blockB"), new Constant("blockD")),
            ////        !Equal(new Constant("blockB"), new Constant("blockE")),
            ////        Block(new Constant("blockC")),
            ////        !Equal(new Constant("blockC"), new Constant("blockA")),
            ////        !Equal(new Constant("blockC"), new Constant("blockB")),
            ////        Equal(new Constant("blockC"), new Constant("blockC")),
            ////        !Equal(new Constant("blockC"), new Constant("blockD")),
            ////        !Equal(new Constant("blockC"), new Constant("blockE")),
            ////        Block(new Constant("blockD")),
            ////        !Equal(new Constant("blockD"), new Constant("blockA")),
            ////        !Equal(new Constant("blockD"), new Constant("blockB")),
            ////        !Equal(new Constant("blockD"), new Constant("blockC")),
            ////        Equal(new Constant("blockD"), new Constant("blockD")),
            ////        !Equal(new Constant("blockD"), new Constant("blockE")),
            ////        Block(new Constant("blockE")),
            ////        !Equal(new Constant("blockE"), new Constant("blockA")),
            ////        !Equal(new Constant("blockE"), new Constant("blockB")),
            ////        !Equal(new Constant("blockE"), new Constant("blockC")),
            ////        !Equal(new Constant("blockE"), new Constant("blockD")),
            ////        Equal(new Constant("blockE"), new Constant("blockE")),
            ////        ForAll(A, B, If(On(A, B), !Clear(B))),
            ////    })),
        };

        [ParamsSource(nameof(TestCases))]
        public TestCase? CurrentTestCase { get; set; }

        [Benchmark]
        public Plan GraphPlan()
        {
            return new GraphPlanPlanner().CreatePlan(CurrentTestCase.Problem);
        }

        [Benchmark]
        public Plan StateSpaceAStar()
        {
            return new StateSpaceAStarPlanner(CurrentTestCase!.CostStrategy).CreatePlan(CurrentTestCase.Problem);
        }

        [Benchmark]
        public Plan GoalSpaceAStar()
        {
            return new GoalSpaceAStarPlanner(CurrentTestCase!.CostStrategy).CreatePlan(CurrentTestCase.Problem);
        }

        [Benchmark]
        public Plan GoalSpaceAStar_PropositionalWithoutKB()
        {
            return new GoalSpaceAStarPlanner_PropositionalWithoutKB(CurrentTestCase!.CostStrategy).CreatePlan(CurrentTestCase.Problem);
        }

        [Benchmark]
        public Plan GoalSpaceAStar_PropositionalWithKB()
        {
            return new GoalSpaceAStarPlanner_PropositionalWithKB(CurrentTestCase!.CostStrategy, CurrentTestCase.InvariantsKB).CreatePlan(CurrentTestCase.Problem);
        }

        private static IKnowledgeBase MakeInvariantsKB(IEnumerable<Sentence> invariants)
        {
            var invariantKb = new SimpleResolutionKnowledgeBase(
                new SimpleClauseStore(invariants),
                SimpleResolutionKnowledgeBase.Filters.None,
                SimpleResolutionKnowledgeBase.PriorityComparisons.UnitPreference);

            return invariantKb;
        }
    }
}
