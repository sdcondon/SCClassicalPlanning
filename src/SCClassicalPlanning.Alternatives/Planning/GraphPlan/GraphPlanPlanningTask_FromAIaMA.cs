﻿// Copyright 2022-2024 Simon Condon
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
using System.Collections;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace SCClassicalPlanning.Planning.GraphPlan;

/// <summary>
/// An implementation of <see cref="IPlanningTask"/> that uses the GraphPlan algorithm.
/// Influenced mostly by AIaMA - but doesn't work 'cos nogoods aren't implemented properly.
/// </summary>
internal class GraphPlanPlanningTask_FromAIaMA : TemplatePlanningTask
{
    private readonly Problem problem;

    /// <summary>
    /// Initializes a new instance of the <see cref="GraphPlanPlanningTask"/> class.
    /// </summary>
    /// <param name="problem">The problem to solve.</param>
    public GraphPlanPlanningTask_FromAIaMA(Problem problem)
    {
        this.problem = problem;
        PlanningGraph = new(problem);
    }

    /// <summary>
    /// Gets the planning graph used by this planning task.
    /// </summary>
    public PlanningGraph PlanningGraph { get; }

    /// <inheritdoc />
    protected override async Task<Plan> ExecuteAsyncCore(CancellationToken cancellationToken = default)
    {
        HashSet<SearchState> noGoods = new();
        var goalElementsPresentAndNonMutex = false;
        var noGoodsLevelledOff = false;

        // Iterate through the levels of the planning graph, and for each..
        for (int i = 0; ; i++)
        {
            var graphLevel = PlanningGraph.GetLevel(i);

            // ..check if all of the goal's elements occur at this level, with no pair mutually exclusive. If so..
            // (NB: we don't need to keep checking this once its true for a level - because mutexes decrease monotonically)
            if (goalElementsPresentAndNonMutex || (goalElementsPresentAndNonMutex = graphLevel.ContainsNonMutex(problem.EndGoal.Elements)))
            {
                var priorNoGoodsCount = noGoods.Count;

                // ..try to extract a solution:
                Plan? result = null;
                if (await Task.Run(() => TryExtractSolution(graphLevel, noGoods, out result, cancellationToken), cancellationToken))
                {
                    // If we managed to extract a solution, we're done. Return it.
                    return result!;
                }

                noGoodsLevelledOff = noGoods.Count == priorNoGoodsCount;
            }

            // If we haven't managed to extract a solution, and both the graph and no-goods have levelled off, we fail.
            if (graphLevel.IsLevelledOff && noGoodsLevelledOff)
            {
                throw new InvalidOperationException("Plan creation failed");
            }
        }
    }

    /// <inheritdoc />
    public override void Dispose()
    {
        // Nothing to do
        GC.SuppressFinalize(this);
    }

    private bool TryExtractSolution(
        PlanningGraphPropositionLevel graphLevel,
        HashSet<SearchState> noGoods,
        [MaybeNullWhen(false)] out Plan plan,
        CancellationToken cancellationToken)
    {
        // NB: AIaMA says that the target is *level is zero* and initial state meets the goal.
        // Don't actually need to check for level being zero. If we meet the goal at level n > 0, we can just
        // use no-op actions to get to that level then execute the plan.
        // In practice this won't happen because we'd have found the target at an earlier step anyway. 
        // So, what the book is trying (fairly badly..) to say is that when we first find the target, it'll be
        // at level zero.
        var search = new SolutionExtractionDFS(problem, graphLevel, noGoods);
        search.Complete(cancellationToken);

        if (search.IsSucceeded)
        {
            var actions = search
                .PathFromTarget()
                .SelectMany(e => e.Actions)
                .Where(a => !a.Identifier.Equals(PlanningGraph.PersistenceActionIdentifier.Instance));

            plan = new Plan(actions);
            return true;
        }
        else
        {
            plan = null;
            return false;
        }
    }

    private record struct SearchState(int Level, Goal Goal);

    // A node in the solution extraction search represents having a particular goal at a particular
    // level of the planning graph. The outbound edges of this node each represent sets of actions
    // applicable to the previous level (no pair of which are mutually exclusive) that collectively
    // meet the goal.
    [DebuggerDisplay("{Goal} @ L{graphLevel.Index}")]
    private readonly struct SearchNode : INode<SearchNode, SearchEdge>, IEquatable<SearchNode>
    {
        public SearchNode(PlanningGraphPropositionLevel graphLevel, Goal goal)
        {
            this.GraphLevel = graphLevel;
            this.Goal = goal;
        }

        public readonly Goal Goal { get; }

        public readonly PlanningGraphPropositionLevel GraphLevel { get; }

        public IReadOnlyCollection<SearchEdge> Edges => new SearchNodeEdges(GraphLevel, Goal);

        public override bool Equals(object? obj) => obj is SearchNode node && Equals(node);

        // NB: this struct is private - so we don't need to look at the planning graph, since it'll always match
        public bool Equals(SearchNode node) => GraphLevel.Index == node.GraphLevel.Index && Equals(Goal, node.Goal);

        public override int GetHashCode() => HashCode.Combine(GraphLevel.Index, Goal);
    }

    private readonly struct SearchNodeEdges : IReadOnlyCollection<SearchEdge>
    {
        private readonly PlanningGraphPropositionLevel graphLevel;
        private readonly Goal goal;

        public SearchNodeEdges(PlanningGraphPropositionLevel graphLevel, Goal goal)
        {
            this.graphLevel = graphLevel;
            this.goal = goal;
        }

        /// <inheritdoc />
        [SuppressMessage("CA", "CA1829", Justification = "False positive - this *is* the Count implementation")]
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
             * 
             * Artificial Intelligence: A Modern Approach (Russel & Norvig)
             */

            var graphLevel = this.graphLevel; // copy level variable because we can't access struct instance field in lambda

            // Order the goal elements by descending level cost:
            var goalElements = goal.Elements
                .OrderByDescending(e => graphLevel.Graph.GetLevelCost(e));

            // Find all of the actions that meet at least one element of the goal and order by sum of precondition level costs:
            // NB: While we use the term "relevant" here, note that we're not discounting those that clash with the goal - that will 
            // be dealt with by mutex checks.
            var relevantActionNodes = goal.Elements
                .SelectMany(e => graphLevel.NodesByProposition[e].Causes)
                .OrderBy(n => n.Preconditions.Sum(p => graphLevel.Graph.GetLevelCost(p.Proposition)))
                .Distinct(); // TODO: can probably do this before order by? ref equality, but we take action to avoid dups.

            // Now (recursively) attempt to cover all elements of the goal, with no mutexes:
            // We go recursive to ensure that we ultimately find all combinations - but this is of course potentially expensive.
            // There's no way around this, unfortunately - the set cover problem is NP-complete, after all..
            return FindCoveringActionSets(goalElements, relevantActionNodes.ToImmutableHashSet(), ImmutableHashSet<PlanningGraphActionNode>.Empty).GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private IEnumerable<SearchEdge> FindCoveringActionSets(
            IEnumerable<Literal> unmetGoalElements,
            ImmutableHashSet<PlanningGraphActionNode> unselectedActionNodes,
            ImmutableHashSet<PlanningGraphActionNode> selectedActionNodes)
        {
            // TODO-PERFORMANCE: a lot of GC pressure here, what with all the immutable hash sets.
            // We could eliminate the duplication by creating a tree instead, or even just some kind
            // of bit-vector struct to indicate selection. Meh, lets get it working first, then at
            // least we have a baseline for improvements.
            if (!unmetGoalElements.Any())
            {
                yield return new SearchEdge(graphLevel, goal, selectedActionNodes.Select(n => n.Action));
            }
            else
            {
                // Try to cover the first goal element (any others covered by the same action are a bonus),
                // then recurse for the other actions and remaining uncovered goal elements.
                foreach (var actionNode in unselectedActionNodes)
                {
                    if (actionNode.Action.Effect.Elements.Contains(unmetGoalElements.First()) && !actionNode.IsMutexWithAny(selectedActionNodes))
                    {
                        foreach (var edge in FindCoveringActionSets(
                            unmetGoalElements.Except(actionNode.Action.Effect.Elements),
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
        private readonly PlanningGraphPropositionLevel graphLevel;
        private readonly Goal goal;

        public SearchEdge(PlanningGraphPropositionLevel graphLevel, Goal goal, IEnumerable<Action> actions)
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
    }

    // Use our own search logic rather than a generic recursive DFS to
    // gracefully make note of no-goods. Raises questions of whether we
    // should bother with SCGraphTheory's abstractions, but meh, lets
    // keep them for now.
    //
    // TODO: this design is the worst of both worlds. If we're not going to use one of SCGraphTheorySearch's
    // DFS classes, I strongly suspect that just making solution extraction recursive would be much cleaner.
    private class SolutionExtractionDFS
    {
        private readonly SearchNode source;
        private readonly Problem problem;
        private readonly HashSet<SearchState> noGoods;
        private readonly Dictionary<SearchNode, SearchEdge> visited = new();

        public SolutionExtractionDFS(Problem problem, PlanningGraphPropositionLevel graphLevel, HashSet<SearchState> noGoods)
        {
            this.problem = problem;
            this.source = new SearchNode(graphLevel, problem.EndGoal);
            this.noGoods = noGoods;

            Visited = new ReadOnlyDictionary<SearchNode, SearchEdge>(visited);
            visited[source] = default;
        }

        public bool IsConcluded { get; private set; } = false;

        public bool IsSucceeded { get; private set; } = false;

        public SearchNode Target { get; private set; }

        public IReadOnlyDictionary<SearchNode, SearchEdge> Visited { get; }

        public void Complete(CancellationToken cancellationToken = default)
        {
            Visit(source, cancellationToken);
            IsConcluded = true;
        }

        public IEnumerable<SearchEdge> PathFromTarget()
        {
            if (!IsSucceeded)
            {
                throw new InvalidOperationException("Search failed");
            }

            for (var node = Target; !object.Equals(Visited[node], default(SearchEdge)); node = Visited[node].From)
            {
                yield return Visited[node];
            }
        }

        private void Visit(SearchNode node, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (problem.InitialState.Meets(node.Goal))
            {
                Target = node;
                IsSucceeded = true;
                return;
            }

            var enumerator = node.Edges.GetEnumerator();
            try
            {
                if (!enumerator.MoveNext())
                {
                    noGoods.Add(new(node.GraphLevel.Index, node.Goal));
                }
                else
                {
                    do
                    {
                        var nextEdge = enumerator.Current;
                        var nextNode = nextEdge.To;

                        if (!visited.ContainsKey(nextNode))
                        {
                            Visit(nextNode, cancellationToken);
                            visited[nextNode] = nextEdge;
                        }
                    }
                    while (!IsSucceeded && enumerator.MoveNext());
                }
            }
            finally
            {
                enumerator.Dispose();
            }
        }
    }
}
