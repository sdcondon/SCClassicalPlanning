using FluentAssertions;
using FlUnit;
using SCClassicalPlanning;
using SCClassicalPlanning.ExampleDomains;
using SCClassicalPlanning.Planning;
using SCClassicalPlanning.Planning.StateSpaceSearch;
using SCFirstOrderLogic;
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
                VariableReference cargo1 = new(nameof(cargo1));
                VariableReference cargo2 = new(nameof(cargo2));
                VariableReference plane1 = new(nameof(plane1));
                VariableReference plane2 = new(nameof(plane2));
                VariableReference sfo = new(nameof(sfo));
                VariableReference jfk = new(nameof(jfk));

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
                    GoalState: new(
                        At(cargo1, jfk)
                        & At(cargo2, sfo)));
            })
            .When((_, tc) => tc.Execute())
            .ThenReturns()
            .And((_, tc, p) => p.ApplyTo(tc.InitialState).Elements.IsSupersetOf(tc.GoalState.Elements).Should().BeTrue())
            .And((cxt, _, p) => cxt.WriteOutputLine(new PlanFormatter().Format(p)));

        public static Test BlocksWorldScenario => TestThat
            .GivenTestContext()
            .And(() =>
            {
                VariableReference blockA = new(nameof(blockA));
                VariableReference blockB = new(nameof(blockB));
                VariableReference blockC = new(nameof(blockC));

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
                    GoalState: new(
                        On(blockA, blockB)
                        & On(blockB, blockC)));
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
                    InitialState: new(
                        SpareTire.ImplicitState
                        & IsAt(Flat, Axle)
                        & IsAt(Spare, Trunk)),
                    GoalState: new(
                        IsAt(Spare, Axle)));
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
