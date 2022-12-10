// Copyright 2022 Simon Condon
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
using SCClassicalPlanning.Planning.GraphPlan;

namespace SCClassicalPlanning.Planning.Search.Heuristics
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
        private readonly Domain domain;

        /// <summary>
        /// Initialises a new instance of the <see cref="PlanningGraphMaxLevel"/> class.
        /// </summary>
        /// <param name="domain">The relevant domain.</param>
        public PlanningGraphSetLevel(Domain domain) => this.domain = domain;

        /// <inheritdoc/>
        public float EstimateCost(State state, Goal goal)
        {
            var planningGraph = new PlanningGraph(new(domain, state, goal));

            var level = planningGraph.GetLevelCost(goal.Elements);
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
