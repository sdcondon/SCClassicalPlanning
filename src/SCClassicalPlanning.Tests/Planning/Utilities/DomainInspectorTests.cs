using FluentAssertions;
using FluentAssertions.Equivalency;
using FlUnit;
using SCClassicalPlanning._TestUtilities;
using SCClassicalPlanning.ExampleDomains.AsCode;
using SCFirstOrderLogic;
using static SCClassicalPlanning.ExampleDomains.AsCode.Container;
using static SCClassicalPlanning.ExampleDomains.AsCode.AirCargo;
using static SCFirstOrderLogic.SentenceCreation.OperableSentenceFactory;
using SCFirstOrderLogic.SentenceManipulation;

namespace SCClassicalPlanning.Planning.Utilities
{
    public static class DomainInspectorTests
    {
        private static readonly Constant element1 = new(nameof(element1));
        private static readonly Constant element2 = new(nameof(element2));

        private static readonly Constant sfo = new(nameof(sfo));
        private static readonly Constant cargo = new(nameof(cargo));

        public static Test GetRelevantActionsBehaviour => TestThat
            .GivenEachOf(() => new GetRelevantActionsTestCase[]
            {
                new(
                    Domain: Container.Domain,
                    Goal: new(IsPresent(element1)),
                    ExpectedResult: new[]
                    {
                        Add(element1),
                        Swap(R, element1).WithAdditionalPreconditions(!AreEqual(R, element1)),
                    }),

                new(
                    Domain: Container.Domain,
                    Goal: new(IsPresent(element1) & IsPresent(element2)),
                    ExpectedResult: new[]
                    {
                        Add(element1),
                        Add(element2),
                        Swap(R, element1).WithAdditionalPreconditions(!AreEqual(R, element1) & !AreEqual(R, element2)),
                        Swap(R, element2).WithAdditionalPreconditions(!AreEqual(R, element1) & !AreEqual(R, element2)),
                    }),

                new(
                    Domain: Container.Domain,
                    Goal: new(IsPresent(element1) & !IsPresent(element2)),
                    ExpectedResult: new[]
                    {
                        Add(element1),
                        Remove(element2),
                        Swap(R, element1).WithAdditionalPreconditions(!AreEqual(R, element1)),
                        Swap(element2, A).WithAdditionalPreconditions(!AreEqual(A, element2)),
                    }),

                new(
                    Domain: Container.Domain,
                    Goal: new(!IsPresent(element1) & !IsPresent(element2)),
                    ExpectedResult: new[]
                    {
                        Remove(element1),
                        Remove(element2),
                        Swap(element1, A).WithAdditionalPreconditions(!AreEqual(A, element1) & !AreEqual(A, element2)),
                        Swap(element2, A).WithAdditionalPreconditions(!AreEqual(A, element1) & !AreEqual(A, element2)),
                    }),

                new(
                    Domain: AirCargo.Domain,
                    Goal: new(At(cargo, sfo)),
                    ExpectedResult: new Action[]
                    {
                        Unload(cargo, Var("plane"), sfo),

                        // obviously unsatisfiable because non-planes can't become planes, but spotting that is not this method's job:
                        Fly(cargo, Var("from"), sfo).WithAdditionalPreconditions(!AreEqual(Var("from"), sfo)),
                    }),
            })
            .When(tc => DomainInspector.GetRelevantActions(tc.Domain, tc.Goal))
            .ThenReturns()
            .And((tc, r) => r.Should().BeEquivalentTo(tc.ExpectedResult, ExpectVariablesToBeStandardised));

        public static Test GetMappingFromSchemaBehaviour => TestThat
            .GivenEachOf(() => new GetMappingFromSchemaTestCase[]
            {
                new(
                    Domain: Container.Domain,
                    Action: Container.Add(new VariableReference("myObject")),
                    Expected: new VariableSubstitution(new Dictionary<VariableReference, Term>()
                    {
                        [Var("A")] = Var("myObject"),
                    })),

                new( // Repeated vars shouldn't cause an issue
                    Domain: AirCargo.Domain,
                    Action: AirCargo.Fly(Var("plane1"), Var("airport1"), Var("airport1")),
                    Expected: new VariableSubstitution(new Dictionary<VariableReference, Term>()
                    {
                        [Var("plane")] = Var("plane1"),
                        [Var("from")] = Var("airport1"),
                        [Var("to")] = Var("airport1"),
                    })),
            })
            .When(tc => DomainInspector.GetMappingFromSchema(tc.Domain, tc.Action))
            .ThenReturns((tc, rv) => rv.Should().Be(tc.Expected));
    
        private static EquivalencyAssertionOptions<Action> ExpectVariablesToBeStandardised(this EquivalencyAssertionOptions<Action> opts)
        {
            return opts
                .RespectingRuntimeTypes()
                .ComparingByMembers<Action>()
                .ComparingByMembers<Goal>()
                .ComparingByMembers<Effect>()
                .ComparingByMembers<Literal>()
                .ComparingByMembers<Predicate>()
                .ComparingByMembers<Term>()
                .Using<VariableReference>(cxt =>
                {
                    cxt.Subject
                        .Symbol.Should().BeOfType<DomainInspector.StandardisedVariableSymbol>()
                        .Which.OriginalSymbol.Should().Be(cxt.Expectation.Symbol);
                })
                .WhenTypeIs<VariableReference>()
                .Using<VariableDeclaration>(cxt =>
                {
                    cxt.Subject
                        .Symbol.Should().BeOfType<DomainInspector.StandardisedVariableSymbol>()
                        .Which.OriginalSymbol.Should().Be(cxt.Expectation.Symbol);
                })
                .WhenTypeIs<VariableDeclaration>();
        }

        private record GetRelevantActionsTestCase(Domain Domain, Goal Goal, Action[] ExpectedResult);

        private record GetMappingFromSchemaTestCase(Domain Domain, Action Action, VariableSubstitution Expected);
    }
}
