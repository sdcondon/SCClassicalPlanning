using SCClassicalPlanning.ProblemCreation;
using System.Reflection;

namespace SCClassicalPlanning.ExampleDomains.AsPDDL;

/// <summary>
/// The "Blocks World" example from §10.1.3 of "Artificial Intelligence: A Modern Approach".
/// </summary>
public static class BlocksWorldDomain
{
    private static readonly PddlDomain domain = PddlParser.ParseDomain(DomainPDDL);

    public static IQueryable<Action> ActionSchemas => domain.ActionSchemas;

    /// <summary>
    /// Gets a PDDL representation of the "Blocks World" domain.
    /// </summary>
    public static string DomainPDDL => ReadEmbeddedResource("BlocksWorld.pddl");

    /// <summary>
    /// Gets an instance of the customary example problem in this domain.
    /// Consists of three blocks.
    /// In the initial state, blocks A and B are on the table and block C is on top of block A.
    /// The goal is to get block B on top of block C, and block A on top of block B.
    /// </summary>
    public static Problem ExampleProblem { get; } = PddlParser.ParseProblem(ExampleProblemPDDL, domain);

    /// <summary>
    /// Gets a PDDL representation of the customary example problem in this domain.
    /// Consists of three blocks.
    /// In the initial state, blocks A and B are on the table and block C is on top of block A.
    /// The goal is to get block B on top of block C, and block A on top of block B.
    /// </summary>
    public static string ExampleProblemPDDL => ReadEmbeddedResource("BlocksWorld.Sussman.pddl");

    private static string ReadEmbeddedResource(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var fullResourceName = $"SCClassicalPlanning.ExampleDomains.AsPDDL.{resourceName}";

        using Stream stream = assembly.GetManifestResourceStream(fullResourceName)!;
        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }
}