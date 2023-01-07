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
using SCFirstOrderLogic.Inference;
using SCGraphTheory;
using SCGraphTheory.Search.Classic;
using System.Collections;

namespace SCClassicalPlanning.Planning.Search
{
    /// <summary>
    /// A simple implementation of <see cref="IPlanner"/> that carries out an A-star search of
    /// the goal space to create plans.
    /// </summary>
    public class GoalSpaceSearch_LiftedWithKB : IPlanner
    {
        private readonly IStrategy strategy;
        private readonly InvariantInspector? invariantInspector;

        /// <summary>
        /// Initializes a new instance of the <see cref="GoalSpaceSearch_LiftedWithKB"/> class.
        /// </summary>
        /// <param name="strategy">The strategy to use.</param>
        public GoalSpaceSearch_LiftedWithKB(IStrategy strategy, IKnowledgeBase? invariantsKB = null)
        {
            this.strategy = strategy;
            this.invariantInspector = invariantsKB != null ? new InvariantInspector(invariantsKB) : null;
        }

        /// <summary>
        /// Creates a (concretely-typed) planning task to work on solving a given problem.
        /// </summary>
        /// <param name="problem">The problem to create a plan for.</param>
        /// <returns></returns>
        public PlanningTask CreatePlanningTask(Problem problem) => new(problem, strategy, invariantInspector);

        /// <inheritdoc />
        IPlanningTask IPlanner.CreatePlanningTask(Problem problem) => CreatePlanningTask(problem);

        /// <summary>
        /// The implementation of <see cref="IPlanningTask"/> used by <see cref="GoalSpaceSearch_LiftedWithKB"/>.
        /// </summary>
        public class PlanningTask : SteppablePlanningTask<(Goal, Action, Goal)>
        {
            private readonly AStarSearch<GoalSpaceNode, GoalSpaceEdge> search;

            private bool isComplete;
            private Plan? result;

            internal PlanningTask(Problem problem, IStrategy strategy, InvariantInspector? invariantInspector)
            {
                Domain = problem.Domain;
                InvariantInspector = invariantInspector;

                search = new AStarSearch<GoalSpaceNode, GoalSpaceEdge>(
                    source: new GoalSpaceNode(this, problem.Goal),
                    isTarget: n => problem.InitialState.GetSatisfyingSubstitutions(n.Goal).Any(),
                    getEdgeCost: e => strategy.GetCost(e.Action),
                    getEstimatedCostToTarget: n => strategy.EstimateCost(problem.InitialState, n.Goal));

                CheckForSearchCompletion();
            }

            public Domain Domain { get; }

            public InvariantInspector? InvariantInspector { get; }

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
            private readonly PlanningTask planningTask;

            public GoalSpaceNode(PlanningTask planningTask, Goal goal) => (this.planningTask, Goal) = (planningTask, goal);

            /// <summary>
            /// Gets the goal represented by this node.
            /// </summary>
            public Goal Goal { get; }

            /// <inheritdoc />
            public IReadOnlyCollection<GoalSpaceEdge> Edges => new GoalSpaceNodeEdges(planningTask, Goal);

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
            private readonly PlanningTask planningTask;
            private readonly Goal goal;

            public GoalSpaceNodeEdges(PlanningTask planningTask, Goal goal) => (this.planningTask, this.goal) = (planningTask, goal);

            /// <inheritdoc />
            public int Count => DomainInspector.GetRelevantActions(planningTask.Domain, goal).Count();

            /// <inheritdoc />
            public IEnumerator<GoalSpaceEdge> GetEnumerator()
            {
                foreach (var action in DomainInspector.GetRelevantActions(planningTask.Domain, goal))
                {
                    if (planningTask.InvariantInspector != null)
                    {
                        var effectiveAction = action;

                        var nonTrivialPreconditions = planningTask.InvariantInspector.RemoveTrivialElements(action.Precondition);
                        if (nonTrivialPreconditions != action.Precondition)
                        {
                            effectiveAction = new(action.Identifier, nonTrivialPreconditions, action.Effect);
                        }

                        if (!planningTask.InvariantInspector.IsGoalPrecludedByInvariants(effectiveAction.Regress(goal)))
                        {
                            yield return new GoalSpaceEdge(planningTask, goal, effectiveAction);
                        }
                    }
                    else
                    {
                        yield return new GoalSpaceEdge(planningTask, goal, action);
                    }
                }
            }

            /// <inheritdoc />
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        private readonly struct GoalSpaceEdge : IEdge<GoalSpaceNode, GoalSpaceEdge>
        {
            private readonly PlanningTask planningTask;
            private readonly Goal fromGoal;

            public GoalSpaceEdge(PlanningTask planningTask, Goal fromGoal, Action action)
            {
                this.planningTask = planningTask;
                this.fromGoal = fromGoal;
                this.Action = action;
            }

            /// <inheritdoc />
            public GoalSpaceNode From => new(planningTask, fromGoal);

            /// <inheritdoc />
            public GoalSpaceNode To => new(planningTask, Action.Regress(fromGoal));

            /// <summary>
            /// Gets the action that is regressed over to achieve this goal transition.
            /// </summary>
            public Action Action { get; }

            /// <inheritdoc />
            public override string ToString() => new PlanFormatter(planningTask.Domain).Format(Action);
        }
    }
}
