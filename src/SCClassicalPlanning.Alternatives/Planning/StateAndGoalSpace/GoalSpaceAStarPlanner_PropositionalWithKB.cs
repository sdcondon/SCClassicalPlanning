﻿// Copyright 2022-2024 Simon Condon
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
using SCClassicalPlanning.ProblemManipulation;
using SCFirstOrderLogic.Inference;
using SCGraphTheory;
using SCGraphTheory.Search.Classic;
using System.Collections;

namespace SCClassicalPlanning.Planning.StateAndGoalSpace;

/// <summary>
/// A simple implementation of <see cref="IPlanner"/> that carries out an A-star search of
/// the goal space to create plans.
/// </summary>
public class GoalSpaceAStarPlanner_PropositionalWithKB : IPlanner
{
    private readonly ICostStrategy costStrategy;
    private readonly InvariantInspector? invariantInspector;

    /// <summary>
    /// Initializes a new instance of the <see cref="GoalSpaceAStarPlanner_PropositionalWithKB"/> class.
    /// </summary>
    /// <param name="costStrategy">The strategy to use.</param>
    public GoalSpaceAStarPlanner_PropositionalWithKB(ICostStrategy costStrategy, IKnowledgeBase? invariantsKB = null)
    {
        this.costStrategy = costStrategy;
        this.invariantInspector = invariantsKB != null ? new InvariantInspector(invariantsKB) : null;
    }

    /// <summary>
    /// Creates a (concretely-typed) planning task to work on solving a given problem.
    /// </summary>
    /// <param name="problem">The problem to create a plan for.</param>
    /// <returns></returns>
    public Task<PlanningTask> CreatePlanningTaskAsync(Problem problem) => Task.FromResult(new PlanningTask(problem, costStrategy, invariantInspector));

    /// <inheritdoc />
    async Task<IPlanningTask> IPlanner.CreatePlanningTaskAsync(Problem problem) => await CreatePlanningTaskAsync(problem);

    /// <summary>
    /// The implementation of <see cref="IPlanningTask"/> used by <see cref="GoalSpaceAStarPlanner_PropositionalWithKB"/>.
    /// </summary>
    public class PlanningTask : SteppablePlanningTask<(Goal, Action, Goal)>
    {
        private readonly AStarSearch<GoalSpaceNode, GoalSpaceEdge> search;

        private bool isComplete;
        private Plan? result;

        internal PlanningTask(Problem problem, ICostStrategy costStrategy, InvariantInspector? invariantInspector)
        {
            Problem = problem;
            InvariantInspector = invariantInspector;

            search = new AStarSearch<GoalSpaceNode, GoalSpaceEdge>(
                source: new GoalSpaceNode(this, problem.EndGoal),
                isTarget: n => n.Goal.IsMetBy(problem.InitialState),
                getEdgeCost: e => costStrategy.GetCost(e.Action),
                getEstimatedCostToTarget: n => costStrategy.EstimateCost(problem.InitialState, n.Goal));

            CheckForSearchCompletion();
        }

        public Problem Problem { get; }

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
        public override Task<(Goal, Action, Goal)> NextStepAsync(CancellationToken cancellationToken = default)
        {
            if (IsComplete)
            {
                throw new InvalidOperationException("Task is complete");
            }

            var edge = search.NextStep();
            CheckForSearchCompletion();

            return Task.FromResult((edge.From.Goal, edge.Action, edge.To.Goal));
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

    public readonly struct GoalSpaceNode : INode<GoalSpaceNode, GoalSpaceEdge>, IEquatable<GoalSpaceNode>
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
        // NB: we don't compare the problem, since in expected usage (i.e. searching a particular
        // state space) it'll always match, so would be a needless drag on performance.
        public bool Equals(GoalSpaceNode node) => Equals(Goal, node.Goal);

        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(Goal);

        /// <inheritdoc />
        public override string ToString() => Goal.ToString();
    }

    public readonly struct GoalSpaceNodeEdges : IReadOnlyCollection<GoalSpaceEdge>
    {
        private readonly PlanningTask planningTask;
        private readonly Goal goal;

        public GoalSpaceNodeEdges(PlanningTask planningTask, Goal goal) => (this.planningTask, this.goal) = (planningTask, goal);

        /// <inheritdoc />
        public int Count => ProblemInspector.GetRelevantGroundActions(goal, planningTask.Problem.ActionSchemas, planningTask.Problem.InitialState.GetAllConstants()).Count();

        /// <inheritdoc />
        public IEnumerator<GoalSpaceEdge> GetEnumerator()
        {
            if (planningTask.InvariantInspector != null)
            {
                foreach (var action in ProblemInspector.GetRelevantGroundActions(goal, planningTask.Problem.ActionSchemas, planningTask.Problem.InitialState.GetAllConstants()))
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
            }
            else
            {
                foreach (var action in ProblemInspector.GetRelevantGroundActions(goal, planningTask.Problem.ActionSchemas, planningTask.Problem.InitialState.GetAllConstants()))
                {
                    yield return new GoalSpaceEdge(planningTask, goal, action);
                }
            }
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public readonly struct GoalSpaceEdge : IEdge<GoalSpaceNode, GoalSpaceEdge>
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
        public override string ToString() => new PlanFormatter(planningTask.Problem).Format(Action);
    }
}
