﻿// Copyright 2022-2023 Simon Condon
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
using SCFirstOrderLogic;

namespace SCClassicalPlanning.Planning.GraphPlan
{
    /// <summary>
    /// An implementation of <see cref="IPlanningTask"/> that uses the GraphPlan algorithm.
    /// </summary>
    // NB: Lots of comments here, perhaps too many - I'm leaving them in due to the
    // primary purpose of this lib - learning and experimentation.
    internal class GraphPlanPlanningTask : TemplatePlanningTask
    {
        private readonly Problem problem;

        // Is a dictionary because its described as a hashtable in the listing.
        // Seems like a List'd do the job just fine (and obv be faster; okay, index 0 unneeded, but..)?
        private readonly Dictionary<int, HashSet<Goal>> noGoods = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphPlanPlanningTask"/> class.
        /// </summary>
        /// <param name="problem">The problem to solve.</param>
        public GraphPlanPlanningTask(Problem problem)
        {
            this.problem = problem;
            PlanningGraph = new(problem);
        }

        /// <summary>
        /// Gets the planning graph used by this planning task.
        /// </summary>
        public PlanningGraph PlanningGraph { get; }

        // TODO: should probably expose NoGoods - just need to make it read-only.

        /// <inheritdoc/>
        public override async Task<Plan> ExecuteAsync(CancellationToken cancellationToken = default)
        {
            // Starting from the first level of the graph, step forward through the
            // levels until all goal elements are present and pairwise non-mutex
            // (initialising the no-goods table HashSets as we go):
            var currentGraphLevel = PlanningGraph.GetLevel(0);
            while (!currentGraphLevel.ContainsNonMutex(problem.Goal.Elements))
            {
                // If the graph levels off before all elements are present and non-mutex,
                // then the problem is unsolvable:
                if (currentGraphLevel.IsLevelledOff)
                {
                    throw new InvalidOperationException("Problem is unsolvable");
                }

                currentGraphLevel = currentGraphLevel.NextLevel;
                noGoods[currentGraphLevel.Index] = new();
            }

            // Attempt to extract a plan until we succeed - stepping forward through
            // the levels on each attempt. Fail if we get to a point where both the
            // graph and the fixed-point no-goods have levelled off.
            Plan? plan;
            var previousFixedPointNoGoodCount = currentGraphLevel.IsLevelledOff ? noGoods[currentGraphLevel.Graph.LevelsOffAtLevel + 1].Count : 0;
            do
            {
                plan = Extract(problem.Goal, currentGraphLevel);

                if (plan == null)
                {
                    if (currentGraphLevel.IsLevelledOff)
                    {
                        var currentFixedPointNoGoodCount = noGoods[currentGraphLevel.Graph.LevelsOffAtLevel + 1].Count;

                        if (previousFixedPointNoGoodCount == currentFixedPointNoGoodCount)
                        {
                            throw new InvalidOperationException("Problem is unsolvable");
                        }
                        else
                        {
                            previousFixedPointNoGoodCount = currentFixedPointNoGoodCount;
                        }
                    }

                    currentGraphLevel = currentGraphLevel.NextLevel;
                    noGoods[currentGraphLevel.Index] = new();
                    await Task.Yield(); // just until such time as e.g. States can include async stuff..
                }
            }
            while (plan == null);

            return plan;
        }

        public override void Dispose()
        {
            // Nothing to do..
            GC.SuppressFinalize(this);
        }

        private Plan? Extract(Goal goal, PlanningGraphLevel level)
        {
            // If we've reached level 0 (i.e. the initial state of the problem), we're done.
            // NB: there's no need to consider the goal here. The initial state MUST satisfy
            // whatever goal we are left with at this point - because the initial state determines
            // what actions were available for selection in TryGPSearch at level 1. (and we
            // could have only hit this method for the first time at level 0 if the initial state
            // satisfies the goal).
            if (level.Index == 0)
            {
                return Plan.Empty;
            }

            // Now check if we've hit a known no-good, and return failure if so:
            if (noGoods[level.Index].Contains(goal))
            {
                return null;
            }

            // Otherwise, try to find a plan using the (recursive) GPSearch method:
            Plan? plan = GPSearch(
                remainingGoalElements: SortGoalElements(goal, level),
                chosenActionNodes: Enumerable.Empty<PlanningGraphActionNode>(),
                level: level);

            if (plan == null)
            {
                // Add to known no-goods if we fail, so that we can save ourselves from wasted effort
                // later on in the search.
                noGoods[level.Index].Add(goal);
            }

            return plan;
        }

        private Plan? GPSearch(
            IEnumerable<Literal> remainingGoalElements,
            IEnumerable<PlanningGraphActionNode> chosenActionNodes,
            PlanningGraphLevel level)
        {
            if (!remainingGoalElements.Any())
            {
                // There are no remaining uncovered goal elements at this level. That is, we've
                // found a set of (non-mutually-exclusive) actions, the collective effects of which
                // cover our goal.
                // Now we call Extract with the previous level and all of the preconditions of our chosen actions.
                var plan = Extract(
                    goal: new Goal(chosenActionNodes.SelectMany(n => n.Action.Precondition.Elements)),
                    level: level.PreviousLevel);

                // If Extract found a plan for the prior level and goal, append our chosen actions to it
                // (omitting any persistence actions) and return the result. If no plan could be found
                // for the prior level, return failure.
                if (plan != null)
                {
                    return new Plan(plan.Steps
                        .Concat(chosenActionNodes.Select(n => n.Action))
                        .Where(a => !a.Identifier.Equals(PlanningGraph.PersistenceActionIdentifier)));
                }
                else
                {
                    return null;
                }
            }
            else
            {
                // There are still goal elements to be covered.
                // Try to cover the first goal element (any others covered by the same action are a bonus),
                // then recurse for the other actions and remaining uncovered goal elements.
                var firstRemainingGoalElement = remainingGoalElements.First();

                foreach (var actionNode in GetRelevantActions(remainingGoalElements, level))
                {
                    if (actionNode.Action.Effect.Elements.Contains(firstRemainingGoalElement) && !actionNode.IsMutexWithAny(chosenActionNodes))
                    {
                        // TODO-PERFORMANCE: we'll end up with lots of wrapped IEnumerables here.
                        // Benchmark and optimise - but get it working first.
                        var plan = GPSearch(
                            remainingGoalElements: remainingGoalElements.Except(actionNode.Action.Effect.Elements),
                            chosenActionNodes: chosenActionNodes.Append(actionNode),
                            level: level);

                        if (plan != null)
                        {
                            return plan;
                        }
                    }
                }

                return null;
            }
        }

        /*
         * "We need some heuristic guidance for choosing among actions during the backward search
         * One approach that works well in practice is a greedy algorithm based on the level cost of the literals.
         * For any set of goals, we proceed in the following order:
         * 1. Pick first the literal with the highest level cost.
         * 2. To achieve that literal, prefer actions with easier preconditions.That is, choose an action such that the sum (or maximum) of the level costs of its preconditions is smallest."
         * 
         * Artificial Intelligence: A Modern Approach (Russel & Norvig)
         */
        // TODO: At some point, remove these hard-coded heuristics in favour of something injected by the consumer.
        IEnumerable<Literal> SortGoalElements(Goal goal, PlanningGraphLevel graphLevel)
        {
            return goal.Elements
                .OrderByDescending(e => graphLevel.Graph.GetLevelCost(e));
        }

        IEnumerable<PlanningGraphActionNode> GetRelevantActions(IEnumerable<Literal> goalElements, PlanningGraphLevel graphLevel)
        {
            return goalElements
                .SelectMany(e => graphLevel.NodesByProposition[e].Causes)
                .OrderBy(n => n.Preconditions.Sum(p => graphLevel.Graph.GetLevelCost(p.Proposition)))
                .Distinct(); // todo: can probably do this before order by? ref equality, but we take action to avoid dups.
        }
    }
}
