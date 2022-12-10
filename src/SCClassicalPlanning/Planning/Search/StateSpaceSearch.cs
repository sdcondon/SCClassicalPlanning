// Copyright 2022 Simon Condon
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
using SCClassicalPlanning.Planning.Utilities;
using SCGraphTheory;
using SCGraphTheory.Search.Classic;
using System.Collections;

namespace SCClassicalPlanning.Planning.Search
{
    /// <summary>
    /// A simple implementation of <see cref="IPlanner"/> that carries out an A-star search of
    /// the state space to create plans.
    /// <para/>
    /// See section 10.2.2 of "Artificial Intelligence: A Modern Approach" for more on this.
    /// </summary>
    public class StateSpaceSearch : IPlanner
    {
        private readonly IHeuristic heuristic;
        private readonly Func<Action, float> getActionCost;

        /// <summary>
        /// Initializes a new instance of the <see cref="StateSpaceSearch"/> class that attempts to minimise the number of actions in the resulting plan.
        /// </summary>
        /// <param name="heuristic">The heuristic to use - with the "cost" being interpreted as the estimated number of actions that need to be performed.</param>
        public StateSpaceSearch(IHeuristic heuristic)
            : this(heuristic, e => 1f)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StateSpaceSearch"/> class that attempts to minimise the total "cost" of actions in the resulting plan.
        /// </summary>
        /// <param name="getActionCost">A delegate to retrieve the cost of an action.</param>
        /// <param name="heuristic">The heuristic to use - with the "cost" being interpreted as the estimated total cost of the actions that need to be performed.</param>
        public StateSpaceSearch(IHeuristic heuristic, Func<Action, float> getActionCost)
        {
            this.heuristic = heuristic;
            this.getActionCost = getActionCost;
        }

        /// <summary>
        /// Creates a (concretely-typed) planning task to work on solving a given problem.
        /// </summary>
        /// <param name="problem">The problem to create a plan for.</param>
        /// <returns></returns>
        public PlanningTask CreatePlanningTask(Problem problem) => new(problem, heuristic, getActionCost);

        /// <inheritdoc />
        IPlanningTask IPlanner.CreatePlanningTask(Problem problem) => CreatePlanningTask(problem);

        /// <summary>
        /// The implementation of <see cref="IPlanningTask"/> used by <see cref="StateSpaceSearch"/>.
        /// </summary>
        public class PlanningTask : SteppablePlanningTask<(State, Action, State)>
        {
            private readonly AStarSearch<StateSpaceNode, StateSpaceEdge> search;

            private bool isComplete;
            private Plan? result;

            internal PlanningTask(Problem problem, IHeuristic heuristic, Func<Action, float> getActionCost)
            {
                search = new AStarSearch<StateSpaceNode, StateSpaceEdge>(
                    source: new StateSpaceNode(problem, problem.InitialState),
                    isTarget: n => n.State.Satisfies(problem.Goal),
                    getEdgeCost: e => getActionCost(e.Action),
                    getEstimatedCostToTarget: n => heuristic.EstimateCost(n.State, problem.Goal));

                CheckForSearchCompletion();
            }

            /// <inheritdoc />
            public override bool IsComplete => isComplete;

            /// <inheritdoc />
            public override bool IsSucceeded => result != null;

            /// <inheritdoc />
            public override Plan Result
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
            public override (State, Action, State) NextStep()
            {
                if (IsComplete)
                {
                    throw new InvalidOperationException("Task is complete");
                }

                var edge = search.NextStep();
                CheckForSearchCompletion();

                return (edge.From.State, edge.Action, edge.To.State);
            }

            /// <inheritdoc />
            public override void Dispose()
            {
                // Nothing to do
                GC.SuppressFinalize(this);
            }

            private void CheckForSearchCompletion()
            {
                if (search.IsConcluded)
                {
                    if (search.IsSucceeded)
                    {
                        result = new Plan(search.PathToTarget().Select(e => e.Action).ToList());
                    }

                    isComplete = true;
                }
            }
        }

        private readonly struct StateSpaceNode : INode<StateSpaceNode, StateSpaceEdge>, IEquatable<StateSpaceNode>
        {
            private readonly Problem problem;

            public StateSpaceNode(Problem problem, State state) => (this.problem, State) = (problem, state);

            public State State { get; }

            public IReadOnlyCollection<StateSpaceEdge> Edges => new StateSpaceNodeEdges(problem, State);

            public override bool Equals(object? obj) => obj is StateSpaceNode node && Equals(node);

            // NB: this struct is private - so we don't need to look at the problem, since it'll always match
            public bool Equals(StateSpaceNode node) => Equals(State, node.State);

            public override int GetHashCode() => HashCode.Combine(State);
        }

        private readonly struct StateSpaceNodeEdges : IReadOnlyCollection<StateSpaceEdge>
        {
            private readonly Problem problem;
            private readonly State state;

            public StateSpaceNodeEdges(Problem problem, State state) => (this.problem, this.state) = (problem, state);

            public int Count => ProblemInspector.GetApplicableActions(problem, state).Count();

            public IEnumerator<StateSpaceEdge> GetEnumerator()
            {
                foreach (var action in ProblemInspector.GetApplicableActions(problem, state))
                {
                    yield return new StateSpaceEdge(problem, state, action);
                }
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        private readonly struct StateSpaceEdge : IEdge<StateSpaceNode, StateSpaceEdge>
        {
            private readonly Problem problem;
            private readonly State fromState;

            public StateSpaceEdge(Problem problem, State state, Action action)
            {
                this.problem = problem;
                this.fromState = state;
                this.Action = action;
            }

            public StateSpaceNode From => new(problem, fromState);

            public StateSpaceNode To => new(problem, Action.ApplyTo(fromState));

            public Action Action { get; }

            public override string ToString() => new PlanFormatter(problem.Domain).Format(Action);
        }
    }
}
