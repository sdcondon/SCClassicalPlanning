using FluentAssertions;
using FluentAssertions.Equivalency;
using FlUnit;
using SCClassicalPlanning._TestUtilities;
using SCClassicalPlanning.ExampleDomains.AsCode;
using SCClassicalPlanning.ProblemCreation;
using SCFirstOrderLogic;
using SCFirstOrderLogic.SentenceManipulation.VariableManipulation;
using static SCClassicalPlanning.ExampleDomains.AsCode.AirCargoDomain;
using static SCClassicalPlanning.ExampleDomains.AsCode.ContainerDomain;
using static SCFirstOrderLogic.SentenceCreation.OperableSentenceFactory;

namespace SCClassicalPlanning.Planning.Utilities;

public static class ProblemInspectorTests
{
    private static readonly Function element1 = new(nameof(element1));
    private static readonly Function element2 = new(nameof(element2));

    private static readonly Function sfo = new(nameof(sfo));
    private static readonly Function cargo = new(nameof(cargo));

    public static Test GetApplicableActionsBehaviour => TestThat
        .GivenEachOf(() => new GetApplicableActionsTestCase[]
        {
            new(
                State: new HashSetState(new("Exists", element1), new("Exists", element2)),
                ActionSchemas: ContainerDomain.ActionSchemas,
                ExpectedResult: [Add(element1), Add(element2)]),

            new(
                State: new HashSetState(IsPresent(element1), new("Exists", element2)),
                ActionSchemas: ContainerDomain.ActionSchemas,
                ExpectedResult: [Remove(element1), Add(element2), Swap(element1, element2)]),

            new(
                State: new HashSetState(IsPresent(element1) & IsPresent(element2)),
                ActionSchemas: ContainerDomain.ActionSchemas,
                ExpectedResult: [Remove(element1), Remove(element2)]),
        })
        .When(tc => ProblemInspector.GetApplicableActions(tc.State, tc.ActionSchemas))
        .ThenReturns()
        .And((tc, r) => r.Should().BeEquivalentTo(tc.ExpectedResult));

    public static Test GetRelevantGroundActionsBehaviour => TestThat
        .GivenEachOf(() => new GetRelevantGroundActionsTestCase[]
        {
            new(
                Goal: new(IsPresent(element1)),
                ActionSchemas: ContainerDomain.ActionSchemas,
                Constants: [element1, element2],
                ExpectedResult:
                [
                    Add(element1),
                    Swap(element2, element1),
                ]),

            new(
                Goal: new(IsPresent(element1) & IsPresent(element2)),
                ActionSchemas: ContainerDomain.ActionSchemas,
                Constants: [element1, element2],
                ExpectedResult:
                [
                    Add(element1),
                    Add(element2),
                ]),

            new(
                Goal: new(IsPresent(element1) & !IsPresent(element2)),
                ActionSchemas: ContainerDomain.ActionSchemas,
                Constants: [element1, element2],
                ExpectedResult:
                [
                    Add(element1),
                    Remove(element2),
                    Swap(element2, element1),
                    Swap(element2, element1), // TODO-BUG: yeah, hits twice - once for the element1 presence and again for the element2 non-presence. dedupe should probably be expected.
                ]),

            new(
                Goal: new(!IsPresent(element1) & !IsPresent(element2)),
                ActionSchemas: ContainerDomain.ActionSchemas,
                Constants: [element1, element2],
                ExpectedResult:
                [
                    Remove(element1),
                    Remove(element2),
                ]),
        })
        .When(tc => ProblemInspector.GetRelevantGroundActions(tc.Goal, tc.ActionSchemas, tc.Constants))
        .ThenReturns()
        .And((tc, r) => r.Should().BeEquivalentTo(tc.ExpectedResult));

    public static Test GetRelevantLiftedActionsBehaviour => TestThat
        .GivenEachOf(() => new GetRelevantLiftedActionsTestCase[]
        {
            new(
                Goal: new(IsPresent(element1)),
                ActionSchemas: ContainerDomain.ActionSchemas,
                ExpectedResult:
                [
                    Add(element1),
                    Swap(R, element1).WithExpandedPrecondition(!AreEqual(R, element1)),
                ]),

            new(
                Goal: new(IsPresent(element1) & IsPresent(element2)),
                ActionSchemas: ContainerDomain.ActionSchemas,
                ExpectedResult:
                [
                    Add(element1),
                    Add(element2),
                    Swap(R, element1).WithExpandedPrecondition(!AreEqual(R, element1) & !AreEqual(R, element2)),
                    Swap(R, element2).WithExpandedPrecondition(!AreEqual(R, element1) & !AreEqual(R, element2)),
                ]),

            new(
                Goal: new(IsPresent(element1) & !IsPresent(element2)),
                ActionSchemas: ContainerDomain.ActionSchemas,
                ExpectedResult:
                [
                    Add(element1),
                    Remove(element2),
                    Swap(R, element1).WithExpandedPrecondition(!AreEqual(R, element1)),
                    Swap(element2, A).WithExpandedPrecondition(!AreEqual(A, element2)),
                ]),

            new(
                Goal: new(!IsPresent(element1) & !IsPresent(element2)),
                ActionSchemas: ContainerDomain.ActionSchemas,
                ExpectedResult:
                [
                    Remove(element1),
                    Remove(element2),
                    Swap(element1, A).WithExpandedPrecondition(!AreEqual(A, element1) & !AreEqual(A, element2)),
                    Swap(element2, A).WithExpandedPrecondition(!AreEqual(A, element1) & !AreEqual(A, element2)),
                ]),

            new(
                Goal: new(At(cargo, sfo)),
                ActionSchemas: AirCargoDomain.ActionSchemas,
                ExpectedResult:
                [
                    Unload(cargo, Var("plane"), sfo),

                    // obviously unmeetable because non-planes can't become planes, but spotting that is not this method's job:
                    Fly(cargo, Var("from"), sfo).WithExpandedPrecondition(!AreEqual(Var("from"), sfo)),
                ]),
        })
        .When(tc => ProblemInspector.GetRelevantLiftedActions(tc.Goal, tc.ActionSchemas))
        .ThenReturns()
        .And((tc, r) => r.Should().BeEquivalentTo(tc.ExpectedResult, ExpectVariablesToBeStandardised));

    public static Test GetMappingFromSchemaPositiveBehaviour => TestThat
        .GivenEachOf(() => new GetMappingFromSchemaPositiveTestCase[]
        {
            new( // Basic case should work
                Action: ContainerDomain.Add(Var("myObject")),
                ActionSchemas: ContainerDomain.ActionSchemas,
                ExpectedBindings: new()
                {
                    [Var("A")] = Var("myObject"),
                }),

            new( // Repeated vars shouldn't cause an issue
                Action: AirCargoDomain.Fly(Var("plane1"), Var("airport1"), Var("airport1")),
                ActionSchemas: AirCargoDomain.ActionSchemas,
                ExpectedBindings: new()
                {
                    [Var("plane")] = Var("plane1"),
                    [Var("from")] = Var("airport1"),
                    [Var("to")] = Var("airport1"),
                }),

            new( // Extra preconditions should be allowed
                Action: ContainerDomain
                    .Add(Var("myObject"))
                    .WithExpandedPrecondition(!AreEqual(Var("myObject"), Var("otherObject"))),
                ActionSchemas: ContainerDomain.ActionSchemas,
                ExpectedBindings: new()
                {
                    [Var("A")] = Var("myObject"),
                }),

            new( // Extra effect elements should also be allowed
                Action: ContainerDomain
                    .Add(Var("myObject"))
                    .WithExpandedEffect(AreEqual(Var("myObject"), Var("something"))),
                ActionSchemas: ContainerDomain.ActionSchemas,
                ExpectedBindings: new()
                {
                    [Var("A")] = Var("myObject"),
                }),
        })
        .When(tc => ProblemInspector.GetMappingFromSchema(tc.Action, tc.ActionSchemas))
        .ThenReturns((tc, rv) => rv.Should().Be(new VariableSubstitution(tc.ExpectedBindings)));

    public static Test GetMappingFromSchemaNegativeBehaviour => TestThat
        .GivenEachOf(() => new GetMappingFromSchemaNegativeTestCase[]
        {
            new( // No matching identifier should cause failure
                Action: new("Bad Action Identifier", new(), new()),
                ActionSchemas: ContainerDomain.ActionSchemas),

            new( // Missing effect elements should cause failure, perhaps?
                Action: ContainerDomain.Add(Var("myObject")).WithReducedEffect(IsPresent(Var("myObject"))),
                ActionSchemas: ContainerDomain.ActionSchemas),

            new( // Missing prerequisite elements should cause failure, perhaps?
                Action: ContainerDomain.Add(Var("myObject")).WithReducedPrecondition(!IsPresent(Var("myObject"))),
                ActionSchemas: ContainerDomain.ActionSchemas),
        })
        .When(tc => ProblemInspector.GetMappingFromSchema(tc.Action, tc.ActionSchemas))
        .ThenThrows();

    public static Test Temp_POS => TestThat
        .GivenTestContext()
        .When(cxt =>
        {
            var actions = ProblemInspector.GetRelevantLiftedActions(
                BlocksWorldDomain.ExampleProblem.EndGoal,
                BlocksWorldDomain.ExampleProblem.ActionSchemas);

            foreach (var action in actions)
            {
                var mapping = ProblemInspector.GetMappingFromSchema(action, BlocksWorldDomain.ActionSchemas, out var extras);
                cxt.WriteOutputLine(string.Join(", ", mapping.Bindings.Select(kvp => $"{kvp.Key}: {kvp.Value}")) + ". CONSTRAINTS: " + string.Join(", ", extras));
            }
        })
        .ThenReturns();

    public static Test Temp => TestThat
        .GivenTestContext()
        .When(cxt =>
        {
            var domain = PddlParser.ParseDomain(ExampleDomains.AsPDDL.BlocksWorldDomain.DomainPDDL);
            var problem = PddlParser.ParseProblem(ExampleDomains.AsPDDL.BlocksWorldDomain.ExampleProblemPDDL, domain);

            var actions = ProblemInspector.GetRelevantLiftedActions(
                problem.EndGoal,
                domain.ActionSchemas);

            foreach (var action in actions)
            {
                var mapping = ProblemInspector.GetMappingFromSchema(action, BlocksWorldDomain.ActionSchemas, out var extras);
                cxt.WriteOutputLine(string.Join(", ", mapping.Bindings.Select(kvp => $"{kvp.Key}: {kvp.Value}")) + ". CONSTRAINTS: " + string.Join(", ", extras));
            }
        })
        .ThenReturns();

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
                    .Identifier.Should().BeOfType<ProblemInspector.StandardisedVariableSymbol>()
                    .Which.OriginalSymbol.Should().Be(cxt.Expectation.Identifier);
            })
            .WhenTypeIs<VariableReference>()
            .Using<VariableDeclaration>(cxt =>
            {
                cxt.Subject
                    .Identifier.Should().BeOfType<ProblemInspector.StandardisedVariableSymbol>()
                    .Which.OriginalSymbol.Should().Be(cxt.Expectation.Identifier);
            })
            .WhenTypeIs<VariableDeclaration>();
    }

    private record GetApplicableActionsTestCase(IState State, IQueryable<Action> ActionSchemas, Action[] ExpectedResult);
    private record GetRelevantGroundActionsTestCase(Goal Goal, IQueryable<Action> ActionSchemas, IEnumerable<Function> Constants, Action[] ExpectedResult);
    private record GetRelevantLiftedActionsTestCase(Goal Goal, IQueryable<Action> ActionSchemas, Action[] ExpectedResult);
    private record GetMappingFromSchemaPositiveTestCase(Action Action, IQueryable<Action> ActionSchemas, Dictionary<VariableReference, Term> ExpectedBindings);
    private record GetMappingFromSchemaNegativeTestCase(Action Action, IQueryable<Action> ActionSchemas);
}
