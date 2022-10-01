using FluentAssertions;
using FlUnit;
using SCClassicalPlanning.ExampleDomains;
using SCFirstOrderLogic;
using static SCClassicalPlanning.ExampleDomains.AirCargo;
using static SCClassicalPlanning.ExampleDomains.BlocksWorld;
using static SCClassicalPlanning.ExampleDomains.SpareTire;

namespace SCClassicalPlanning.Planning.StateSpaceSearch
{
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
                Constant sfo = new(nameof(sfo));
                Constant jfk = new(nameof(jfk));

                return new TestCase(
                    Domain: AirCargo.Domain,
                    InitialState: new(
                        Cargo(cargo1)
                        & Cargo(cargo2)
                        & Plane(plane1)
                        & Plane(plane2)
                        & Airport(jfk)
                        & Airport(sfo)
                        & At(cargo1, sfo)
                        & At(cargo2, jfk)
                        & At(plane1, sfo)
                        & At(plane2, jfk)),
                    Goal: new(
                        At(cargo1, jfk)
                        & At(cargo2, sfo)));
            })
            .When((_, tc) => tc.Execute())
            .ThenReturns()
            .And((_, tc, p) => tc.Goal.IsSatisfiedBy(p.ApplyTo(tc.InitialState)).Should().BeTrue())
            .And((cxt, _, p) => cxt.WriteOutputLine(new PlanFormatter().Format(p)));

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
                        & Block(blockB)
                        & Block(blockC)
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
            .And((cxt, _, p) => cxt.WriteOutputLine(new PlanFormatter().Format(p)));

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
            .And((cxt, _, p) => cxt.WriteOutputLine(new PlanFormatter().Format(p)));

        private record TestCase(Domain Domain, State InitialState, Goal Goal)
        {
            public IPlan Execute()
            {
                var planner = new BackwardStateSpaceSearch((s, g) => 0);
                var problem = new Problem(Domain, InitialState, Goal);
                return planner.CreatePlanAsync(problem).GetAwaiter().GetResult();
            }
        }
    }
}
