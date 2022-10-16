////using FluentAssertions;
////using FlUnit;
////using SCClassicalPlanning.ExampleDomains.FromAIaMA;
////using SCFirstOrderLogic;
////using static SCClassicalPlanning.ExampleDomains.FromAIaMA.BlocksWorld;

////namespace SCClassicalPlanning.Planning.GraphPlan
////{
////    public static class GraphPlanTests
////    {
////        public static Test CreatedPlanValidity => TestThat
////            .GivenTestContext()
////            .AndEachOf(() => new Problem[]
////            {
////                AirCargo.ExampleProblem,
////                BlocksWorld.ExampleProblem,
////                SpareTire.ExampleProblem,
////                MakeBigBlocksWorldProblem(),
////            })
////            .When((_, tc) =>
////            {
////                var planner = new GraphPlan();
////                return planner.CreatePlanAsync(tc).GetAwaiter().GetResult();
////            })
////            .ThenReturns()
////            .And((_, tc, pl) => tc.Goal.IsSatisfiedBy(pl.ApplyTo(tc.InitialState)).Should().BeTrue())
////            .And((cxt, tc, pl) => cxt.WriteOutputLine(new PlanFormatter(tc.Domain).Format(pl)));

////        private static Problem MakeBigBlocksWorldProblem()
////        {
////            Constant blockA = new(nameof(blockA));
////            Constant blockB = new(nameof(blockB));
////            Constant blockC = new(nameof(blockC));
////            Constant blockD = new(nameof(blockD));
////            Constant blockE = new(nameof(blockE));

////            return BlocksWorld.MakeProblem(
////                initialState: new(
////                    Block(blockA)
////                    & Equal(blockA, blockA)
////                    & Block(blockB)
////                    & Equal(blockB, blockB)
////                    & Block(blockC)
////                    & Equal(blockC, blockC)
////                    & Block(blockD)
////                    & Equal(blockD, blockD)
////                    & Block(blockE)
////                    & Equal(blockE, blockE)
////                    & On(blockA, Table)
////                    & On(blockB, Table)
////                    & On(blockC, blockA)
////                    & On(blockD, blockB)
////                    & On(blockE, Table)
////                    & Clear(blockD)
////                    & Clear(blockE)
////                    & Clear(blockC)),
////                goal: new(
////                    On(blockA, blockB)
////                    & On(blockB, blockC)
////                    & On(blockC, blockD)
////                    & On(blockD, blockE)));
////        }
////    }
////}
