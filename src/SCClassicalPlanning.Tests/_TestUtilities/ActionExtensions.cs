using static SCClassicalPlanning.ProblemCreation.OperableProblemFactory;

namespace SCClassicalPlanning._TestUtilities;

internal static class ActionExtensions
{
    /// <summary>
    /// Appends some extra elements to a given action's precondition and returns the result.
    /// </summary>
    /// <returns>The updated action.</returns>
    public static Action WithExpandedPrecondition(this Action action, OperableGoal additionalPrecondition)
    {
        return new Action(action.Identifier, new(action.Precondition.Elements.Union(additionalPrecondition.Elements)), action.Effect);
    }

    /// <summary>
    /// Removes some elements from a given action's precondition and returns the result.
    /// </summary>
    /// <returns>The updated action.</returns>
    public static Action WithReducedPrecondition(this Action action, OperableGoal removedPrecondition)
    {
        return new Action(action.Identifier, new(action.Precondition.Elements.Except(removedPrecondition.Elements)), action.Effect);
    }

    /// <summary>
    /// Appends some extra elements to a given action's effect and returns the result.
    /// </summary>
    /// <returns>The updated action.</returns>
    public static Action WithExpandedEffect(this Action action, OperableEffect additionalEffect)
    {
        return new Action(action.Identifier, action.Precondition, new(action.Effect.Elements.Union(additionalEffect.Elements)));
    }

    /// <summary>
    /// Removes some elements from a given action's effect and returns the result.
    /// </summary>
    /// <returns>The updated action.</returns>
    public static Action WithReducedEffect(this Action action, OperableEffect removedEffect)
    {
        return new Action(action.Identifier, action.Precondition, new(action.Effect.Elements.Except(removedEffect.Elements)));
    }
}
