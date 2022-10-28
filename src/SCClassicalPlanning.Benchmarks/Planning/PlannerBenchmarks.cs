using BenchmarkDotNet.Attributes;
using SCClassicalPlanning.ExampleDomains.FromAIaMA;
using SCClassicalPlanning.Planning;
using SCClassicalPlanning.Planning.StateSpaceSearch;
using SCClassicalPlanning.Planning.StateSpaceSearch.Heuristics;
using SCFirstOrderLogic;
using SCFirstOrderLogic.Inference;
using static SCFirstOrderLogic.SentenceCreation.OperableSentenceFactory;
using static SCClassicalPlanning.ExampleDomains.FromAIaMA.BlocksWorld;
using SCFirstOrderLogic.Inference.Resolution;

namespace SCClassicalPlanning.Benchmarks.Planning
{
    [MemoryDiagnoser]
    [InProcess]
    public class PlannerBenchmarks
    {
        public record TestCase(string Label, Problem Problem, IHeuristic Heuristic, IKnowledgeBase InvariantsKB)
        {
            public override string ToString() => Label;
        }

        public static IEnumerable<TestCase> TestCases { get; } = new TestCase[]
        {
            new(
                Label: "Air Cargo",
                Problem: AirCargo.ExampleProblem,
                Heuristic: new IgnorePreconditionsGreedySetCover(AirCargo.Domain),
                InvariantsKB: MakeInvariantsKB(Array.Empty<Sentence>())),

            new(
                Label: "Blocks - Small",
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
                Label: "Spare Tire",
                Problem: SpareTire.ExampleProblem,
                Heuristic: new IgnorePreconditionsGreedySetCover(SpareTire.Domain),
                InvariantsKB: MakeInvariantsKB(Array.Empty<Sentence>())),

            ////new(
            ////    Label: "Blocks - Large",
            ////    Problem: BlocksWorld.LargeExampleProblem,
            ////    Heuristic: new IgnorePreconditionsGreedySetCover(BlocksWorld.Domain),
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
        public Plan ForwardStateSpaceSearch()
        {
            return new ForwardStateSpaceSearch(CurrentTestCase!.Heuristic).CreatePlan(CurrentTestCase.Problem);
        }

        [Benchmark]
        public Plan BackwardStateSpaceSearch()
        {
            return new BackwardStateSpaceSearch(CurrentTestCase!.Heuristic).CreatePlan(CurrentTestCase.Problem);
        }

        [Benchmark]
        public Plan BackwardStateSpaceSearch_PropositionalWithoutKB()
        {
            return new BackwardStateSpaceSearch_PropositionalWithoutKB(CurrentTestCase!.Heuristic).CreatePlan(CurrentTestCase.Problem);
        }

        [Benchmark]
        public Plan BackwardStateSpaceSearch_PropositionalWithKB()
        {
            return new BackwardStateSpaceSearch_PropositionalWithKB(CurrentTestCase!.Heuristic, CurrentTestCase.InvariantsKB).CreatePlan(CurrentTestCase.Problem);
        }

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
