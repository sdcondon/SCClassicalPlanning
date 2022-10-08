using FluentAssertions;
using FlUnit;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using SCClassicalPlanning.ExampleDomains;
using SCClassicalPlanning.ExampleDomains.FromAIaMA;
using SCFirstOrderLogic;
using System.Numerics;
using static SCClassicalPlanning.ExampleDomains.Container;
using static SCClassicalPlanning.ExampleDomains.FromAIaMA.AirCargo;

namespace SCClassicalPlanning.Planning
{
    public static class ProblemInspectorTests
    {
        private static readonly Constant element1 = new(nameof(element1));
        private static readonly Constant element2 = new(nameof(element2));
        private static readonly Problem containerProblem = new Problem(Container.Domain, State.Empty, Goal.Empty, new[] { element1, element2 });

        private record GetApplicableActionsTestCase(Problem Problem, State State, Action[] ExpectedResult);

        public static Test GetApplicableActionsBehaviour => TestThat
            .GivenEachOf(() => new GetApplicableActionsTestCase[]
            {
                new(
                    Problem: containerProblem,
                    State: State.Empty,
                    ExpectedResult: new[] { Add(element1), Add(element2) }),

                new(
                    Problem: containerProblem,
                    State: new(IsPresent(element1)),
                    ExpectedResult: new[] { Remove(element1), Add(element2), Swap(element1, element2) }),

                new(
                    Problem: containerProblem,
                    State: new(IsPresent(element1) & IsPresent(element2)),
                    ExpectedResult: new[] { Remove(element1), Remove(element2) }),
            })
            .When(tc => ProblemInspector.GetApplicableActions(tc.Problem, tc.State))
            .ThenReturns()
            .And((tc, r) => r.Should().BeEquivalentTo(tc.ExpectedResult));

        private record GetRelevantActionsTestCase(Problem Problem, Goal Goal, Action[] ExpectedResult);

        public static Test GetRelevantActionsBehaviour => TestThat
            .GivenEachOf(() => new GetRelevantActionsTestCase[]
            {
                new(
                    Problem: containerProblem,
                    Goal: new(IsPresent(element1)),
                    ExpectedResult: new[]
                    {
                        Add(element1),
                        Swap(element2, element1),
                    }),

                new(
                    Problem: containerProblem,
                    Goal: new(IsPresent(element1) & IsPresent(element2)),
                    ExpectedResult: new[]
                    {
                        Add(element1),
                        Add(element2),
                    }),

                new(
                    Problem: containerProblem,
                    Goal: new(IsPresent(element1) & !IsPresent(element2)),
                    ExpectedResult: new[]
                    {
                        Add(element1),
                        Remove(element2),
                        Swap(element2, element1),
                        Swap(element2, element1), // TODO-BUG: yeah, hits twice - for the element1 presence and for the element2 non-presence)
                    }),

                new(
                    Problem: containerProblem,
                    Goal: new(!IsPresent(element1) & !IsPresent(element2)),
                    ExpectedResult: new[]
                    {
                        Remove(element1),
                        Remove(element2),
                    }),

                new(
                    Problem: new Problem(
                        domain: AirCargo.Domain,
                        initialState: new(
                            Cargo(new Constant("cargo"))
                            & Plane(new Constant("plane1"))
                            & Plane(new Constant("plane2"))
                            & Airport(new Constant("sfo"))
                            & At(new Constant("cargo"), new Constant("sfo"))
                            & At(new Constant("plane1"), new Constant("sfo"))
                            & At(new Constant("plane2"), new Constant("sfo"))),
                        goal: Goal.Empty),
                    Goal: new(At(new Constant("cargo"), new Constant("sfo"))),
                    ExpectedResult: new Action[]
                    {
                        // the important bit - leaves variables alone if it can
                        Unload(new Constant("cargo"), new VariableReference("plane"), new Constant("sfo")),

                        // These three serve as an interesting indication of why backward state space searching
                        // is so slow with this lib. Obviously none of these actions make sense - their preconditions could
                        // never be satisfied (e.g. cargo will never be a plane, plane1 will never be an airport, etc)
                        // but of course these constraints are expressed as preconditions, and relevancy (as we implement it -
                        // in its most basic form) is nothing to do with preconditions per se.
                        //
                        // A good heuristic will of course note that the pre-conditions are unreachable
                        // and the associated state space edges will thus never be explored. But these non-starters
                        // are generated at every step and take a lot of time to generate and chew through. This
                        // is a good example of why even very simple/early versions of PDDL include a TYPED
                        // predicate parameter lists.. For the purposes of this lib (without any extensions), its 
                        // an example of why its not a good idea to re-use predicates for different "types".
                        Fly(new Constant("cargo"), new Constant("plane1"), new Constant("sfo")),
                        Fly(new Constant("cargo"), new Constant("plane2"), new Constant("sfo")),
                        Fly(new Constant("cargo"), new Constant("cargo"), new Constant("sfo")),
                    }),
            })
            .When(tc => ProblemInspector.GetRelevantActions(tc.Problem, tc.Goal))
            .ThenReturns()
            .And((tc, r) => r.Should().BeEquivalentTo(tc.ExpectedResult));
    }
}
