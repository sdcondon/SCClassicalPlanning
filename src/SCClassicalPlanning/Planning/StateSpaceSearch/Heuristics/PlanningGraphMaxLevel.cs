﻿using SCClassicalPlanning.Planning.GraphPlan;

namespace SCClassicalPlanning.Planning.StateSpaceSearch.Heuristics
{
    /// <summary>
    /// Heuristic that uses a "max level" planning graph heuristic.
    /// <para/>
    /// To give an estimate, it first constructs a planning graph (yup, this is rather expensive..)
    /// starting from the current state. The cost estimate is the maximum level cost of any of the goal's
    /// elements.
    /// </summary>
    public class PlanningGraphMaxLevel : IHeuristic
    {
        private readonly Domain domain;

        /// <summary>
        /// Initialises a new instance of the <see cref="PlanningGraphMaxLevel"/> class.
        /// </summary>
        /// <param name="domain">The relevant domain.</param>
        public PlanningGraphMaxLevel(Domain domain) => this.domain = domain;

        /// <inheritdoc/>
        public float EstimateCost(State state, Goal goal)
        {
            var planningGraph = new PlanningGraph(new(domain, state, goal));

            return goal.Elements.Max(e =>
            {
                var level = planningGraph.GetLevelCost(e);
                if (level != -1)
                {
                    return level;
                }
                else
                {
                    return float.PositiveInfinity;
                }
            });
        }
    }
}