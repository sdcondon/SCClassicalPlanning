using FluentAssertions;
using FlUnit;

namespace SCClassicalPlanning.ProblemCreation
{
    public static class ProblemParserTests
    {
        public static Test PositiveTestCases => TestThat
            .GivenEachOf(() => new PositiveTestCase[]
            {
                new(
                    ExampleDomains.AsPDDL.BlocksWorld.ExampleProblemPDDL, 
                    ExampleDomains.AsPDDL.BlocksWorld.DomainPDDL,
                    ExampleDomains.AsCode.BlocksWorld.ExampleProblem),
            })
            .When(tc => PddlParser.ParseProblem(tc.problemPddl, tc.domainPddl))
            .ThenReturns()
            .And((tc, rv) => rv.Domain.Predicates.Should().BeEquivalentTo(tc.expected.Domain.Predicates))
            .And((tc, rv) => rv.Domain.Constants.Should().BeEquivalentTo(tc.expected.Domain.Constants))
            .And((tc, rv) => rv.Domain.Actions.Should().BeEquivalentTo(tc.expected.Domain.Actions))
            .And((tc, rv) => rv.InitialState.Should().Be(tc.expected.InitialState))
            .And((tc, rv) => rv.Goal.Should().Be(tc.expected.Goal));

        private record PositiveTestCase(string problemPddl, string domainPddl, Problem expected);
    }
}
