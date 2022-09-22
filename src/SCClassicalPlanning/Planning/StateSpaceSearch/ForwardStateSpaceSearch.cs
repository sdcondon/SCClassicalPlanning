using SCGraphTheory;
using SCGraphTheory.Search.Classic;
using System.Collections.ObjectModel;

namespace SCClassicalPlanning.Planning.StateSpaceSearch
{
    public class ForwardStateSpaceSearch : IPlanner
    {
        private readonly Func<State, State, float> heuristic;

        /// <summary>
        /// Initializes a new instance of the <see cref="ForwardStateSpaceSearch"/> class.
        /// </summary>
        /// <param name="heuristic">The heuristic to use.</param>
        public ForwardStateSpaceSearch(Func<State, State, float> heuristic) => this.heuristic = heuristic;

        /// <inheritdoc />
        async Task<IPlan> IPlanner.CreatePlanAsync(Problem problem) => await CreatePlanAsync(problem);

        public async Task<Plan> CreatePlanAsync(Problem problem)
        {
            var search = new AStarSearch<StateSpaceNode, StateSpaceEdge>(
                source: new StateSpaceNode(problem, problem.InitialState),
                isTarget: n => n.State.IsSuperstateOf(problem.GoalState),
                getEdgeCost: e => 0,
                getEstimatedCostToTarget: n => heuristic(n.State, problem.GoalState));

            await Task.Run(() => search.Complete());

            return new Plan(search.PathToTarget().Select(e => e.Action).ToList());
        }

        public class Plan : IPlan
        {
            public Plan(IList<Action> steps) => Steps = new ReadOnlyCollection<Action>(steps);

            public IReadOnlyCollection<Action> Steps { get; }
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
            public IReadOnlyCollection<StateSpaceEdge> Edges
            {
                get
                {
                    // TODO: Find applicable actions, new state is action applied to this state.
                    throw new NotImplementedException();
                }
            }
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
            /// <para/>
            /// TODO: Ref type. Given that, is there really much value in val types for nodes and edges. Test me.
            /// </summary>
            public Action Action { get; }
        }
    }
}
