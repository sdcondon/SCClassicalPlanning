namespace SCClassicalPlanning.Planning.StateSpaceSearch
{
    public static class StateSpaceSearchHeuristics
    {
        ////public class IgnorePreconditions
        ////{
        ////    private readonly Problem problem;

        ////    public IgnorePreconditions(Problem problem) => this.problem = problem;

        ////    public float EstimateCountOfActionsToGoal(State state, Goal goal)
        ////    {
        ////        throw new NotImplementedException();
        ////    }
        ////}

        ////public class IgnoreDeleteLists
        ////{
        ////    private readonly Problem problem;

        ////    public IgnoreDeleteLists(Problem problem) => this.problem = problem;

        ////    public float EstimateCountOfActionsToGoal(State state, Goal goal)
        ////    {
        ////        throw new NotImplementedException();
        ////    }
        ////}

        /// <summary>
        /// Very simplistic heuristic that just adds the number of positive elements of the goal that do not occur in the state
        /// to the number of negative elements that do (that is, it assumes that we need to carry out one action per element of
        /// the goal that isn't currently satisfied). 
        /// </summary>
        public static class ElementDifferenceCount
        {
            public static float EstimateCountOfActionsToGoal(State state, Goal goal)
            {
                return goal.Elements.Where(e => e.IsPositive).Select(e => e.Predicate).Except(state.Elements).Count()
                    + goal.Elements.Where(e => e.IsNegated).Select(e => e.Predicate).Intersect(state.Elements).Count();
            }
        }
    }
}
