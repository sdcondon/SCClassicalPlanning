using FluentAssertions;
using FlUnit;
using SCClassicalPlanning.ExampleDomains.AsCode;
using SCFirstOrderLogic;
using static SCClassicalPlanning.ExampleDomains.AsCode.AirCargo;
using static SCClassicalPlanning.ExampleDomains.AsCode.BlocksWorld;
using static SCClassicalPlanning.ExampleDomains.AsCode.Container;
using static SCClassicalPlanning.ExampleDomains.AsCode.SpareTire;
using static SCFirstOrderLogic.SentenceCreation.OperableSentenceFactory;

namespace SCClassicalPlanning
{
    public static class DomainTests
    {
        private static readonly Constant element1 = new(nameof(element1));
        private static readonly Constant element2 = new(nameof(element2));

        private record ConstructionTestCase(Func<Domain> MakeDomain, IEnumerable<Predicate> ExpectedPredicates, IEnumerable<Constant> ExpectedConstants);

        public static Test ConstructionBehaviour => TestThat
            .GivenEachOf<ConstructionTestCase>(() => new ConstructionTestCase[]
            {
                new(
                    MakeDomain: Container.MakeDomain,
                    ExpectedPredicates: new Predicate[] { IsPresent(A) },
                    ExpectedConstants: Array.Empty<Constant>()),

                new(
                    MakeDomain: AirCargo.MakeDomain,
                    ExpectedPredicates: new Predicate[]
                    {
                        Cargo(A),
                        Plane(A),
                        Airport(A),
                        At(A, B),
                        In(A, B),
                        Equal(A, B),
                    },
                    ExpectedConstants: Array.Empty<Constant>()),

                new(
                    MakeDomain: BlocksWorld.MakeDomain,
                    ExpectedPredicates: new Predicate[]
                    {
                        On(A, B),
                        Block(A),
                        Clear(A),
                        Equal(A, B),
                    },
                    ExpectedConstants: new[] { Table }),

                new(
                    MakeDomain: SpareTire.MakeDomain,
                    ExpectedPredicates: new Predicate[]
                    {
                        IsTire(A),
                        IsAt(A, B),
                    },
                    ExpectedConstants: new[] { Spare, Flat, Ground, Axle, Trunk }),
            })
            .When<Domain>(tc => tc.MakeDomain()) 
            .ThenReturns()
            .And((tc, r) => r.Predicates.Should().BeEquivalentTo(tc.ExpectedPredicates))
            .And((tc, r) => r.Constants.Should().BeEquivalentTo(tc.ExpectedConstants));
    }
}