using BenchmarkDotNet.Attributes;
using SCClassicalPlanning.ExampleDomains.AsCode;
using SCClassicalPlanning.Planning;
using SCClassicalPlanning.Planning.GraphPlan;
using SCClassicalPlanning.Planning.StateAndGoalSpace;
using SCClassicalPlanning.Planning.StateAndGoalSpace.CostStrategies;
using SCFirstOrderLogic;
using SCFirstOrderLogic.Inference;
using SCFirstOrderLogic.Inference.Basic.Resolution;
using static SCClassicalPlanning.ExampleDomains.AsCode.BlocksWorldDomain;
using static SCFirstOrderLogic.SentenceCreation.OperableSentenceFactory;

namespace SCClassicalPlanning.Benchmarks.Planning;

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
            Problem: AirCargoDomain.ExampleProblem,
            CostStrategy: new IgnorePreconditionsGreedySetCover(AirCargoDomain.ActionSchemas),
            InvariantsKB: MakeInvariantsKB(Array.Empty<Sentence>())),

        new(
            Label: "Blocks - Small",
            Problem: BlocksWorldDomain.ExampleProblem,
            CostStrategy: new IgnorePreconditionsGreedySetCover(BlocksWorldDomain.ActionSchemas),
            InvariantsKB: MakeInvariantsKB(new Sentence[]
            {
                // TODO: slicker support for unique names assumption worth looking into at some point..
                // Probably via invariants KB.
                Block(new Function("blockA")),
                Equal(new Function("blockA"), new Function("blockA")),
                !Equal(new Function("blockA"), new Function("blockB")),
                !Equal(new Function("blockA"), new Function("blockC")),
                Block(new Function("blockB")),
                !Equal(new Function("blockB"), new Function("blockA")),
                Equal(new Function("blockB"), new Function("blockB")),
                !Equal(new Function("blockB"), new Function("blockC")),
                Block(new Function("blockC")),
                !Equal(new Function("blockC"), new Function("blockA")),
                !Equal(new Function("blockC"), new Function("blockB")),
                Equal(new Function("blockC"), new Function("blockC")),
                ForAll(A, B, If(On(A, B), !Clear(B))),
            })),

        new(
            Label: "Spare Tire",
            Problem: SpareTireDomain.ExampleProblem,
            CostStrategy: new IgnorePreconditionsGreedySetCover(SpareTireDomain.ActionSchemas),
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
        return new GraphPlanPlanner().CreatePlan(CurrentTestCase?.Problem ?? throw new InvalidOperationException());
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
        var invariantKb = new ResolutionKnowledgeBase(new DelegateResolutionStrategy(
            new HashSetClauseStore(invariants),
            ClauseResolutionFilters.None,
            ClauseResolutionPriorityComparisons.UnitPreference));

        return invariantKb;
    }
}
