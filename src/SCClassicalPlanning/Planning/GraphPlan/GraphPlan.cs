using SCGraphTheory;
using SCGraphTheory.Search.Classic;
using System.Collections;
using System.Collections.Immutable;

namespace SCClassicalPlanning.Planning.GraphPlan
{
    /// <summary>
    /// Planner implementation that uses the GraphPlan algorithm.
    /// <para/>
    /// Extracts solutions via a backward search.
    /// </summary>
    public class GraphPlan : IPlanner
    {
        /// <inheritdoc/>
        public async Task<Plan> CreatePlanAsync(Problem problem, CancellationToken cancellationToken = default)
        {
            var graph = new PlanningGraph(problem);
            (int, Goal)? noGood = null, lastNoGood = null;

            for (int i = 0; ; i++)
            {
                var graphLevel = graph.GetLevel(i);

                if (graphLevel.ContainsNonMutex(problem.Goal.Elements))
                {
                    var result = await TryExtractSolutionAsync(problem.InitialState, problem.Goal, graphLevel, cancellationToken);

                    if (result.Success)
                    {
                        return result.Plan!;
                    }
                    else
                    {
                        noGood = result.NoGood;
                    }
                }

                if (graph.LevelledOff && noGood.Equals(lastNoGood))
                {
                    throw new ArgumentException("Problem is unsolvable");
                }

                lastNoGood = noGood;
            }
        }

        private async Task<SolutionExtractionResult> TryExtractSolutionAsync(State initialState, Goal goal, PlanningGraph.Level graphLevel, CancellationToken cancellationToken)
        {
            // NB: the book says that the target is *level is zero* and initial state matches goal.
            // don't actually need to look for level being zero. If we satisfy the goal at level n > 0, we can just
            // use no-op actions to get to that level then execute the plan. in practice i dont think
            // this'll happen because we'll find the solution on an earlier step anyway
            var search = new AStarSearch<StateSpaceNode, StateSpaceEdge>(
                source: new StateSpaceNode(graphLevel, goal),
                isTarget: n => initialState.Satisfies(n.Goal), // todo: nogoods - record level and goals here, or..?
                getEdgeCost: e => 1,
                getEstimatedCostToTarget: EstimateCost);

            await search.CompleteAsync(1, cancellationToken);

            // todo: nogoods - ..or query search tree for nogoods here?
            // otherwise going to have to open things up and not use this a* implementation.

            if (search.IsSucceeded())
            {
                return new SolutionExtractionResult(
                    Success: true,
                    NoGood: null,
                    Plan: new Plan(search.PathToTarget().Reverse().SelectMany(e => e.Actions).ToList()));
            }
            else
            {
                return new SolutionExtractionResult(
                    Success: false,
                    NoGood: (0, Goal.Empty), // todo
                    Plan: null);
            }
        }

        /*
         * "We need some heuristic guidance for choosing among actions during the backward search
         * One approach that works well in practice is a greedy algorithm based on the level cost of the literals.
         * For any set of goals, we proceed in the following order:
         * 1. Pick first the literal with the highest level cost.
         * 2. To achieve that literal, prefer actions with easier preconditions. That is, choose an action such that the sum (or maximum) of the level costs of its preconditions is smallest." 
         * Russell, Stuart; Norvig, Peter. Artificial Intelligence: A Modern Approach, Global Edition (p. 385). Pearson Education Limited. Kindle Edition.
         * 
         * :/ Implies not using A* I think - and also doesn't explain how to compromise if first choice doesn't work.
         * The implication is using some kind of cost-aware algorithm, though, because of the earlier comment
         * about all actions have cost 1. All in all, not very helpful..
         */
        private float EstimateCost(StateSpaceNode node)
        {
            // todo
            throw new NotImplementedException();
        }

        private record struct SolutionExtractionResult(bool Success, Plan? Plan, (int Level, Goal Goal)? NoGood);

        private struct StateSpaceNode : INode<StateSpaceNode, StateSpaceEdge>, IEquatable<StateSpaceNode>
        {
            private readonly PlanningGraph.Level graphLevel;

            public StateSpaceNode(PlanningGraph.Level graphLevel, Goal goal)
            {
                this.graphLevel = graphLevel;
                this.Goal = goal;
            }

            public readonly Goal Goal { get; }

            public IReadOnlyCollection<StateSpaceEdge> Edges => new StateSpaceNodeEdges(graphLevel, Goal);

            public override bool Equals(object? obj) => obj is StateSpaceNode node && Equals(node);

            // NB: this struct is private - so we don't need to look at the planning graph, since it'll always match
            public bool Equals(StateSpaceNode node) => Equals(Goal, node.Goal);

            public override int GetHashCode() => HashCode.Combine(Goal);

            public override string ToString() => Goal.ToString(); // mebbe add info about the level (e.g. the index)
        }

        private struct StateSpaceNodeEdges : IReadOnlyCollection<StateSpaceEdge>
        {
            private readonly PlanningGraph.Level graphLevel;
            private readonly Goal goal;

            public StateSpaceNodeEdges(PlanningGraph.Level graphLevel, Goal goal)
            {
                this.graphLevel = graphLevel;
                this.goal = goal;
            }

            /// <inheritdoc />
            public int Count => this.Count();

            /// <inheritdoc />
            public IEnumerator<StateSpaceEdge> GetEnumerator()
            {
                var possibleActions = graphLevel.Nodes.SelectMany(n => n.Causes).Select(n => n.Action);
                return Recurse(goal, possibleActions.ToImmutableHashSet(), ImmutableHashSet<Action>.Empty).GetEnumerator();
            }

            /// <inheritdoc />
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            /*
             * "The actions available in a state at level S[i] are to select any conflict-free subset of the actions in A[i−1]
             * whose effects cover the goals in the state. The resulting state has level S[i−1] and has as its set of goals
             * the preconditions for the selected set of actions. By “conflict free,” we mean a set of actions such that
             * no two of them are mutex and no two of their preconditions are mutex."
             */
            // (recursively, yeurch) iterates the available sets of actions in A[i-1] that cover the goal.
            private IEnumerable<StateSpaceEdge> Recurse(Goal goal, ImmutableHashSet<Action> unselectedActions, ImmutableHashSet<Action> selectedActions)
            {
                // NB-PERFORMANCE: a lot of GC pressure here, what with all the immutable hash sets.
                // could eliminate the duplication by creating a tree instead. meh, not worth it right now.
                if (goal.Equals(Goal.Empty))
                {
                    yield return new StateSpaceEdge(graphLevel, selectedActions);
                }
                else
                {
                    foreach (var action in unselectedActions)
                    {
                        var unsatisfiedGoalElements = goal.Elements.Except(action.Effect.Elements);

                        if (unsatisfiedGoalElements.Count < goal.Elements.Count)
                        {
                            throw new NotImplementedException();
                            
                            var isNonMutexWithSelectedActions = true; // todo

                            if (isNonMutexWithSelectedActions)
                            {
                                foreach (var edge in Recurse(
                                    new Goal(unsatisfiedGoalElements),
                                    unselectedActions.Remove(action),
                                    selectedActions.Add(action)))
                                {
                                    yield return edge;
                                }
                            }
                        }
                    }
                }
            }
        }

        private struct StateSpaceEdge : IEdge<StateSpaceNode, StateSpaceEdge>
        {
            private readonly PlanningGraph.Level graphLevel;

            public StateSpaceEdge(PlanningGraph.Level graphLevel, IEnumerable<Action> actions)
            {
                this.graphLevel = graphLevel;
                this.Actions = actions;
            }

            public IEnumerable<Action> Actions { get; }

            /// <inheritdoc />
            // could be new StateSpaceNode(planningGraph, fromGoal); - but we'd need fromGoal, which is a waste. Private struct and unused, so just ignore it
            public StateSpaceNode From => throw new NotImplementedException(); 

            /// <inheritdoc />
            public StateSpaceNode To => new(graphLevel.PreviousLevel!, new Goal(Actions.SelectMany(a => a.Precondition.Elements)));

            /// <inheritdoc />
            //public override string ToString() => new PlanFormatter(problem.Domain).Format(Action);
        }
    }
}
