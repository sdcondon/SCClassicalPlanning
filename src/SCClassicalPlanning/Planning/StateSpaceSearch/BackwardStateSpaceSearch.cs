using SCGraphTheory;
using SCGraphTheory.Search.Classic;
using System.Collections;

namespace SCClassicalPlanning.Planning.StateSpaceSearch
{
    /// <summary>
    /// A simple implementation of <see cref="IPlanner"/> that carries out a backward (A-star) search of
    /// the state space to create plans.
    /// <para/>
    /// See section 10.2.2 of "Artificial Intelligence: A Modern Approach" for more on this.
    /// </summary>
    public class BackwardStateSpaceSearch : IPlanner
    {
        private readonly Func<Action, float> getActionCost;
        private readonly Func<State, Goal, float> estimateCostToGoal;

        /// <summary>
        /// Initializes a new instance of the <see cref="ForwardStateSpaceSearch"/> class that attempts to minimise the number of actions in the resulting plan.
        /// </summary>
        /// <param name="estimateCountOfActionsToGoal">The heuristic to use - should give an estimate of the number of actions required to get from the state represented by the first argument to a state that satisfies the goal represented by the second argument.</param>
        public BackwardStateSpaceSearch(Func<State, Goal, float> estimateCountOfActionsToGoal)
            : this(a => 1f, estimateCountOfActionsToGoal)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ForwardStateSpaceSearch"/> class that attempts to minimise the total "cost" of actions in the resulting plan.
        /// </summary>
        /// <param name="getActionCost">A delegate to retrieve the cost of an action.</param>
        /// <param name="estimateCostToGoal">The heuristic to use - should give an estimate of the total cost of the actions required to get from the state represented by the first argument to a state that satisfies the goal represented by the second argument.</param>
        public BackwardStateSpaceSearch(Func<Action, float> getActionCost, Func<State, Goal, float> estimateCostToGoal)
        {
            this.getActionCost = getActionCost;
            this.estimateCostToGoal = estimateCostToGoal;
        }

        /// <inheritdoc />
        public async Task<Plan> CreatePlanAsync(Problem problem)
        {
            var search = new AStarSearch<StateSpaceNode, StateSpaceEdge>(
                source: new StateSpaceNode(problem, problem.Goal),
                isTarget: n => n.Goal.IsSatisfiedBy(problem.InitialState),
                getEdgeCost: e => getActionCost(e.Action),
                getEstimatedCostToTarget: n => estimateCostToGoal(problem.InitialState, n.Goal));

            await Task.Run(() => search.Complete());
            ////var exploredEdges = new HashSet<StateSpaceEdge>();
            ////while (!search.IsConcluded)
            ////{
            ////    search.NextStep();

            ////    var newEdges = search.Visited.Values/*.Where(i => !i.IsOnFrontier)*/.Select(i => i.Edge);
            ////    foreach (var edge in newEdges)
            ////    {
            ////        if (!object.Equals(edge, default(StateSpaceEdge)) && !exploredEdges.Contains(edge))
            ////        {
            ////            var heuristic = estimateCountOfActionsToGoal(problem.InitialState, edge.To.Goal);
            ////            exploredEdges.Add(edge);
            ////        }
            ////    }
            ////}

            // TODO: handle failure gracefully..
            return new Plan(search.PathToTarget().Reverse().Select(e => e.Action).ToList());
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
            // NB: this struct is private - so we don't need to look at the problem, since it'll always match
            public bool Equals(StateSpaceNode node) => node.Goal.Equals(Goal);

            /// <inheritdoc />
            public override int GetHashCode() => HashCode.Combine(Goal);

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
            public StateSpaceNode From => new StateSpaceNode(problem, fromGoal);

            /// <inheritdoc />
            public StateSpaceNode To => new StateSpaceNode(problem, toGoal);

            /// <summary>
            /// Gets the action that is regressed over to achieve this goal transition.
            /// </summary>
            public Action Action { get; }

            /// <inheritdoc />
            public override string ToString() => new PlanFormatter(problem.Domain).Format(Action);
        }
    }
}
