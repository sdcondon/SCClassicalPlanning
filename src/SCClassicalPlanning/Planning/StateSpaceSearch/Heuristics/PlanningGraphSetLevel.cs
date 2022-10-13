using SCClassicalPlanning.Planning.GraphPlan;

namespace SCClassicalPlanning.Planning.StateSpaceSearch.Heuristics
{
    /// <summary>
    /// Heuristic that uses a "max level" planning graph heuristic.
    /// <para/>
    /// To give an estimate, it first constructs a planning graph (yup, this is rather expensive..)
    /// starting from the current state. The cost estimate is the maximum level cost of any of the goal's
    /// elements.
    /// </summary>
    public class PlanningGraphSetLevel : IHeuristic
    {
        private readonly Problem problem;

        /// <summary>
        /// Initialises a new instance of the <see cref="PlanningGraphMaxLevel"/> class.
        /// </summary>
        /// <param name="problem">The problem being solved.</param>
        public PlanningGraphSetLevel(Problem problem) => this.problem = problem;

        /// <inheritdoc/>
        public float EstimateCost(State state, Goal goal)
        {
            var planningGraph = new PlanningGraph(problem, state);

            var level = planningGraph.GetSetLevel(goal.Elements);
            if (level != -1)
            {
                return level;
            }
            else
            {
                return float.PositiveInfinity;
            }
        }
    }
}
