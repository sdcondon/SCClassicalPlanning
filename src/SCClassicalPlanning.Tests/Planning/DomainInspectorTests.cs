using FluentAssertions;
using FlUnit;
using SCClassicalPlanning.ExampleDomains;
using SCClassicalPlanning.ExampleDomains.FromAIaMA;
using SCFirstOrderLogic;
using static SCFirstOrderLogic.SentenceCreation.OperableSentenceFactory;
using static SCClassicalPlanning.ExampleDomains.Container;
using static SCClassicalPlanning.ExampleDomains.FromAIaMA.AirCargo;

namespace SCClassicalPlanning.Planning
{
    public static class DomainInspectorTests
    {
        private static readonly Constant element1 = new(nameof(element1));
        private static readonly Constant element2 = new(nameof(element2));

        private static readonly Constant sfo = new(nameof(sfo));
        private static readonly Constant cargo = new(nameof(cargo));

        private record GetRelevantActionsTestCase(Domain Domain, Goal Goal, Action[] ExpectedResult);

        public static Test GetRelevantActionsBehaviour => TestThat
            .GivenEachOf(() => new GetRelevantActionsTestCase[]
            {
                new(
                    Domain: Container.Domain,
                    Goal: new(IsPresent(element1)),
                    ExpectedResult: new[]
                    {
                        Add(element1),
                        new( // Swap element1 in for something else
                            identifier: "Swap",
                            precondition: new(IsPresent(R) & !IsPresent(element1) & !AreEqual(R, element1)),
                            effect: new(!IsPresent(R) & IsPresent(element1))),
                    }),

                new(
                    Domain: Container.Domain,
                    Goal: new(IsPresent(element1) & IsPresent(element2)),
                    ExpectedResult: new[]
                    {
                        Add(element1),
                        Add(element2),
                        new( // Swap element1 in for something else
                            identifier: "Swap",
                            precondition: new(IsPresent(R) & !IsPresent(element1) & !AreEqual(R, element1) & !AreEqual(R, element2)),
                            effect: new(!IsPresent(R) & IsPresent(element1))),
                        new( // Swap element2 in for something else
                            identifier: "Swap",
                            precondition: new(IsPresent(R) & !IsPresent(element2) & !AreEqual(R, element2) & !AreEqual(R, element1)),
                            effect: new(!IsPresent(R) & IsPresent(element2))),
                    }),

                new(
                    Domain: Container.Domain,
                    Goal: new(IsPresent(element1) & !IsPresent(element2)),
                    ExpectedResult: new[]
                    {
                        Add(element1),
                        Remove(element2),
                        new( // Swap element1 in for something else
                            identifier: "Swap",
                            precondition: new(IsPresent(R) & !IsPresent(element1) & !AreEqual(R, element1)),
                            effect: new(!IsPresent(R) & IsPresent(element1))),
                        new( // Swap element2 out for something else
                            identifier: "Swap",
                            precondition: new(IsPresent(element2) & !IsPresent(A) & !AreEqual(A, element2)),
                            effect: new(IsPresent(A) & !IsPresent(element2))),
                    }),

                new(
                    Domain: Container.Domain,
                    Goal: new(!IsPresent(element1) & !IsPresent(element2)),
                    ExpectedResult: new[]
                    {
                        Remove(element1),
                        Remove(element2),
                        new( // Swap element1 out for something else
                            identifier: "Swap",
                            precondition: new(IsPresent(element1) & !IsPresent(A) & !AreEqual(A, element1) & !AreEqual(A, element2)),
                            effect: new(IsPresent(A) & !IsPresent(element1))),
                        new( // Swap element2 out for something else
                            identifier: "Swap",
                            precondition: new(IsPresent(element2) & !IsPresent(A) & !AreEqual(A, element2) & !AreEqual(A, element1)),
                            effect: new(IsPresent(A) & !IsPresent(element2))),
                    }),

                new(
                    Domain: AirCargo.Domain,
                    Goal: new(At(cargo, sfo)),
                    ExpectedResult: new Action[]
                    {
                        Unload(cargo, new VariableReference("plane"), sfo),
                        new(
                            identifier: "Fly",
                            precondition: new(Airport(sfo) & At(cargo, new VariableReference("from")) & !AreEqual(new VariableReference("from"), sfo) & Plane(cargo) & Airport(new VariableReference("from"))),
                            effect: new(!At(cargo, new VariableReference("from")) & At(cargo, sfo))),
                    }),
            })
            .When(tc => DomainInspector.GetRelevantActions(tc.Domain, tc.Goal))
            .ThenReturns()
            .And((tc, r) => r.Should().BeEquivalentTo(tc.ExpectedResult));
    }
}
