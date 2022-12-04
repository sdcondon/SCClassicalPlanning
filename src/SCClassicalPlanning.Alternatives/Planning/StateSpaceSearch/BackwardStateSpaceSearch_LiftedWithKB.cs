﻿// Copyright 2022 Simon Condon
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

namespace SCClassicalPlanning.Planning.StateSpaceSearch
{
    /// <summary>
    /// A simple implementation of <see cref="IPlanner"/> that carries out a backward (A-star) search of
    /// the state space to create plans.
    /// <para/>
    /// See section 10.2.2 of "Artificial Intelligence: A Modern Approach" for more on this.
    /// </summary>
    public class BackwardStateSpaceSearch_LiftedWithKB : IPlanner
    {
        private readonly IHeuristic heuristic;
        private readonly InvariantInspector? invariantInspector;
        private readonly Func<Action, float> getActionCost;

        /// <summary>
        /// Initializes a new instance of the <see cref="BackwardStateSpaceSearch_LiftedWithKB"/> class that attempts to minimise the number of actions in the resulting plan.
        /// </summary>
        /// <param name="heuristic">The heuristic to use - the returned cost will be interpreted as the estimated number of actions that need to be performed.</param>
        public BackwardStateSpaceSearch_LiftedWithKB(IHeuristic heuristic, IKnowledgeBase? invariantsKB = null)
            : this(heuristic, a => 1f, invariantsKB)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BackwardStateSpaceSearch_LiftedWithKB"/> class that attempts to minimise the total "cost" of actions in the resulting plan.
        /// </summary>
        /// <param name="heuristic">The heuristic to use - with the returned cost will be interpreted as the estimated total cost of the actions that need to be performed.</param>
        /// <param name="getActionCost">A delegate to retrieve the cost of an action.</param>
        public BackwardStateSpaceSearch_LiftedWithKB(IHeuristic heuristic, Func<Action, float> getActionCost, IKnowledgeBase? invariantsKB = null)
        {
            this.heuristic = heuristic;
            this.getActionCost = getActionCost;
            this.invariantInspector = invariantsKB != null ? new InvariantInspector(invariantsKB) : null;
        }

        /// <summary>
        /// Creates a (concretely-typed) planning task to work on solving a given problem.
        /// </summary>
        /// <param name="problem">The problem to create a plan for.</param>
        /// <returns></returns>
        public PlanningTask CreatePlanningTask(Problem problem) => new(problem, heuristic, getActionCost, invariantInspector);

        /// <inheritdoc />
        IPlanningTask IPlanner.CreatePlanningTask(Problem problem) => CreatePlanningTask(problem);

        /// <summary>
        /// The implementation of <see cref="IPlanningTask"/> used by <see cref="BackwardStateSpaceSearch_LiftedWithKB"/>.
        /// </summary>
        public class PlanningTask : SteppablePlanningTask<(Goal, Action, Goal)>
        {
            private readonly AStarSearch<StateSpaceNode, StateSpaceEdge> search;

            private bool isComplete;
            private Plan? result;

            internal PlanningTask(Problem problem, IHeuristic heuristic, Func<Action, float> getActionCost, InvariantInspector? invariantInspector)
            {
                Domain = problem.Domain;
                InvariantInspector = invariantInspector;

                search = new AStarSearch<StateSpaceNode, StateSpaceEdge>(
                    source: new StateSpaceNode(this, problem.Goal),
                    isTarget: n => problem.InitialState.GetSatisfyingSubstitutions(n.Goal).Any(),
                    getEdgeCost: e => getActionCost(e.Action),
                    getEstimatedCostToTarget: n => heuristic.EstimateCost(problem.InitialState, n.Goal));

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

        private readonly struct StateSpaceNode : INode<StateSpaceNode, StateSpaceEdge>, IEquatable<StateSpaceNode>
        {
            private readonly PlanningTask planningTask;

            public StateSpaceNode(PlanningTask planningTask, Goal goal) => (this.planningTask, Goal) = (planningTask, goal);

            /// <summary>
            /// Gets the goal represented by this node.
            /// </summary>
            public Goal Goal { get; }

            /// <inheritdoc />
            public IReadOnlyCollection<StateSpaceEdge> Edges => new StateSpaceNodeEdges(planningTask, Goal);

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

        private readonly struct StateSpaceNodeEdges : IReadOnlyCollection<StateSpaceEdge>
        {
            private readonly PlanningTask planningTask;
            private readonly Goal goal;

            public StateSpaceNodeEdges(PlanningTask planningTask, Goal goal) => (this.planningTask, this.goal) = (planningTask, goal);

            /// <inheritdoc />
            public int Count => DomainInspector.GetRelevantActions(planningTask.Domain, goal).Count();

            /// <inheritdoc />
            public IEnumerator<StateSpaceEdge> GetEnumerator()
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
                            yield return new StateSpaceEdge(planningTask, goal, effectiveAction);
                        }
                    }
                    else
                    {
                        yield return new StateSpaceEdge(planningTask, goal, action);
                    }
                }
            }

            /// <inheritdoc />
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        private readonly struct StateSpaceEdge : IEdge<StateSpaceNode, StateSpaceEdge>
        {
            private readonly PlanningTask planningTask;
            private readonly Goal fromGoal;

            public StateSpaceEdge(PlanningTask planningTask, Goal fromGoal, Action action)
            {
                this.planningTask = planningTask;
                this.fromGoal = fromGoal;
                this.Action = action;
            }

            /// <inheritdoc />
            public StateSpaceNode From => new(planningTask, fromGoal);

            /// <inheritdoc />
            public StateSpaceNode To => new(planningTask, Action.Regress(fromGoal));

            /// <summary>
            /// Gets the action that is regressed over to achieve this goal transition.
            /// </summary>
            public Action Action { get; }

            /// <inheritdoc />
            public override string ToString() => new PlanFormatter(planningTask.Domain).Format(Action);
        }
    }
}