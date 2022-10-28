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
    public class BackwardStateSpaceSearch_LiftedWithoutKB : IPlanner
    {
        private readonly IHeuristic heuristic;
        private readonly Func<Action, float> getActionCost;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ForwardStateSpaceSearch"/> class that attempts to minimise the number of actions in the resulting plan.
        /// </summary>
        /// <param name="heuristic">The heuristic to use - the returned cost will be interpreted as the estimated number of actions that need to be performed.</param>
        public BackwardStateSpaceSearch_LiftedWithoutKB(IHeuristic heuristic)
            : this(heuristic, a => 1f)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ForwardStateSpaceSearch"/> class that attempts to minimise the total "cost" of actions in the resulting plan.
        /// </summary>
        /// <param name="heuristic">The heuristic to use - with the returned cost will be interpreted as the estimated total cost of the actions that need to be performed.</param>
        /// <param name="getActionCost">A delegate to retrieve the cost of an action.</param>
        public BackwardStateSpaceSearch_LiftedWithoutKB(IHeuristic heuristic, Func<Action, float> getActionCost)
        {
            this.heuristic = heuristic;
            this.getActionCost = getActionCost;
        }

        /// <inheritdoc />
        public async Task<Plan> CreatePlanAsync(Problem problem, CancellationToken cancellationToken = default)
        {
            var search = new AStarSearch<StateSpaceNode, StateSpaceEdge>(
                source: new StateSpaceNode(problem.Domain, problem.Goal),
                isTarget: n => problem.InitialState.GetSatisfyingSubstitutions(n.Goal).Any(),
                getEdgeCost: e => getActionCost(e.Action),
                getEstimatedCostToTarget: n => heuristic.EstimateCost(problem.InitialState, n.Goal));

            await search.CompleteAsync(1, cancellationToken);
            ////var exploredEdges = new HashSet<StateSpaceEdge>();
            ////while (!search.IsConcluded)
            ////{
            ////    search.NextStep();

            ////    var newEdges = search.Visited.Values.Where(i => !i.IsOnFrontier).Select(i => i.Edge);
            ////    foreach (var edge in newEdges)
            ////    {
            ////        if (!object.Equals(edge, default(StateSpaceEdge)) && !exploredEdges.Contains(edge))
            ////        {
            ////            var h = heuristic.EstimateCost(problem.InitialState, edge.To.Goal);
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
            private readonly Domain domain;

            public StateSpaceNode(Domain domain, Goal goal) => (this.domain, Goal) = (domain, goal);

            /// <summary>
            /// Gets the goal represented by this node.
            /// </summary>
            public Goal Goal { get; }

            /// <inheritdoc />
            public IReadOnlyCollection<StateSpaceEdge> Edges => new StateSpaceNodeEdges(domain, Goal);

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
            private readonly Domain domain;
            private readonly Goal goal;

            public StateSpaceNodeEdges(Domain domain, Goal goal) => (this.domain, this.goal) = (domain, goal);

            /// <inheritdoc />
            public int Count => DomainInspector.GetRelevantActions(domain, goal).Count();

            /// <inheritdoc />
            public IEnumerator<StateSpaceEdge> GetEnumerator()
            {
                foreach (var action in DomainInspector.GetRelevantActions(domain, goal))
                {
                    yield return new StateSpaceEdge(domain, goal, action);
                }
            }

            /// <inheritdoc />
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        private struct StateSpaceEdge : IEdge<StateSpaceNode, StateSpaceEdge>
        {
            private readonly Domain domain;
            private readonly Goal fromGoal;

            public StateSpaceEdge(Domain domain, Goal fromGoal, Action action)
            {
                this.domain = domain;
                this.fromGoal = fromGoal;
                this.Action = action;
            }

            /// <inheritdoc />
            public StateSpaceNode From => new(domain, fromGoal);

            /// <inheritdoc />
            public StateSpaceNode To => new(domain, Action.Regress(fromGoal));

            /// <summary>
            /// Gets the action that is regressed over to achieve this goal transition.
            /// </summary>
            public Action Action { get; }

            /// <inheritdoc />
            public override string ToString() => new PlanFormatter(domain).Format(Action);
        }
    }
}
