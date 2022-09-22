using FluentAssertions;
using FlUnit;
using SCClassicalPlanning;
using SCClassicalPlanning.ExampleDomains;
using SCClassicalPlanning.Planning;
using SCClassicalPlanning.Planning.StateSpaceSearch;
using static SCClassicalPlanning.ExampleDomains.AirCargo;
using static SCClassicalPlanning.ExampleDomains.BlocksWorld;
using static SCClassicalPlanning.ExampleDomains.SpareTire;

namespace SCAutomatedPlanning.Planning.StateSpaceSearch
{
    internal class ForwardStateSpaceSearchTest
    {
        public static Test AirCargoScenario => TestThat
            .GivenTestContext()
            .And(() =>
            {
                Variable cargo1 = new(nameof(cargo1));
                Variable cargo2 = new(nameof(cargo2));
                Variable plane1 = new(nameof(plane1));
                Variable plane2 = new(nameof(plane2));
                Variable sfo = new(nameof(sfo));
                Variable jfk = new(nameof(jfk));

                return new TestCase(
                    Domain: AirCargo.Domain,
                    InitialState:
                        Cargo(cargo1)
                        & Cargo(cargo2)
                        & Plane(plane1)
                        & Plane(plane2)
                        & Airport(jfk)
                        & Airport(sfo)
                        & At(cargo1, sfo)
                        & At(cargo2, jfk)
                        & At(plane1, sfo)
                        & At(plane2, jfk),
                    GoalState:
                        At(cargo1, jfk)
                        & At(cargo2, sfo));
            })
            .When((_, tc) => tc.Execute())
            .ThenReturns()
            .And((_, tc, p) => p.ApplyTo(tc.InitialState).Elements.IsSupersetOf(tc.GoalState.Elements).Should().BeTrue())
            .And((cxt, _, p) => cxt.WriteOutputLine(new PlanFormatter().Format(p)));

        public static Test BlocksWorldScenario => TestThat
            .GivenTestContext()
            .And(() =>
            {
                Variable blockA = new(nameof(blockA));
                Variable blockB = new(nameof(blockB));
                Variable blockC = new(nameof(blockC));

                return new TestCase(
                    Domain: BlocksWorld.Domain,
                    InitialState:
                        Block(blockA)
                        & Block(blockB)
                        & Block(blockC)
                        & On(blockA, Table)
                        & On(blockB, Table)
                        & On(blockC, blockA)
                        & Clear(blockB)
                        & Clear(blockC),
                    GoalState:
                        On(blockA, blockB)
                        & On(blockB, blockC));
            })
            .When((_, tc) => tc.Execute())
            .ThenReturns()
            .And((_, tc, p) => p.ApplyTo(tc.InitialState).Elements.IsSupersetOf(tc.GoalState.Elements).Should().BeTrue())
            .And((cxt, _, p) => cxt.WriteOutputLine(new PlanFormatter().Format(p)));

        public static Test SpareTireScenario => TestThat
            .GivenTestContext()
            .And(() =>
            {
                return new TestCase(
                    Domain: SpareTire.Domain,
                    InitialState:
                        ImplicitState
                        & IsAt(Flat, Axle)
                        & IsAt(Spare, Trunk),
                    GoalState:
                        IsAt(Spare, Axle));
            })
            .When((_, tc) => tc.Execute())
            .ThenReturns()
            .And((_, tc, p) => p.ApplyTo(tc.InitialState).Elements.IsSupersetOf(tc.GoalState.Elements).Should().BeTrue())
            .And((cxt, _, p) => cxt.WriteOutputLine(new PlanFormatter().Format(p)));

        private record TestCase(Domain Domain, State InitialState, State GoalState)
        {
            public IPlan Execute()
            {
                var planner = new ForwardStateSpaceSearch((s, g) => 0);
                var problem = new Problem(Domain, InitialState, GoalState);
                return planner.CreatePlanAsync(problem).GetAwaiter().GetResult();
            }
        }
    }
}
