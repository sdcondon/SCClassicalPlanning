namespace SCClassicalPlanning.Planning.StateSpaceSearch.Heuristics
{
    /// <summary>
    /// Very simplistic heuristic that just adds the number of positive elements of the goal that do not occur in the state
    /// to the number of negative elements that do (that is, it assumes that we need to carry out one action per element of
    /// the goal that isn't currently satisfied). 
    /// </summary>
    public static class ElementDifferenceCount
    {
        public static float CountDifferences(State state, Goal goal)
        {
            return goal.PositivePredicates.Except(state.Elements).Count()
                + goal.NegativePredicates.Intersect(state.Elements).Count();
        }
    }
}
