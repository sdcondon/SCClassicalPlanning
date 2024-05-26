using FluentAssertions;
using FlUnit;
using SCClassicalPlanning.ExampleDomains.AsCode;
using SCFirstOrderLogic;
using static SCClassicalPlanning.ExampleDomains.AsCode.AirCargo;
using static SCClassicalPlanning.ProblemCreation.OperableProblemFactory;

namespace SCClassicalPlanning.Planning.StateAndGoalSpace.CostStrategies;

public static class IgnorePreconditionsGreedySetCoverTests
{
    private static readonly Constant cargo1 = new(nameof(cargo1));
    private static readonly Constant cargo2 = new(nameof(cargo2));
    private static readonly Constant airport1 = new(nameof(airport1));
    private static readonly Constant airport2 = new(nameof(airport2));

    private static readonly VariableDeclaration somePlane = new(nameof(somePlane));

    private record TestCase(IDomain Domain, IState State, OperableGoal Goal, float ExpectedCost);

    public static Test EstimateCostBehaviour => TestThat
        .GivenEachOf(() => new TestCase[]
        {
            // Unload (or indeed Fly - ignoring preconds means ignoring 'type'..) cargo1 to airport1,
            // = 1
            new TestCase(
                Domain: AirCargo.Domain,
                State: AirCargo.ExampleProblem.InitialState,
                Goal: At(cargo1, airport2),
                ExpectedCost: 1),

            // Unload (or indeed Fly - ignoring preconds means ignoring 'type'..) cargo1 to airport1,
            // Unload (or indeed Fly - ignoring preconds means ignoring 'type'..) cargo2 to airport2,
            // = 2
            new TestCase(
                Domain: AirCargo.Domain,
                State: AirCargo.ExampleProblem.InitialState,
                Goal: At(cargo1, airport2) & At(cargo2, airport1),
                ExpectedCost: 2),

            // e.g. the regression of Fly(cargo2, airport2, airport1) from the "standard" air cargo goal
            // Impossible - cargo can never be a plane
            // = float.PositiveInfinity
            new TestCase(
                Domain: AirCargo.Domain,
                State: AirCargo.ExampleProblem.InitialState,
                Goal: At(cargo2, airport1) & Plane(cargo1) & Airport(airport2) & Airport(airport1),
                ExpectedCost: float.PositiveInfinity),

            // e.g. the regression of Unload(cargo2, somePlane, airport1) includes a variable
            new TestCase(
                Domain: AirCargo.Domain,
                State: AirCargo.ExampleProblem.InitialState,
                Goal: Plane(somePlane) 
                    & In(cargo2, somePlane)
                    & At(somePlane, airport1)
                    & Cargo(cargo2)
                    & Airport(airport1),
                ExpectedCost: 1),

            // e.g. the result of a Unload(cargo2, somePlane, airport1) being seen as relevant to the goal state - NB includes a variable
            // As above - Plane(somePlane) is 2..
            new TestCase(
                Domain: AirCargo.Domain,
                State: AirCargo.ExampleProblem.InitialState,
                Goal: Plane(somePlane) // met by state
                    & In(cargo2, somePlane) // load
                    & At(somePlane, airport1) // met by state
                    & Cargo(cargo2) // met by state
                    & Airport(airport1) // met by state

                    // the 'other' inital goal, that is not affected by the regression..
                    & At(cargo1, airport2), // unload (or fly)
                ExpectedCost: 2),
        })
        .When(tc => new IgnorePreconditionsGreedySetCover(tc.Domain).EstimateCost(tc.State, tc.Goal))
        .ThenReturns()
        .And((tc, rv) => rv.Should().Be(tc.ExpectedCost));
}
