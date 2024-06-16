using SCClassicalPlanning.ProblemCreation;
using System.Reflection;

namespace SCClassicalPlanning.ExampleDomains.AsPDDL;

/// <summary>
/// The "Air Cargo" example from §10.1.1 of "Artificial Intelligence: A Modern Approach".
/// </summary>
public static class AirCargoDomain
{
    private static readonly PddlDomain domain = PddlParser.ParseDomain(DomainPDDL);

    public static IQueryable<Action> ActionSchemas => domain.ActionSchemas;

    /// <summary>
    /// Gets a PDDL representation of the "Air Cargo" domain.
    /// </summary>
    public static string DomainPDDL => ReadEmbeddedResource("AirCargo.pddl");

    /// <summary>
    /// Gets an instance of the customary example problem in this domain.
    /// Consists of two airports, two planes and two pieces of cargo.
    /// In the initial state, plane1 and cargo1 are at airport1; plane2 and cargo2 are at airport2.
    /// The goal is to get cargo2 to airport1 and cargo1 to airport2.
    /// </summary>
    public static Problem ExampleProblem { get; } = PddlParser.ParseProblem(ExampleProblemPDDL, domain);

    /// <summary>
    /// Gets a PDDL representation of the customary example problem in this domain.
    /// Consists of two airports, two planes and two pieces of cargo.
    /// In the initial state, plane1 and cargo1 are at airport1; plane2 and cargo2 are at airport2.
    /// The goal is to get cargo2 to airport1 and cargo1 to airport2.
    /// </summary>
    public static string ExampleProblemPDDL => ReadEmbeddedResource("AirCargo.TwoPlanes.pddl");

    private static string ReadEmbeddedResource(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var fullResourceName = $"SCClassicalPlanning.ExampleDomains.AsPDDL.{resourceName}";

        using Stream stream = assembly.GetManifestResourceStream(fullResourceName)!;
        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }
}