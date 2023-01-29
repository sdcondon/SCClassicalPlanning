using SCFirstOrderLogic;

namespace SCClassicalPlanning.Planning.GraphPlan
{
    /// <summary>
    /// <para>
    /// GraphPlan, as close a possible to the way it is described in chapter 6
    /// of "Automated Planning: Theory and Practice".
    /// </para>
    /// </summary>
    // NB: Lots of comments here, perhaps too many - I'm leaving them in due to the
    // primary purpose of this lib - learning and experimentation.
    public class GraphPlanPlanningTask_FromAPTaP : TemplatePlanningTask
    {
        private readonly Problem problem;

        // Is a dictionary because its described as a hashtable in the listing.
        // Seems like a List'd do the job just fine (and obv be faster; okay, index 0 unneeded, but..)?
        private readonly Dictionary<int, HashSet<Goal>> noGoods = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphPlanPlanningTask"/> class.
        /// </summary>
        /// <param name="problem">The problem to solve.</param>
        public GraphPlanPlanningTask_FromAPTaP(Problem problem)
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
        protected override Task<Plan> ExecuteAsyncCore(CancellationToken cancellationToken = default)
        {
            // Starting from the first level of the graph, step forward through the
            // levels until all goal elements are present and pairwise non-mutex, or the graph
            // has levelled off:
            var currentGraphLevel = PlanningGraph.GetLevel(0);
            while (!currentGraphLevel.ContainsNonMutex(problem.Goal.Elements) && !currentGraphLevel.IsLevelledOff)
            {
                currentGraphLevel = currentGraphLevel.NextLevel;
                noGoods[currentGraphLevel.Index] = new();
            }

            // If the graph levelled off before all elements were non-mutex,
            // then we can't solve the problem:
            if (!currentGraphLevel.ContainsNonMutex(problem.Goal.Elements))
            {
                throw new InvalidOperationException("Problem is unsolvable");
            }

            // Try to extract a plan ending at the current level.
            var plan = Extract(problem.Goal, currentGraphLevel);
            var previousFixedPointNoGoodCount = currentGraphLevel.IsLevelledOff ? noGoods[currentGraphLevel.Graph.LevelsOffAtLevel + 1].Count : 0;

            // While we couldn't extract a plan, step forward through the
            // levels and retry. Fail if we get to a point where both the
            // graph and the nogoods have levelled off.
            // (Yes, in C# this could probably be made more succinct with a do-while, but
            // we're sticking to the book listing as closely as possible here).
            while (plan == null)
            {
                currentGraphLevel = currentGraphLevel.NextLevel;
                noGoods[currentGraphLevel.Index] = new();

                plan = Extract(problem.Goal, currentGraphLevel);
                if (plan == null && currentGraphLevel.IsLevelledOff)
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
            }

            return Task.FromResult(plan);
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

            // Otherwise, try to find a plan:
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
                // Now we call TryExtract with the previous level and all of the preconditions of our chosen actions.
                // If that manages to return a plan, append our chosen actions to it and return.
                var plan = Extract(
                    goal: new Goal(chosenActionNodes.SelectMany(n => n.Action.Precondition.Elements)),
                    level: level.PreviousLevel);

                // If a plan was successfully for the prior level, append the chosen actions at this level to the plan returned.
                // If no plan could be found for the prior level, return failure.
                if (plan == null)
                {
                    return null;
                }
                else
                {
                    return new Plan(plan.Steps
                        .Concat(chosenActionNodes.Select(n => n.Action))
                        .Where(a => !a.Identifier.Equals(PlanningGraph.PersistenceActionIdentifier)));
                }
            }
            else
            {
                // There are still goal elements to be covered.
                // Try to cover the first goal element (any others covered by the same action are a bonus),
                // then recurse for the other actions and remaining uncovered goal elements.
                var firstRemainingGoalElement = remainingGoalElements.First();

                foreach (var actionNode in GetRelevantActions(firstRemainingGoalElement, level))
                {
                    if (!actionNode.IsMutexWithAny(chosenActionNodes))
                    {
                        // TODO-PERFORMANCE: lots of wrapped enumerables here. benchmark and optimise (but get it working, first..).
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

                return null; // yuck..
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
        // TODO: At some point, remove this hard-coding.
        IEnumerable<Literal> SortGoalElements(Goal goal, PlanningGraphLevel graphLevel)
        {
            return goal.Elements
                .OrderByDescending(e => graphLevel.Graph.GetLevelCost(e));
        }

        IEnumerable<PlanningGraphActionNode> GetRelevantActions(Literal goalElement, PlanningGraphLevel graphLevel)
        {
            return graphLevel.NodesByProposition[goalElement].Causes
                .OrderBy(n => n.Preconditions.Sum(p => graphLevel.Graph.GetLevelCost(p.Proposition)));
        }
    }
}
