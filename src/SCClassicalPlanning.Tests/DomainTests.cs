using FluentAssertions;
using FlUnit;
using SCClassicalPlanning.ExampleDomains;
using SCClassicalPlanning.ExampleDomains.FromAIaMA;
using SCFirstOrderLogic;
using static SCClassicalPlanning.ExampleDomains.FromAIaMA.AirCargo;
using static SCClassicalPlanning.ExampleDomains.FromAIaMA.BlocksWorld;
using static SCClassicalPlanning.ExampleDomains.FromAIaMA.SpareTire;
using static SCClassicalPlanning.ExampleDomains.Container;
using static SCFirstOrderLogic.SentenceCreation.OperableSentenceFactory;

namespace SCClassicalPlanning
{
    public static class DomainTests
    {
        private static readonly Constant element1 = new(nameof(element1));
        private static readonly Constant element2 = new(nameof(element2));

        private record ConstructionTestCase(Domain Domain, IEnumerable<Predicate> ExpectedPredicates, IEnumerable<Constant> ExpectedConstants);

        public static Test ConstructionBehaviour => TestThat
            .GivenEachOf(() => new ConstructionTestCase[]
            {
                new(
                    Domain: Container.Domain,
                    ExpectedPredicates: new Predicate[] { IsPresent(A) },
                    ExpectedConstants: Array.Empty<Constant>()),

                new(
                    Domain: AirCargo.Domain,
                    ExpectedPredicates: new Predicate[]
                    {
                        Cargo(A),
                        Plane(A),
                        Airport(A),
                        At(A, B),
                        In(A, B),
                    },
                    ExpectedConstants: Array.Empty<Constant>()),

                new(
                    Domain: BlocksWorld.Domain,
                    ExpectedPredicates: new Predicate[]
                    {
                        On(A, B),
                        Block(A),
                        Clear(A),
                        Equal(A, B),
                    },
                    ExpectedConstants: new[] { Table }),

                new(
                    Domain: SpareTire.Domain,
                    ExpectedPredicates: new Predicate[]
                    {
                        IsTire(A),
                        IsAt(A, B),
                    },
                    ExpectedConstants: new[] { Spare, Flat, Ground, Axle, Trunk }),
            })
            .When(tc => tc.Domain) // ... :/ yeah, pointless, would be better to invoke ctor in the test action. Means the test name is a bit of a lie, but.. meh.
            .ThenReturns()
            .And((tc, r) => r.Predicates.Should().BeEquivalentTo(tc.ExpectedPredicates))
            .And((tc, r) => r.Constants.Should().BeEquivalentTo(tc.ExpectedConstants));
    }
}