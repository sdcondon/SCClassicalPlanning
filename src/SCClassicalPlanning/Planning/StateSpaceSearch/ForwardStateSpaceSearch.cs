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
        private readonly Func<State, Goal, float> estimateCountOfActionsToGoal;

        /// <summary>
        /// Initializes a new instance of the <see cref="ForwardStateSpaceSearch"/> class.
        /// </summary>
        /// <param name="estimateCountOfActionsToGoal">The heuristic to use - should give an estimate of the number of actions required to get from the state represented by the first argument to a state that satisfies the goal represented by the second argument.</param>
        public ForwardStateSpaceSearch(Func<State, Goal, float> estimateCountOfActionsToGoal) => this.estimateCountOfActionsToGoal = estimateCountOfActionsToGoal;

        /// <inheritdoc />
        public async Task<Plan> CreatePlanAsync(Problem problem)
        {
            var search = new AStarSearch<StateSpaceNode, StateSpaceEdge>(
                source: new StateSpaceNode(problem, problem.InitialState),
                isTarget: n => n.State.Satisfies(problem.Goal),
                getEdgeCost: e => 1,
                getEstimatedCostToTarget: n => estimateCountOfActionsToGoal(n.State, problem.Goal));

            await Task.Run(() => search.Complete()); // todo?: worth adding all the Steppable stuff like in FoL?

            // TODO: handle failure gracefully..
            return new Plan(search.PathToTarget().Select(e => e.Action).ToList());
        }

        private struct StateSpaceNode : INode<StateSpaceNode, StateSpaceEdge>
        {
            private readonly Problem problem;

            public StateSpaceNode(Problem problem, State state) => (this.problem, State) = (problem, state);

            /// <summary>
            /// Gets the state represented by this node.
            /// </summary>
            public State State { get; }

            /// <inheritdoc />
            public IReadOnlyCollection<StateSpaceEdge> Edges => new StateSpaceNodeEdges(problem, State);
        }

        private struct StateSpaceNodeEdges : IReadOnlyCollection<StateSpaceEdge>
        {
            private readonly Problem problem;
            private readonly State state;

            public StateSpaceNodeEdges(Problem problem, State state) => (this.problem, this.state) = (problem, state);

            /// <inheritdoc />
            public int Count => problem.GetApplicableActions(state).Count();

            /// <inheritdoc />
            public IEnumerator<StateSpaceEdge> GetEnumerator()
            {
                foreach (var action in problem.GetApplicableActions(state))
                {
                    yield return new StateSpaceEdge(
                        new StateSpaceNode(problem, state),
                        new StateSpaceNode(problem, action.ApplyTo(state)),
                        action);
                }
            }

            /// <inheritdoc />
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        private struct StateSpaceEdge : IEdge<StateSpaceNode, StateSpaceEdge>
        {
            public StateSpaceEdge(StateSpaceNode from, StateSpaceNode to, Action action) => (From, To, Action) = (from, to, action);

            /// <inheritdoc />
            public StateSpaceNode From { get; }

            /// <inheritdoc />
            public StateSpaceNode To { get; }

            /// <summary>
            /// Gets the action that is carried out to achieve this state transition.
            /// </summary>
            public Action Action { get; }
        }
    }
}
