﻿using FluentAssertions;
using FlUnit;

namespace SCClassicalPlanning.ProblemCreation;

public static class ProblemParserTests
{
    public static Test PositiveTestCases => TestThat
        .GivenEachOf(() => new PositiveTestCase[]
        {
            new(
                ExampleDomains.AsPDDL.BlocksWorldDomain.ExampleProblemPDDL, 
                ExampleDomains.AsPDDL.BlocksWorldDomain.DomainPDDL,
                ExampleDomains.AsCode.BlocksWorldDomain.ExampleProblem),

            new(
                ExampleDomains.AsPDDL.AirCargoDomain.ExampleProblemPDDL,
                ExampleDomains.AsPDDL.AirCargoDomain.DomainPDDL,
                ExampleDomains.AsCode.AirCargoDomain.ExampleProblem),
        })
        .When(tc => PddlParser.ParseProblem(tc.problemPddl, tc.domainPddl))
        .ThenReturns()
        //.And((tc, rv) => rv.Domain.Predicates.Should().BeEquivalentTo(tc.expected.Domain.Predicates))
        //.And((tc, rv) => rv.Domain.Constants.Should().BeEquivalentTo(tc.expected.Domain.Constants))
        .And((tc, rv) => rv.ActionSchemas.Should().BeEquivalentTo(tc.expected.ActionSchemas))
        .And((tc, rv) => rv.InitialState.Should().Be(tc.expected.InitialState))
        .And((tc, rv) => rv.EndGoal.Should().Be(tc.expected.EndGoal));

    private record PositiveTestCase(string problemPddl, string domainPddl, Problem expected);
}
