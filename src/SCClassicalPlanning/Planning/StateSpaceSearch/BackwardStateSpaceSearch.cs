using SCGraphTheory;
using SCGraphTheory.Search.Classic;
using System.Collections.ObjectModel;

namespace SCClassicalPlanning.Planning.StateSpaceSearch
{
    /// <summary>
    /// A simple implementation of <see cref="IPlanner"/> that carries out a backward search of
    /// the state space to create plans. See section 10.2.2 of "Artificial Intelligence: A Modern
    /// Approach" for more on this.
    /// </summary>
    public class BackwardStateSpaceSearch : IPlanner
    {
        private readonly Func<State, Goal, float> heuristic;

        /// <summary>
        /// Initializes a new instance of the <see cref="ForwardStateSpaceSearch"/> class.
        /// </summary>
        /// <param name="heuristic">The heuristic to use.</param>
        public BackwardStateSpaceSearch(Func<State, Goal, float> heuristic) => this.heuristic = heuristic;

        /// <inheritdoc />
        async Task<IPlan> IPlanner.CreatePlanAsync(Problem problem) => await CreatePlanAsync(problem);

        public async Task<Plan> CreatePlanAsync(Problem problem)
        {
            var search = new AStarSearch<StateSpaceNode, StateSpaceEdge>(
                source: new StateSpaceNode(problem, problem.Goal),
                isTarget: n => n.Goal.IsSatisfiedBy(problem.InitialState),
                getEdgeCost: e => 1,
                getEstimatedCostToTarget: n => heuristic(problem.InitialState, n.Goal));

            await Task.Run(() => search.Complete());

            return new Plan(search.PathToTarget().Select(e => e.Action).ToList());
        }

        // IsRelevantTo

        public class Plan : IPlan
        {
            public Plan(IList<Action> steps) => Steps = new ReadOnlyCollection<Action>(steps);

            public IReadOnlyCollection<Action> Steps { get; }
        }

        private struct StateSpaceNode : INode<StateSpaceNode, StateSpaceEdge>
        {
            private readonly Problem problem;

            public StateSpaceNode(Problem problem, Goal goal) => (this.problem, Goal) = (problem, goal);

            /// <summary>
            /// Gets the goal represented by this node.
            /// </summary>
            public Goal Goal { get; }

            /// <inheritdoc />
            public IReadOnlyCollection<StateSpaceEdge> Edges
            {
                get
                {
                    // TODO: Find relevant actions, new state is state regressed over action.
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
            /// Gets the action that is regressed over to achieve this state transition.
            /// <para/>
            /// TODO: Ref type. Given that, is there really much value in val types for nodes and edges. Test me.
            /// </summary>
            public Action Action { get; }
        }
    }
}
