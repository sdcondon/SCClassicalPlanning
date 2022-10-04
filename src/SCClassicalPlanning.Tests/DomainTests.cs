#if false
using FluentAssertions;
using FlUnit;
using SCClassicalPlanning.ExampleDomains;
using SCFirstOrderLogic;
using static SCClassicalPlanning.ExampleDomains.AirCargo;
using static SCClassicalPlanning.ExampleDomains.Container;
using static SCFirstOrderLogic.SentenceCreation.OperableSentenceFactory;

namespace SCClassicalPlanning
{
    public static class DomainTests
    {
        private static readonly Constant element1 = new(nameof(element1));
        private static readonly Constant element2 = new(nameof(element2));

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
                        Swap(R, element1),
                    }),

                new(
                    Domain: Container.Domain,
                    Goal: new(IsPresent(element1) & IsPresent(element2)),
                    ExpectedResult: new[]
                    {
                        Add(element1),
                        Add(element2),
                        //Swap(R, element1) where R != element2,
                        //Swap(R, element2) where R != element1,
                    }),

                new(
                    Domain: Container.Domain,
                    Goal: new(IsPresent(element1) & !IsPresent(element2)),
                    ExpectedResult: new[]
                    {
                        Add(element1),
                        Remove(element2),
                        Swap(R, element1),
                        Swap(element2, A),
                    }),

                new(
                    Domain: Container.Domain,
                    Goal: new(!IsPresent(element1) & !IsPresent(element2)),
                    ExpectedResult: new[]
                    {
                        Remove(element1),
                        Remove(element2),
                        //Swap(element1, A) where A != element2,
                        //Swap(element2, A) where A != element1,
                    }),

                ////new(
                ////    Domain: AirCargo.Domain,
                ////    Goal: new(At(new Constant("cargo2"), new Constant("sfo"))),
                ////    ExpectedResult: new Action[]
                ///     {
                ///         Unload(new Constant("cargo2"), P, new Constant("sfo")),
                ///     }),
            })
            .When(tc => tc.Domain.GetRelevantActions(tc.Goal))
            .ThenReturns()
            .And((tc, r) => r.Should().BeEquivalentTo(tc.ExpectedResult));

        // Model for a variable symbol that is precluded from taking particular values...
        // #1 Is this approach justifiable?
        // #2 Or would it need to be separate? i.e. ConstrainedGoal(Elements, Constraints..). Better than new Term type.
        // #3 Or would it need to be a different Term type (which'd obv cause problems for SCFirstOrderLogic - REALLY want to avoid this approach)
        // On first glance, I think I actually prefer #2..
        // Experiment with me.. Later. Sticking point will obviously be if/how constraints can propogate backwards..
        public class ConstrainedVariableSymbol
        {
            public ConstrainedVariableSymbol(object originalSymbol, IEnumerable<Constant> precludedValues) => (OriginalSymbol, PrecludedValues) = (originalSymbol, precludedValues);

            public object OriginalSymbol { get; }

            public IEnumerable<Constant> PrecludedValues { get; }
        }
    }
}
#endif