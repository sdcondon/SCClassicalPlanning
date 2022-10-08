using FluentAssertions;
using FlUnit;
using SCClassicalPlanning.ExampleDomains.FromAIaMA;
using SCClassicalPlanning.Planning.StateSpaceSearch.Heuristics;
using SCFirstOrderLogic;
using System.Numerics;
using static SCClassicalPlanning.ExampleDomains.FromAIaMA.AirCargo;
using static SCClassicalPlanning.ExampleDomains.FromAIaMA.BlocksWorld;
using static SCClassicalPlanning.ExampleDomains.FromAIaMA.SpareTire;
using static SCClassicalPlanning.ProblemCreation.OperableProblemFactory;

namespace SCClassicalPlanning.Planning.StateSpaceSearch
{
    public static class IgnorePreconditionsGreedySetCoverTests
    {
        private static readonly Constant cargo1 = new(nameof(cargo1));
        private static readonly Constant cargo2 = new(nameof(cargo2));
        private static readonly Constant plane1 = new(nameof(plane1));
        private static readonly Constant plane2 = new(nameof(plane2));
        private static readonly Constant sfo = new(nameof(sfo));
        private static readonly Constant jfk = new(nameof(jfk));

        private static readonly VariableDeclaration somePlane = new(nameof(somePlane));

        private static readonly State AirCargoInitialState = new(
            Cargo(cargo1)
            & Cargo(cargo2)
            & Plane(plane1)
            & Plane(plane2)
            & Airport(jfk)
            & Airport(sfo)
            & At(cargo1, sfo)
            & At(cargo2, jfk)
            & At(plane1, sfo)
            & At(plane2, jfk));

        private record TestCase(Problem Problem, State State, OperableGoal Goal, float ExpectedCost);

        public static Test EstimateCostBehaviour => TestThat
            .GivenTestContext()
            .AndEachOf(() => new TestCase[]
            {
                ////// Unload (or indeed Fly - ignoring preconds means ignoring 'type'..) cargo1 to jfk,
                ////// = 1
                ////new TestCase(
                ////    Problem: new(AirCargo.Domain, State.Empty, Goal.Empty),
                ////    State: AirCargoInitialState,
                ////    Goal: At(cargo1, jfk),
                ////    ExpectedCost: 1),

                ////// Unload (or indeed Fly - ignoring preconds means ignoring 'type'..) cargo1 to jfk,
                ////// Unload (or indeed Fly - ignoring preconds means ignoring 'type'..) cargo2 to sfo,
                ////// = 2
                ////new TestCase(
                ////    Problem: new(AirCargo.Domain, State.Empty, Goal.Empty),
                ////    State: AirCargoInitialState,
                ////    Goal: At(cargo1, jfk) & At(cargo2, sfo),
                ////    ExpectedCost: 2),

                ////// e.g. the result of a Fly(cargo1, sfo, jfk) being seen as relevant to the goal state and thus regressed
                ////// Impossible - cargo can never be a plane
                ////// = float.PositiveInfinity
                ////new TestCase(
                ////    Problem: new(AirCargo.Domain, State.Empty, Goal.Empty),
                ////    State: AirCargoInitialState,
                ////    Goal: At(cargo1, sfo) & Plane(cargo1) & Airport(sfo) & Airport(jfk),
                ////    ExpectedCost: float.PositiveInfinity),

                // e.g. the result of a Unload(cargo1, sfo, jfk) being seen as relevant to the goal state - NB includes a variable
                // Load(cargo2, somePlane)
                // Fly(plane, sfo) (or Unload(somePlane, plane, sfo) - see above) 
                // Plane(somePlane) not an action, but... meh, im being intuitive here. if we allow unify in satisfied element check, this actually becomes 1..
                new TestCase(
                    Problem: new(AirCargo.Domain, State.Empty, Goal.Empty),
                    State: AirCargoInitialState,
                    Goal: Plane(somePlane) 
                        & In(cargo2, somePlane)
                        & At(somePlane, sfo)
                        & Cargo(cargo2)
                        & Airport(sfo),
                    ExpectedCost: 3),

                // e.g. the result of a Unload(cargo1, sfo, jfk) being seen as relevant to the goal state - NB includes a variable
                // Load(cargo2, somePlane)
                // Unload(cargo, plane, jfk) (or Fly)
                // Plane(somePlane)
                // Fly(somePlane, sfo) (or Unload)
                new TestCase(
                    Problem: new(AirCargo.Domain, State.Empty, Goal.Empty),
                    State: AirCargoInitialState,
                    Goal: Plane(somePlane)
                        & In(cargo2, somePlane)
                        & At(somePlane, sfo)
                        & Cargo(cargo2)
                        & Airport(sfo)
                        & At(cargo1, jfk),
                    ExpectedCost: 4),
            })
            .When((_, tc) => new IgnorePreconditionsGreedySetCover(tc.Problem).EstimateCost(tc.State, tc.Goal))
            .ThenReturns()
            .And((_, tc, rv) => rv.Should().Be(tc.ExpectedCost));
    }
}
