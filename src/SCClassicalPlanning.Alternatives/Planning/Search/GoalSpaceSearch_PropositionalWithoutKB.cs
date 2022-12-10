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
    /// the goal space to create plans.
    /// <para/>
    /// See section 10.2.2 of "Artificial Intelligence: A Modern Approach" for more on this.
    /// <para/>
    /// Differs from the library version in that it is completely propositional - variables are expanded
    /// out to every possible value whenever they occur. This is obviously suboptimal from a performance perspective.
    /// </summary>
    public class GoalSpaceSearch_PropositionalWithoutKB : IPlanner
    {
        private readonly IHeuristic heuristic;
        private readonly Func<Action, float> getActionCost;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ForwardStateSpaceSearch"/> class that attempts to minimise the number of actions in the resulting plan.
        /// </summary>
        /// <param name="heuristic">The heuristic to use - the returned cost will be interpreted as the estimated number of actions that need to be performed.</param>
        public GoalSpaceSearch_PropositionalWithoutKB(IHeuristic heuristic)
            : this(heuristic, a => 1f)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ForwardStateSpaceSearch"/> class that attempts to minimise the total "cost" of actions in the resulting plan.
        /// </summary>
        /// <param name="heuristic">The heuristic to use - with the returned cost will be interpreted as the estimated total cost of the actions that need to be performed.</param>
        /// <param name="getActionCost">A delegate to retrieve the cost of an action.</param>
        public GoalSpaceSearch_PropositionalWithoutKB(IHeuristic heuristic, Func<Action, float> getActionCost)
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
        /// The implementation of <see cref="IPlanningTask"/> used by <see cref="GoalSpaceSearch_PropositionalWithKB"/>.
        /// </summary>
        public class PlanningTask : SteppablePlanningTask<(Goal, Action, Goal)>
        {
            private readonly AStarSearch<GoalSpaceNode, GoalSpaceEdge> search;

            private bool isComplete;
            private Plan? result;

            internal PlanningTask(Problem problem, IHeuristic heuristic, Func<Action, float> getActionCost)
            {
                search = new AStarSearch<GoalSpaceNode, GoalSpaceEdge>(
                    source: new GoalSpaceNode(problem, problem.Goal),
                    isTarget: n => n.Goal.IsSatisfiedBy(problem.InitialState),
                    getEdgeCost: e => getActionCost(e.Action),
                    getEstimatedCostToTarget: n => heuristic.EstimateCost(problem.InitialState, n.Goal));

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
            public override (Goal, Action, Goal) NextStep()
            {
                if (IsComplete)
                {
                    throw new InvalidOperationException("Task is complete");
                }

                var edge = search.NextStep();
                CheckForSearchCompletion();

                return (edge.From.Goal, edge.Action, edge.To.Goal);
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
                        result = new Plan(search.PathToTarget().Reverse().Select(e => e.Action).ToList());
                    }

                    isComplete = true;
                }
            }
        }

        private readonly struct GoalSpaceNode : INode<GoalSpaceNode, GoalSpaceEdge>, IEquatable<GoalSpaceNode>
        {
            private readonly Problem problem;

            public GoalSpaceNode(Problem problem, Goal goal) => (this.problem, Goal) = (problem, goal);

            /// <summary>
            /// Gets the goal represented by this node.
            /// </summary>
            public Goal Goal { get; }

            /// <inheritdoc />
            public IReadOnlyCollection<GoalSpaceEdge> Edges => new GoalSpaceNodeEdges(problem, Goal);

            /// <inheritdoc />
            public override bool Equals(object? obj) => obj is GoalSpaceNode node && Equals(node);

            /// <inheritdoc />
            // NB: this struct is private - so we don't need to look at the problem, since it'll always match
            public bool Equals(GoalSpaceNode node) => Equals(Goal, node.Goal);

            /// <inheritdoc />
            public override int GetHashCode() => HashCode.Combine(Goal);

            /// <inheritdoc />
            public override string ToString() => Goal.ToString();
        }

        private readonly struct GoalSpaceNodeEdges : IReadOnlyCollection<GoalSpaceEdge>
        {
            private readonly Problem problem;
            private readonly Goal goal;

            public GoalSpaceNodeEdges(Problem problem, Goal goal) => (this.problem, this.goal) = (problem, goal);

            /// <inheritdoc />
            public int Count => ProblemInspector.GetRelevantActions(problem, goal).Count();

            /// <inheritdoc />
            public IEnumerator<GoalSpaceEdge> GetEnumerator()
            {
                foreach (var action in ProblemInspector.GetRelevantActions(problem, goal))
                {
                    yield return new GoalSpaceEdge(problem, goal, action);
                }
            }

            /// <inheritdoc />
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        private readonly struct GoalSpaceEdge : IEdge<GoalSpaceNode, GoalSpaceEdge>
        {
            private readonly Problem problem;
            private readonly Goal fromGoal;

            public GoalSpaceEdge(Problem problem, Goal fromGoal, Action action)
            {
                this.problem = problem;
                this.fromGoal = fromGoal;
                this.Action = action;
            }

            /// <inheritdoc />
            public GoalSpaceNode From => new(problem, fromGoal);

            /// <inheritdoc />
            public GoalSpaceNode To => new(problem, Action.Regress(fromGoal));

            /// <summary>
            /// Gets the action that is regressed over to achieve this goal transition.
            /// </summary>
            public Action Action { get; }

            /// <inheritdoc />
            public override string ToString() => new PlanFormatter(problem.Domain).Format(Action);
        }
    }
}
