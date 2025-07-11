﻿using FluentAssertions;
using FlUnit;
using SCClassicalPlanning.ExampleDomains.AsCode;
using SCClassicalPlanning.Planning.StateAndGoalSpace.CostStrategies;
using SCFirstOrderLogic;
using SCFirstOrderLogic.Inference;
using SCFirstOrderLogic.Inference.Basic.KnowledgeBaseDecoration;
using SCFirstOrderLogic.Inference.Basic.Resolution;
using static SCClassicalPlanning.ExampleDomains.AsCode.AirCargoDomain;
using static SCClassicalPlanning.ExampleDomains.AsCode.BlocksWorldDomain;
using static SCFirstOrderLogic.SentenceCreation.OperableSentenceFactory;

namespace SCClassicalPlanning.Planning.StateAndGoalSpace;

public static class GoalSpaceAStarPlannerTests
{
    private record TestCase(Problem Problem, ICostStrategy Strategy, IKnowledgeBase InvariantsKB);

    public static Test CreatedPlanValidity => TestThat
        .GivenTestContext()
        .AndEachOfAsync<TestCase>(async () => new TestCase[]
        {
            new(
                Problem: AirCargoDomain.ExampleProblem,
                Strategy: new IgnorePreconditionsGreedySetCover(AirCargoDomain.ActionSchemas),
                InvariantsKB: await MakeResolutionKBAsync(new Sentence[]
                {
                    Cargo(new Function("cargo1")),
                    Cargo(new Function("cargo2")),
                    Plane(new Function("plane1")),
                    Plane(new Function("plane2")),
                    Airport(new Function("airport1")),
                    Airport(new Function("airport2")),
                    ForAll(A, If(Cargo(A), !Plane(A))),
                    ForAll(A, If(Cargo(A), !Airport(A))),
                    ForAll(A, If(Plane(A), !Cargo(A))),
                    ForAll(A, If(Plane(A), !Airport(A))),
                    ForAll(A, If(Airport(A), !Cargo(A))),
                    ForAll(A, If(Airport(A), !Plane(A))),
                })),

            new(
                Problem: BlocksWorldDomain.ExampleProblem,
                Strategy: new IgnorePreconditionsGreedySetCover(BlocksWorldDomain.ActionSchemas),
                InvariantsKB: await MakeResolutionKBAsync(new Sentence[]
                {
                    Block(new Function("blockA")),
                    Block(new Function("blockB")),
                    Block(new Function("blockC")),
                    !Block(Table),
                    ForAll(A, B, If(On(A, B), !Clear(B))),
                })),

            new(
                Problem: SpareTireDomain.ExampleProblem,
                Strategy: new IgnorePreconditionsGreedySetCover(SpareTireDomain.ActionSchemas),
                InvariantsKB: await MakeResolutionKBAsync(Array.Empty<Sentence>())),

            new(
                Problem: BlocksWorldDomain.LargeExampleProblem,
                Strategy: new IgnorePreconditionsGreedySetCover(BlocksWorldDomain.ActionSchemas),
                InvariantsKB: await MakeResolutionKBAsync(new Sentence[]
                {
                    Block(new Function("blockA")),
                    Block(new Function("blockB")),
                    Block(new Function("blockC")),
                    Block(new Function("blockD")),
                    Block(new Function("blockE")),
                    !Block(Table),
                    ForAll(A, B, If(On(A, B), !Clear(B))),
                })),
        })
        .WhenAsync(async (_, tc) =>
        {
            var planner = new GoalSpaceAStarPlanner(tc.Strategy);
            return await planner.CreatePlanAsync(tc.Problem);
        })
        .ThenReturns()
        .And((_, tc, pl) => pl.ApplyTo(tc.Problem.InitialState).Meets(tc.Problem.EndGoal).Should().BeTrue())
        .And((cxt, tc, pl) => cxt.WriteOutputLine(new PlanFormatter(tc.Problem).Format(pl)));

    public static Test CreatedPlanValidity_AlternativeImplementations => TestThat
        .GivenTestContext()
        .AndEachOfAsync<TestCase>(async () => new TestCase[]
        {
            new(
                Problem: AirCargoDomain.ExampleProblem,
                Strategy: new IgnorePreconditionsGreedySetCover(AirCargoDomain.ActionSchemas),
                InvariantsKB: await MakeResolutionKBAsync(new Sentence[]
                {
                    Cargo(new Function("cargo1")),
                    Cargo(new Function("cargo2")),
                    Plane(new Function("plane1")),
                    Plane(new Function("plane2")),
                    Airport(new Function("airport1")),
                    Airport(new Function("airport2")),
                    ForAll(A, If(Cargo(A), !Plane(A))),
                    ForAll(A, If(Cargo(A), !Airport(A))),
                    ForAll(A, If(Plane(A), !Cargo(A))),
                    ForAll(A, If(Plane(A), !Airport(A))),
                    ForAll(A, If(Airport(A), !Cargo(A))),
                    ForAll(A, If(Airport(A), !Plane(A))),
                })),

            new(
                Problem: BlocksWorldDomain.ExampleProblem,
                Strategy: new IgnorePreconditionsGreedySetCover(BlocksWorldDomain.ActionSchemas),
                InvariantsKB: await MakeResolutionKBAsync(new Sentence[]
                {
                    Block(new Function("blockA")),
                    Block(new Function("blockB")),
                    Block(new Function("blockC")),
                    !Block(Table),
                    ForAll(A, B, If(On(A, B), !Clear(B))),
                })),

            new(
                Problem: SpareTireDomain.ExampleProblem,
                Strategy: new IgnorePreconditionsGreedySetCover(SpareTireDomain.ActionSchemas),
                InvariantsKB: await MakeResolutionKBAsync(Array.Empty<Sentence>())),

            new(
                Problem: BlocksWorldDomain.LargeExampleProblem,
                Strategy: new IgnorePreconditionsGreedySetCover(BlocksWorldDomain.ActionSchemas),
                InvariantsKB: await MakeResolutionKBAsync(new Sentence[]
                {
                    Block(new Function("blockA")),
                    Block(new Function("blockB")),
                    Block(new Function("blockC")),
                    Block(new Function("blockD")),
                    Block(new Function("blockE")),
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
        .And((_, tc, _, pl) => pl.ApplyTo(tc.Problem.InitialState).Meets(tc.Problem.EndGoal).Should().BeTrue())
        .And((cxt, tc, _, pl) => cxt.WriteOutputLine(new PlanFormatter(tc.Problem).Format(pl)));

    private static async Task<IKnowledgeBase> MakeResolutionKBAsync(IEnumerable<Sentence> invariants)
    {
        var resolutionKb = new ResolutionKnowledgeBase(new DelegateResolutionStrategy(
            new HashSetClauseStore(),
            ClauseResolutionFilters.None,
            ClauseResolutionPriorityComparisons.UnitPreference));

        var invariantKb = new UniqueNamesAxiomisingKnowledgeBase(await EqualityAxiomisingKnowledgeBase.CreateAsync(resolutionKb));
        invariantKb.Tell(invariants);

        return invariantKb;
    }
}
