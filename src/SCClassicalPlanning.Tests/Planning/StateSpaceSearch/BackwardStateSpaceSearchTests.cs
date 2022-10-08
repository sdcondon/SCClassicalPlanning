using FluentAssertions;
using FlUnit;
using SCClassicalPlanning.ExampleDomains.FromAIaMA;
using SCClassicalPlanning.Planning.StateSpaceSearch.Heuristics;
using SCFirstOrderLogic;
using static SCClassicalPlanning.ExampleDomains.FromAIaMA.AirCargo;
using static SCClassicalPlanning.ExampleDomains.FromAIaMA.BlocksWorld;
using static SCClassicalPlanning.ExampleDomains.FromAIaMA.SpareTire;

namespace SCClassicalPlanning.Planning.StateSpaceSearch
{
#if false
    public static class BackwardStateSpaceSearchTests
    {
        public static Test AirCargoScenario => TestThat
            .GivenTestContext()
            .And(() =>
            {
                Constant cargo1 = new(nameof(cargo1));
                Constant cargo2 = new(nameof(cargo2));
                Constant plane1 = new(nameof(plane1));
                Constant plane2 = new(nameof(plane2));
                Constant airport1 = new(nameof(airport1));
                Constant airport2 = new(nameof(airport2));

                return new TestCase(
                    Domain: AirCargo.Domain,
                    InitialState: new(
                        Cargo(cargo1)
                        & Cargo(cargo2)
                        & Plane(plane1)
                        & Plane(plane2)
                        & Airport(airport1)
                        & Airport(airport2)
                        & At(cargo1, airport2)
                        & At(cargo2, airport1)
                        & At(plane1, airport1)
                        & At(plane2, airport2)),
                    Goal: new(
                        At(cargo1, airport1)
                        & At(cargo2, airport2)));
            })
            .When((_, tc) => tc.Execute())
            .ThenReturns()
            .And((_, tc, p) => tc.Goal.IsSatisfiedBy(p.ApplyTo(tc.InitialState)).Should().BeTrue())
            .And((cxt, _, p) => cxt.WriteOutputLine(new PlanFormatter(AirCargo.Domain).Format(p)));

        public static Test BlocksWorldScenario => TestThat
            .GivenTestContext()
            .And(() =>
            {
                Constant blockA = new(nameof(blockA));
                Constant blockB = new(nameof(blockB));
                Constant blockC = new(nameof(blockC));

                return new TestCase(
                    Domain: BlocksWorld.Domain,
                    InitialState: new(
                        Block(blockA)
                        & Equal(blockA, blockA)
                        & Block(blockB)
                        & Equal(blockB, blockB)
                        & Block(blockC)
                        & Equal(blockC, blockC)
                        & On(blockA, Table)
                        & On(blockB, Table)
                        & On(blockC, blockA)
                        & Clear(blockB)
                        & Clear(blockC)),
                    Goal: new(
                        On(blockA, blockB)
                        & On(blockB, blockC)));
            })
            .When((_, tc) => tc.Execute())
            .ThenReturns()
            .And((_, tc, p) => tc.Goal.IsSatisfiedBy(p.ApplyTo(tc.InitialState)).Should().BeTrue())
            .And((cxt, _, p) => cxt.WriteOutputLine(new PlanFormatter(BlocksWorld.Domain).Format(p)));

        public static Test BigBlocksWorldScenario => TestThat
            .GivenTestContext()
            .And(() =>
            {
                Constant blockA = new(nameof(blockA));
                Constant blockB = new(nameof(blockB));
                Constant blockC = new(nameof(blockC));
                Constant blockD = new(nameof(blockD));
                Constant blockE = new(nameof(blockE));

                return new TestCase(
                    Domain: BlocksWorld.Domain,
                    InitialState: new(
                        Block(blockA)
                        & Equal(blockA, blockA)
                        & Block(blockB)
                        & Equal(blockB, blockB)
                        & Block(blockC)
                        & Equal(blockC, blockC)
                        & Block(blockD)
                        & Equal(blockD, blockD)
                        & Block(blockE)
                        & Equal(blockE, blockE)
                        & On(blockA, Table)
                        & On(blockB, Table)
                        & On(blockC, blockA)
                        & On(blockD, blockB)
                        & On(blockE, Table)
                        & Clear(blockC)
                        & Clear(blockD)
                        & Clear(blockE)),
                    Goal: new(
                        On(blockA, blockB)
                        & On(blockB, blockC)
                        & On(blockC, blockD)
                        & On(blockD, blockE)));
            })
            .When((_, tc) => tc.Execute())
            .ThenReturns()
            .And((_, tc, p) => tc.Goal.IsSatisfiedBy(p.ApplyTo(tc.InitialState)).Should().BeTrue())
            .And((cxt, _, p) => cxt.WriteOutputLine(new PlanFormatter(BlocksWorld.Domain).Format(p)));

        public static Test SpareTireScenario => TestThat
            .GivenTestContext()
            .And(() =>
            {
                return new TestCase(
                    Domain: SpareTire.Domain,
                    InitialState: new(
                        SpareTire.ImplicitState
                        & IsAt(Flat, Axle)
                        & IsAt(Spare, Trunk)),
                    Goal: new(
                        IsAt(Spare, Axle)));
            })
            .When((_, tc) => tc.Execute())
            .ThenReturns()
            .And((_, tc, p) => tc.Goal.IsSatisfiedBy(p.ApplyTo(tc.InitialState)).Should().BeTrue())
            .And((cxt, _, p) => cxt.WriteOutputLine(new PlanFormatter(SpareTire.Domain).Format(p)));

        private record TestCase(Domain Domain, State InitialState, Goal Goal)
        {
            public Plan Execute()
            {
                var problem = new Problem(Domain, InitialState, Goal);
                var heuristic = new IgnorePreconditionsGreedySetCover(problem);
                var planner = new BackwardStateSpaceSearch(heuristic.EstimateCost);
                return planner.CreatePlanAsync(problem).GetAwaiter().GetResult();
            }
        }
    }
#endif
}
