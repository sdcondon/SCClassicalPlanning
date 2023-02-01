using static SCClassicalPlanning.ProblemCreation.OperableProblemFactory;

namespace SCClassicalPlanning._TestUtilities
{
    internal static class ActionExtensions
    {
        /// <summary>
        /// Appends some extra elements to a given action's precondition and returns the result.
        /// </summary>
        /// <returns>the updated action.</returns>
        public static Action WithAdditionalConstraints(this Action action, OperableGoal additionalConstraints)
        {
            return new Action(action.Identifier, new(action.Precondition.Elements.Union(additionalConstraints.Elements)), action.Effect);
        }
    }
}
