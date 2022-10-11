namespace SCClassicalPlanning.Planning.StateSpaceSearch.Heuristics
{
    /// <summary>
    /// Very simplistic heuristic that just counts the number of unsatisfied goals. That is, it adds the number of positive elements
    /// of the goal that do not occur in the state to the number of negative elements that do (that is, it assumes that we need to
    /// carry out one action per element of the goal that isn't currently satisfied).
    /// <para/>
    /// This is generally a pretty terrible heuristic - as are all heuristics that don't take into account the details of the
    /// problem being solved. Consider using something else. DEFINITELY use something else for backward state space searches.
    /// One of the things that this heuristic simply can't do is tell when preconditions are unsatisfiable - which is very bad 
    /// news for a backward state space search, because you rely on being able to do this to prune branches.
    /// </summary>
    public static class UnsatisfiedGoalCount
    {
        public static float EstimateCost(State state, Goal goal)
        {
            return goal.PositivePredicates.Except(state.Elements).Count()
                + goal.NegativePredicates.Intersect(state.Elements).Count();
        }
    }
}
