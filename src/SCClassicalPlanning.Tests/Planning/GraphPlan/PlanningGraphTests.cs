using FluentAssertions;
using FlUnit;
using SCClassicalPlanning.ExampleDomains.FromAIaMA;
using SCFirstOrderLogic;
using static SCClassicalPlanning.ExampleDomains.FromAIaMA.BlocksWorld;
using static SCClassicalPlanning.ExampleDomains.FromAIaMA.HaveCakeAndEatCakeToo;

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
            .When(tc => new PlanningGraph(tc.Problem, tc.Problem.InitialState))
            .ThenReturns()
            .And((tc, rv) => rv.GetPropositions(0).Should().BeEquivalentTo(tc.ExpectedLayer0Propositions.OfType<Literal>()));

        // NB: yeah, pretty terrible coverage (testing large outputs is generally a PITA)
        // note that it gets indirectly tested via the PlanningGraph heuristics tests.
        // Will also extend at some point - should at least completely cover the cake example -
        // which i think has only 2 levels..
    }
}
