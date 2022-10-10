using SCGraphTheory;
using SCGraphTheory.Search.Classic;
using System.Collections;

namespace SCClassicalPlanning.Planning.StateSpaceSearch
{
    /// <summary>
    /// A simple implementation of <see cref="IPlanner"/> that carries out a forward (A-star) search of
    /// the state space to create plans.
    /// <para/>
    /// See section 10.2.2 of "Artificial Intelligence: A Modern Approach" for more on this.
    /// </summary>
    public class ForwardStateSpaceSearch : IPlanner
    {
        private readonly Func<Action, float> getActionCost;
        private readonly Func<State, Goal, float> estimateCostToGoal;

        /// <summary>
        /// Initializes a new instance of the <see cref="ForwardStateSpaceSearch"/> class that attempts to minimise the number of actions in the resulting plan.
        /// </summary>
        /// <param name="estimateCountOfActionsToGoal">The heuristic to use - should give an estimate of the number of actions required to get from the state represented by the first argument to a state that satisfies the goal represented by the second argument.</param>
        public ForwardStateSpaceSearch(Func<State, Goal, float> estimateCountOfActionsToGoal)
            : this(a => 1f, estimateCountOfActionsToGoal)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ForwardStateSpaceSearch"/> class that attempts to minimise the total "cost" of actions in the resulting plan.
        /// </summary>
        /// <param name="getActionCost">A delegate to retrieve the cost of an action.</param>
        /// <param name="estimateCostToGoal">The heuristic to use - should give an estimate of the total cost of the actions required to get from the state represented by the first argument to a state that satisfies the goal represented by the second argument.</param>
        public ForwardStateSpaceSearch(Func<Action, float> getActionCost, Func<State, Goal, float> estimateCostToGoal)
        {
            this.getActionCost = getActionCost;
            this.estimateCostToGoal = estimateCostToGoal;
        }

        /// <inheritdoc />
        public async Task<Plan> CreatePlanAsync(Problem problem, CancellationToken cancellationToken = default)
        {
            var search = new AStarSearch<StateSpaceNode, StateSpaceEdge>(
                source: new StateSpaceNode(problem, problem.InitialState),
                isTarget: n => n.State.Satisfies(problem.Goal),
                getEdgeCost: e => getActionCost(e.Action),
                getEstimatedCostToTarget: n => estimateCostToGoal(n.State, problem.Goal));

            await search.CompleteAsync(1, cancellationToken); // todo?: worth adding all the Steppable stuff like in FoL?

            if (!object.Equals(search.Target, default(StateSpaceNode)))
            {
                return new Plan(search.PathToTarget().Select(e => e.Action).ToList());
            }
            else
            {
                throw new ArgumentException("Problem is unsolvable", nameof(problem));
            }
        }

        private struct StateSpaceNode : INode<StateSpaceNode, StateSpaceEdge>, IEquatable<StateSpaceNode>
        {
            private readonly Problem problem;

            public StateSpaceNode(Problem problem, State state) => (this.problem, State) = (problem, state);

            /// <summary>
            /// Gets the state represented by this node.
            /// </summary>
            public State State { get; }

            /// <inheritdoc />
            public IReadOnlyCollection<StateSpaceEdge> Edges => new StateSpaceNodeEdges(problem, State);

            /// <inheritdoc />
            // NB: this struct is private - so we don't need to look at the problem, since it'll always match
            public bool Equals(StateSpaceNode node) => node.State.Equals(State);

            /// <inheritdoc />
            public override int GetHashCode() => HashCode.Combine(State);
        }

        private struct StateSpaceNodeEdges : IReadOnlyCollection<StateSpaceEdge>
        {
            private readonly Problem problem;
            private readonly State state;

            public StateSpaceNodeEdges(Problem problem, State state) => (this.problem, this.state) = (problem, state);

            /// <inheritdoc />
            public int Count => ProblemInspector.GetApplicableActions(problem, state).Count();

            /// <inheritdoc />
            public IEnumerator<StateSpaceEdge> GetEnumerator()
            {
                foreach (var action in ProblemInspector.GetApplicableActions(problem, state))
                {
                    yield return new StateSpaceEdge(problem, state, action);
                }
            }

            /// <inheritdoc />
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        private struct StateSpaceEdge : IEdge<StateSpaceNode, StateSpaceEdge>
        {
            private readonly Problem problem;
            private readonly State fromState;
            private readonly State toState;

            public StateSpaceEdge(Problem problem, State state, Action action)
            {
                this.problem = problem;
                this.fromState = state;
                this.toState = action.ApplyTo(fromState);
                this.Action = action;
            }

            /// <inheritdoc />
            public StateSpaceNode From => new StateSpaceNode(problem, fromState);

            /// <inheritdoc />
            public StateSpaceNode To => new StateSpaceNode(problem, toState);

            /// <summary>
            /// Gets the action that is applied to achieve this goal transition.
            /// </summary>
            public Action Action { get; }

            /// <inheritdoc />
            public override string ToString() => new PlanFormatter(problem.Domain).Format(Action);
        }
    }
}
