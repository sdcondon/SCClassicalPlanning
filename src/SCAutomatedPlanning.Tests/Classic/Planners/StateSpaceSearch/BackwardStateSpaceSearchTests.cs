using FluentAssertions;
using FlUnit;
using SCAutomatedPlanning.Classical;
using SCAutomatedPlanning.Classical.Planners;
using SCAutomatedPlanning.Classical.Planners.StateSpaceSearch;
using SCAutomatedPlanning.ExampleProblems.Classical;
using static SCAutomatedPlanning.ExampleProblems.Classical.AirCargo;
using static SCAutomatedPlanning.ExampleProblems.Classical.BlocksWorld;
using static SCAutomatedPlanning.ExampleProblems.Classical.SpareTire;

namespace SCAutomatedPlanning.Classic.Planners.StateSpaceSearch
{
    public static class BackwardStateSpaceSearchTests
    {
        public static Test AirCargoScenario => TestThat
            .Given(() => MakePlanTask(
                initialState:
                    AirCargo.ImplicitState
                    & At(Cargo1, SFO)
                    & At(Cargo2, JFK)
                    & At(Plane1, SFO)
                    & At(Plane2, JFK),
                goalState:
                    At(Cargo1, JFK)
                    & At(Cargo2, SFO),
                actions: AirCargo.Actions))
            .When(t => t.GetAwaiter().GetResult())
            .ThenReturns()
            .And((_, r) => r.Steps.Should().BeEquivalentTo(new Classical.Action[]
            {
                Load(Cargo1, Plane1, SFO),
                Fly(Plane1, SFO, JFK),
                Unload(Cargo1, Plane1, JFK),
                Load(Cargo2, Plane2, JFK),
                Fly(Plane2, JFK, SFO),
                Unload(Cargo2, Plane2, SFO),
            }));

        public static Test BlocksWorldScenario => TestThat
            .Given(() => MakePlanTask(
                initialState:
                    BlocksWorld.ImplicitState
                    & On(BlockA, Table)
                    & On(BlockB, Table)
                    & On(BlockC, BlockA)
                    & Clear(BlockB)
                    & Clear(BlockC),
                goalState:
                    On(BlockA, BlockB)
                    & On(BlockB, BlockC),
                actions: BlocksWorld.Actions))
            .When(t => t.GetAwaiter().GetResult())
            .ThenReturns()
            .And((_, r) => r.Steps.Should().BeEquivalentTo(new Classical.Action[]
            {
                MoveToTable(BlockC, BlockA),
                Move(BlockB, Table, BlockC),
                Move(BlockA, Table, BlockB),
            }));

        public static Test SpareTireScenario => TestThat
            .Given(() => MakePlanTask(
                initialState:
                    SpareTire.ImplicitState
                    & IsAt(Flat, Axle)
                    & IsAt(Spare, Trunk),
                goalState:
                    IsAt(Spare, Axle),
                actions: SpareTire.Actions))
            .When(t => t.GetAwaiter().GetResult())
            .ThenReturns()
            .And((_, r) => r.Steps.Should().BeEquivalentTo(new Classical.Action[]
            {
                Remove(Flat, Axle),
                Remove(Spare, Trunk),
                PutOn(Spare),
            }));

        private static Task<IPlan> MakePlanTask(State initialState, State goalState, IEnumerable<Classical.Action> actions)
        {
            var planner = new BackwardStateSpaceSearch();

            foreach (var action in actions)
            {
                planner.Actions.Add(action);
            }

            return planner.CreatePlanAsync(initialState, goalState);
        }
    }
}
