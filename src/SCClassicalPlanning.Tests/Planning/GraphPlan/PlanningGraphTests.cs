using FluentAssertions;
using FlUnit;
using SCClassicalPlanning.ExampleDomains.FromAIaMA;
using SCFirstOrderLogic;
using static SCClassicalPlanning.ExampleDomains.FromAIaMA.BlocksWorld;
using static SCClassicalPlanning.ExampleDomains.FromAIaMA.HaveCakeAndEatCakeToo;
using static SCClassicalPlanning.ExampleDomains.FromAIaMA.SpareTire;

namespace SCClassicalPlanning.Planning.GraphPlan
{
    public static class PlanningGraphTests
    {
        private static readonly Constant blockA = new(nameof(blockA));
        private static readonly Constant blockB = new(nameof(blockB));
        private static readonly Constant blockC = new(nameof(blockC));
        private static readonly Constant Table = new(nameof(Table));

        private record ConstructionTestCase(Problem Problem, ISet<Literal> ExpectedLayer0Propositions);

        public static Test LargeGraphConstructionBehaviour => TestThat
            .GivenEachOf(() => new ConstructionTestCase[]
            {
                new(
                    Problem: BlocksWorld.ExampleProblem,
                    ExpectedLayer0Propositions: new Sentence[]
                    {
                        Block(blockA),
                        Block(blockB),
                        Block(blockC),
                        !Block(Table),

                        Equal(blockA, blockA),
                        !Equal(blockA, blockB),
                        !Equal(blockA, blockC),
                        !Equal(blockA, Table),
                        !Equal(blockB, blockA),
                        Equal(blockB, blockB),
                        !Equal(blockB, blockC),
                        !Equal(blockB, Table),
                        !Equal(blockC, blockA),
                        !Equal(blockC, blockB),
                        Equal(blockC, blockC),
                        !Equal(blockC, Table),
                        !Equal(Table, blockA),
                        !Equal(Table, blockB),
                        !Equal(Table, blockC),
                        !Equal(Table, Table), // Fine - we just don't say this - and there's no implicit stuff about equality in the lib
                        
                        !On(blockA, blockA),
                        !On(blockA, blockB),
                        !On(blockA, blockC),
                        On(blockA, Table),
                        !On(blockB, blockA),
                        !On(blockB, blockB),
                        !On(blockB, blockC),
                        On(blockB, Table),
                        On(blockC, blockA),
                        !On(blockC, blockB),
                        !On(blockC, blockC),
                        !On(blockC, Table),
                        !On(Table, blockA),
                        !On(Table, blockB),
                        !On(Table, blockC),
                        !On(Table, Table),

                        !Clear(blockA),
                        Clear(blockB),
                        Clear(blockC),
                        !Clear(Table) // Also fine, given how the domain works
                    }.Select(s => new Literal(s)).ToHashSet()),
            })
            .When(tc => new PlanningGraph(tc.Problem))
            .ThenReturns()
            .And((tc, rv) => rv.GetLevel(0).Propositions.Should().BeEquivalentTo(tc.ExpectedLayer0Propositions.OfType<Literal>()));

        public static Test SpareTireScenario => TestThat
            .Given(() => new PlanningGraph(SpareTire.ExampleProblem))
            .When(g =>
            {
                static IEnumerable<Predicate> GetPositiveTireIsAtPropositions(PlanningGraph g, int level)
                {
                    return g.GetLevel(level).Propositions
                        .Where(p =>
                            p.IsPositive
                            && p.Predicate.Symbol.Equals("IsAt")
                            && (p.Predicate.Arguments[0].Equals(Spare) || p.Predicate.Arguments[0].Equals(Flat)))
                        .Select(l => l.Predicate);
                }

                return new
                {
                    level0 = GetPositiveTireIsAtPropositions(g, 0),
                    level1 = GetPositiveTireIsAtPropositions(g, 1),
                    level2 = GetPositiveTireIsAtPropositions(g, 2),
                };
            })
            .ThenReturns()
            .And((_, rv) => rv.level0.Should().BeEquivalentTo(new Predicate[]
            {
                IsAt(Spare, Trunk),
                IsAt(Flat, Axle),
            }))
            .And((_, rv) => rv.level1.Should().BeEquivalentTo(new Predicate[]
            {
                // Can put either tire on the ground as first action:
                IsAt(Spare, Trunk),
                IsAt(Spare, Ground),
                IsAt(Flat, Axle),
                IsAt(Flat, Ground),
            }))
            .And((_, rv) => rv.level2.Should().BeEquivalentTo(new Predicate[]
            {
                // Spare can be on the axle after two actions:
                IsAt(Spare, Trunk),
                IsAt(Spare, Axle),
                IsAt(Spare, Ground),
                IsAt(Flat, Axle),
                IsAt(Flat, Ground),
            }));
    }
}
