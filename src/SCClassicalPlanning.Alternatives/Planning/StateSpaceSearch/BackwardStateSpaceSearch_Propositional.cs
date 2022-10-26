﻿using SCGraphTheory;
using SCGraphTheory.Search.Classic;
using System.Collections;

namespace SCClassicalPlanning.Planning.StateSpaceSearch
{
    /// <summary>
    /// A simple implementation of <see cref="IPlanner"/> that carries out a backward (A-star) search of
    /// the state space to create plans.
    /// <para/>
    /// See section 10.2.2 of "Artificial Intelligence: A Modern Approach" for more on this.
    /// <para/>
    /// Differs from the library version in that it is completely propositional - variables are expanded
    /// out to every possible value whenever they occur. This is obviously suboptimal from a performance perspective.
    /// </summary>
    public class BackwardStateSpaceSearch_Propositional : IPlanner
    {
        private readonly IHeuristic heuristic;
        private readonly Func<Action, float> getActionCost;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ForwardStateSpaceSearch"/> class that attempts to minimise the number of actions in the resulting plan.
        /// </summary>
        /// <param name="heuristic">The heuristic to use - the returned cost will be interpreted as the estimated number of actions that need to be performed.</param>
        public BackwardStateSpaceSearch_Propositional(IHeuristic heuristic)
            : this(heuristic, a => 1f)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ForwardStateSpaceSearch"/> class that attempts to minimise the total "cost" of actions in the resulting plan.
        /// </summary>
        /// <param name="heuristic">The heuristic to use - with the returned cost will be interpreted as the estimated total cost of the actions that need to be performed.</param>
        /// <param name="getActionCost">A delegate to retrieve the cost of an action.</param>
        public BackwardStateSpaceSearch_Propositional(IHeuristic heuristic, Func<Action, float> getActionCost)
        {
            this.heuristic = heuristic;
            this.getActionCost = getActionCost;
        }

        /// <inheritdoc />
        public async Task<Plan> CreatePlanAsync(Problem problem, CancellationToken cancellationToken = default)
        {
            var search = new AStarSearch<StateSpaceNode, StateSpaceEdge>(
                source: new StateSpaceNode(problem, problem.Goal),
                isTarget: n => n.Goal.IsSatisfiedBy(problem.InitialState),
                getEdgeCost: e => getActionCost(e.Action),
                getEstimatedCostToTarget: n => heuristic.EstimateCost(problem.InitialState, n.Goal));

            await search.CompleteAsync(1, cancellationToken);
            // TODO: support interrogable plans
            ////var exploredEdges = new HashSet<StateSpaceEdge>();
            ////while (!search.IsConcluded)
            ////{
            ////    search.NextStep();

            ////    var newEdges = search.Visited.Values.Where(i => !i.IsOnFrontier).Select(i => i.Edge);
            ////    foreach (var edge in newEdges)
            ////    {
            ////        if (!object.Equals(edge, default(StateSpaceEdge)) && !exploredEdges.Contains(edge))
            ////        {
            ////            var heuristic = estimateCostToGoal(problem.InitialState, edge.To.Goal);
            ////            exploredEdges.Add(edge);
            ////        }
            ////    }
            ////}

            if (search.IsSucceeded())
            {
                return new Plan(search.PathToTarget().Reverse().Select(e => e.Action).ToList());
            }
            else
            {
                throw new ArgumentException("Problem is unsolvable", nameof(problem));
            }
        }

        private struct StateSpaceNode : INode<StateSpaceNode, StateSpaceEdge>, IEquatable<StateSpaceNode>
        {
            private readonly Problem problem;

            public StateSpaceNode(Problem problem, Goal goal) => (this.problem, Goal) = (problem, goal);

            /// <summary>
            /// Gets the goal represented by this node.
            /// </summary>
            public Goal Goal { get; }

            /// <inheritdoc />
            public IReadOnlyCollection<StateSpaceEdge> Edges => new StateSpaceNodeEdges(problem, Goal);

            /// <inheritdoc />
            public override bool Equals(object? obj) => obj is StateSpaceNode node && Equals(node);

            /// <inheritdoc />
            // NB: this struct is private - so we don't need to look at the problem, since it'll always match
            public bool Equals(StateSpaceNode node) => Equals(Goal, node.Goal);

            /// <inheritdoc />
            public override int GetHashCode() => HashCode.Combine(Goal);

            /// <inheritdoc />
            public override string ToString() => Goal.ToString();
        }

        private struct StateSpaceNodeEdges : IReadOnlyCollection<StateSpaceEdge>
        {
            private readonly Problem problem;
            private readonly Goal goal;

            public StateSpaceNodeEdges(Problem problem, Goal goal) => (this.problem, this.goal) = (problem, goal);

            /// <inheritdoc />
            public int Count => ProblemInspector.GetRelevantActions(problem, goal).Count();

            /// <inheritdoc />
            public IEnumerator<StateSpaceEdge> GetEnumerator()
            {
                foreach (var action in ProblemInspector.GetRelevantActions(problem, goal))
                {
                    yield return new StateSpaceEdge(problem, goal, action);
                }
            }

            /// <inheritdoc />
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        private struct StateSpaceEdge : IEdge<StateSpaceNode, StateSpaceEdge>
        {
            private readonly Problem problem;
            private readonly Goal fromGoal;
            private readonly Goal toGoal;

            public StateSpaceEdge(Problem problem, Goal fromGoal, Action action)
            {
                this.problem = problem;
                this.fromGoal = fromGoal;
                this.toGoal = action.Regress(fromGoal);
                this.Action = action;
            }

            /// <inheritdoc />
            public StateSpaceNode From => new(problem, fromGoal);

            /// <inheritdoc />
            public StateSpaceNode To => new(problem, toGoal);

            /// <summary>
            /// Gets the action that is regressed over to achieve this goal transition.
            /// </summary>
            public Action Action { get; }

            /// <inheritdoc />
            public override string ToString() => new PlanFormatter(problem.Domain).Format(Action);
        }
    }
}
