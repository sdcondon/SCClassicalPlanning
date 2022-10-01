using SCGraphTheory;
using SCGraphTheory.Search.Classic;
using System.Collections;
using System.Collections.ObjectModel;

namespace SCClassicalPlanning.Planning.StateSpaceSearch
{
    public class ForwardStateSpaceSearch : IPlanner
    {
        private readonly Func<State, Goal, float> heuristic;

        /// <summary>
        /// Initializes a new instance of the <see cref="ForwardStateSpaceSearch"/> class.
        /// </summary>
        /// <param name="heuristic">The heuristic to use - should give an estimate of the number of actions required to get from the state represented by the first argument to a state that satisfies the goal represented by the second argument.</param>
        public ForwardStateSpaceSearch(Func<State, Goal, float> heuristic) => this.heuristic = heuristic;

        /// <inheritdoc />
        async Task<IPlan> IPlanner.CreatePlanAsync(Problem problem) => await CreatePlanAsync(problem);

        public async Task<Plan> CreatePlanAsync(Problem problem)
        {
            var search = new AStarSearch<StateSpaceNode, StateSpaceEdge>(
                source: new StateSpaceNode(problem, problem.InitialState),
                isTarget: n => problem.Goal.IsSatisfiedBy(n.State),
                getEdgeCost: e => 1,
                getEstimatedCostToTarget: n => heuristic(n.State, problem.Goal));

            await Task.Run(() => search.Complete()); // todo: worth adding all the Steppable stuff like in FoL?

            // TODO: will throw nullref if search fails. can do better.
            return new Plan(search.PathToTarget().Select(e => e.Action).ToList());
        }

        /// <summary>
        /// Plan implementation
        /// </summary>
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
            public IReadOnlyCollection<StateSpaceEdge> Edges => new StateSpaceNodeEdges(problem, State);
        }

        private struct StateSpaceNodeEdges : IReadOnlyCollection<StateSpaceEdge>
        {
            private readonly Problem problem;
            private readonly State state;

            public StateSpaceNodeEdges(Problem problem, State state) => (this.problem, this.state) = (problem, state);

            public int Count => this.Count();

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
            /// <para/>
            /// TODO: Ref type. Given that, is there really much value in val types for nodes and edges? Test me.
            /// </summary>
            public Action Action { get; }
        }
    }
}
