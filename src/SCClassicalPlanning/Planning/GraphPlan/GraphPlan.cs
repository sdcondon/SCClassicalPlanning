﻿// Copyright 2022 Simon Condon
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
using SCGraphTheory;
using SCGraphTheory.Search.Classic;
using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace SCClassicalPlanning.Planning.GraphPlan
{
    /// <summary>
    /// Planner implementation that uses the GraphPlan algorithm.
    /// <para/>
    /// Extracts solutions via a backward search.
    /// </summary>
    internal class GraphPlan : IPlanner
    {
        /// <summary>
        /// Creates a (concretely-typed) planning task to work on solving a given problem.
        /// </summary>
        /// <param name="problem">The problem to create a plan for.</param>
        /// <returns></returns>
        public static PlanningTask CreatePlanningTask(Problem problem) => new(problem);

        /// <inheritdoc />
        IPlanningTask IPlanner.CreatePlanningTask(Problem problem) => CreatePlanningTask(problem);

        /// <summary>
        /// The implementation of <see cref="IPlanningTask"/> used by <see cref="GraphPlan"/>.
        /// </summary>
        public class PlanningTask : IPlanningTask
        {
            private readonly Problem problem;

            private bool isComplete;
            private Plan? result;

            internal PlanningTask(Problem problem) => this.problem = problem;

            /// <inheritdoc />
            public bool IsComplete => isComplete;

            /// <inheritdoc />
            public bool IsSucceeded => result != null;

            /// <inheritdoc />
            public Plan Result
            {
                get
                {
                    if (!IsComplete)
                    {
                        throw new InvalidOperationException("Task is not yet complete");
                    }
                    else if (result == null)
                    {
                        throw new InvalidOperationException("Plan creation failed");
                    }
                    else
                    {
                        return result;
                    }
                }
            }

            /// <inheritdoc />
            public async Task<Plan> ExecuteAsync(CancellationToken cancellationToken = default)
            {
                HashSet<NoGood> noGoods = new();
                var goalElementsPresentAndNonMutex = false;
                var noGoodsLevelledOff = false;

                // First, create a planning graph.
                var graph = new PlanningGraph(problem);

                // Iterate through the levels of the planning graph, and for each..
                for (int i = 0; ; i++)
                {
                    var graphLevel = graph.GetLevel(i);

                    // ..check if all of the goal's elements occur at this level, with no pair mutually exclusive. If so..
                    // (NB: we don't need to keep checking this once its true for a level - because mutexes decrease monotonically)
                    if (goalElementsPresentAndNonMutex || (goalElementsPresentAndNonMutex = graphLevel.ContainsNonMutex(problem.Goal.Elements)))
                    {
                        // ..try to extract a solution:
                        if (await Task.Run(() => TryExtractSolution(graphLevel, noGoods, out result, cancellationToken), cancellationToken))
                        {
                            // If we managed to extract a solution, we're done. Return it.
                            isComplete = true;
                            return result!;
                        }
                        // todo - establish whether nogoods have levelled off..
                        ////else if ()
                        ////{
                        ////    noGoodsLevelledOff = true;
                        ////}
                    }

                    // If we haven't managed to extract a solution, and both the graph and no-goods have levelled off, we fail.
                    if (graph.IsLevelledOff && noGoodsLevelledOff)
                    {
                        result = null;
                        isComplete = true;
                        throw new InvalidOperationException("Plan creation failed");
                    }
                }
            }

            private bool TryExtractSolution(
                PlanningGraph.Level graphLevel,
                HashSet<NoGood> noGoods,
                [MaybeNullWhen(false)] out Plan plan,
                CancellationToken cancellationToken)
            {
                // NB: AIaMA says that the target is *level is zero* and initial state satisfies the goal.
                // Don't actually need to check for level being zero. If we satisfy the goal at level n > 0, we can just
                // use no-op actions to get to that level then execute the plan.
                // In practice this won't happen because we'd have found the target at an earlier step anyway. 
                // So, what the book is trying (fairly badly..) to say is that when we first find the target, it'll be
                // at level zero.
                var search = new RecursiveDFS<SearchNode, SearchEdge>(
                    source: new SearchNode(graphLevel, problem.Goal),
                    isTarget: n => problem.InitialState.Satisfies(n.Goal));

                search.Complete(cancellationToken);

                // todo: nogoods - ..query search tree for nogoods here?

                if (search.IsSucceeded)
                {
                    // TODO: eliminate magic string
                    plan = new Plan(search.PathToTarget().Reverse().SelectMany(e => e.Actions).Where(a => !a.Identifier.Equals("NOOP")).ToList());
                    return true;
                }
                else
                {
                    plan = null;
                    return false;
                }
            }

            /// <inheritdoc />
            public void Dispose()
            {
                // Nothing to do
                GC.SuppressFinalize(this);
            }
        }

        private record struct NoGood(int Level, Goal Goal);

        // A node in the solution extraction search represents having a particular goal at a particular
        // level of the planning graph. The outbound edges of this node each represent sets of actions
        // applicable to the previous level (no pair of which are mutually exclusive) that collectively
        // satisfy the goal.
        [DebuggerDisplay("{Goal} @ L{graphLevel.Index}")]
        private readonly struct SearchNode : INode<SearchNode, SearchEdge>, IEquatable<SearchNode>
        {
            private readonly PlanningGraph.Level graphLevel;

            public SearchNode(PlanningGraph.Level graphLevel, Goal goal)
            {
                this.graphLevel = graphLevel;
                this.Goal = goal;
            }

            public readonly Goal Goal { get; }

            public IReadOnlyCollection<SearchEdge> Edges => new SearchNodeEdges(graphLevel, Goal);

            public override bool Equals(object? obj) => obj is SearchNode node && Equals(node);

            // NB: this struct is private - so we don't need to look at the planning graph, since it'll always match
            public bool Equals(SearchNode node) => graphLevel.Index == node.graphLevel.Index && Equals(Goal, node.Goal);

            public override int GetHashCode() => HashCode.Combine(graphLevel.Index, Goal);
        }

        private readonly struct SearchNodeEdges : IReadOnlyCollection<SearchEdge>
        {
            private readonly PlanningGraph.Level graphLevel;
            private readonly Goal goal;

            public SearchNodeEdges(PlanningGraph.Level graphLevel, Goal goal)
            {
                this.graphLevel = graphLevel;
                this.goal = goal;
            }

            /// <inheritdoc />
            public int Count => this.Count();

            /// <inheritdoc />
            public IEnumerator<SearchEdge> GetEnumerator()
            {
                /*
                 * "The actions available in a state at level S[i] are to select any conflict-free subset of the actions in A[i−1]
                 * whose effects cover the goals in the state. The resulting state has level S[i−1] and has as its set of goals
                 * the preconditions for the selected set of actions. By “conflict free,” we mean a set of actions such that
                 * no two of them are mutex and no two of their preconditions are mutex."
                 * 
                 * NB: in the above, why the authors chose to use "action" and "state" to refer to the edges and vertices *of the
                 * search graph* is beyond me - there's obviously huge scope for confusion here..
                 *
                 * and, to establish edge ordering:
                 *
                 * "We need some heuristic guidance for choosing among actions during the backward search
                 * One approach that works well in practice is a greedy algorithm based on the level cost of the literals.
                 * For any set of goals, we proceed in the following order:
                 * 1. Pick first the literal with the highest level cost.
                 * 2. To achieve that literal, prefer actions with easier preconditions.That is, choose an action such that the sum (or maximum) of the level costs of its preconditions is smallest."
                 */

                var graphLevel = this.graphLevel; // copy level variable because we can't access struct instance field in lambda

                // Order the goal elements by descending level cost:
                var goalElements = goal.Elements
                    .OrderByDescending(e => graphLevel.Graph.GetLevelCost(e));

                // Find all of the actions that satisfy at least one element of the goal and order by sum of precondition level costs:
                // NB: While we use the term "relevant" here, note that we're not discounting those that clash with the goal - that will 
                // be dealt with by mutex checks.
                var relevantActionNodes = goal.Elements
                    .SelectMany(e => graphLevel.NodesByProposition[e].Causes)
                    .OrderBy(n => n.Preconditions.Sum(p => graphLevel.Graph.GetLevelCost(p.Proposition)))
                    .Distinct(); // todo: can probably do this before order by? ref equality, but we take action to avoid dups.

                // Now (recursively) attempt to cover all elements of the goal, with no mutexes:
                // We go recursive to ensure that we ultimately find all combinations.
                return Recurse(goalElements, relevantActionNodes.ToImmutableHashSet(), ImmutableHashSet<PlanningGraph.ActionNode>.Empty).GetEnumerator();
            }

            /// <inheritdoc />
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            private IEnumerable<SearchEdge> Recurse(
                IEnumerable<Literal> unsatisfiedGoalElements,
                ImmutableHashSet<PlanningGraph.ActionNode> unselectedActionNodes,
                ImmutableHashSet<PlanningGraph.ActionNode> selectedActionNodes)
            {
                bool IsNonMutexWithSelectedActions(PlanningGraph.ActionNode actionNode)
                {
                    foreach (var selectedActionNode in selectedActionNodes)
                    {
                        if (actionNode.Mutexes.Any(m => m.Action.Equals(selectedActionNode.Action)))
                        {
                            return false;
                        }
                    }

                    return true;
                }

                // TODO-PERFORMANCE: a lot of GC pressure here, what with all the immutable hash sets.
                // could eliminate the duplication by creating a tree instead, or just some bit vector struct to indicate selection.
                // meh, lets get it working first, then at least we have a baseline.
                if (!unsatisfiedGoalElements.Any())
                {
                    yield return new SearchEdge(graphLevel, goal, selectedActionNodes.Select(n => n.Action));
                }
                else
                {
                    // Try to cover the first goal element (any others covered by the same action are a bonus)
                    var firstGoalElement = unsatisfiedGoalElements.First();

                    foreach (var actionNode in unselectedActionNodes)
                    {
                        if (actionNode.Action.Effect.Elements.Contains(firstGoalElement) && IsNonMutexWithSelectedActions(actionNode))
                        {
                            foreach (var edge in Recurse(
                                unsatisfiedGoalElements.Except(actionNode.Action.Effect.Elements),
                                unselectedActionNodes.Remove(actionNode),
                                selectedActionNodes.Add(actionNode)))
                            {
                                yield return edge;
                            }
                        }
                    }
                }
            }
        }

        private readonly struct SearchEdge : IEdge<SearchNode, SearchEdge>
        {
            private readonly PlanningGraph.Level graphLevel;
            private readonly Goal goal;

            public SearchEdge(PlanningGraph.Level graphLevel, Goal goal, IEnumerable<Action> actions)
            {
                this.graphLevel = graphLevel;
                this.goal = goal;
                this.Actions = actions;
            }

            public IEnumerable<Action> Actions { get; }

            /// <inheritdoc />
            public SearchNode From => new(graphLevel, goal);

            /// <inheritdoc />
            public SearchNode To => new(graphLevel.PreviousLevel!, new Goal(Actions.SelectMany(a => a.Precondition.Elements)));

            /////// <inheritdoc />
            ////public override string ToString() => new PlanFormatter(problem.Domain).Format(Action);
        }
    }
}
