// Copyright 2022-2024 Simon Condon
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
using SCGraphTheory;
using SCGraphTheory.Search.Classic;
using System.Collections;

namespace SCClassicalPlanning.Planning.StateAndGoalSpace;

/// <summary>
/// <para>
/// A simple implementation of <see cref="IPlanner"/> that carries out an A-star search of
/// the goal space to create plans.
/// </para>
/// <para>
/// Differs from the library version in that it is completely propositional - variables are expanded
/// out to every possible value whenever they occur. This is obviously suboptimal from a performance perspective.
/// </para>
/// </summary>
public class GoalSpaceAStarPlanner_PropositionalWithoutKB : IPlanner
{
    private readonly ICostStrategy costStrategy;

    /// <summary>
    /// Initializes a new instance of the <see cref="GoalSpaceAStarPlanner_PropositionalWithoutKB"/> class.
    /// </summary>
    /// <param name="costStrategy">The cost strategy to use.</param>
    public GoalSpaceAStarPlanner_PropositionalWithoutKB(ICostStrategy costStrategy) => this.costStrategy = costStrategy;

    /// <summary>
    /// Creates a (concretely-typed) planning task to work on solving a given problem.
    /// </summary>
    /// <param name="problem">The problem to create a plan for.</param>
    /// <returns></returns>
    public Task<PlanningTask> CreatePlanningTaskAsync(Problem problem) => Task.FromResult(new PlanningTask(problem, costStrategy));

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

        internal PlanningTask(Problem problem, ICostStrategy costStrategy)
        {
            search = new AStarSearch<GoalSpaceNode, GoalSpaceEdge>(
                source: new GoalSpaceNode(problem, problem.EndGoal),
                isTarget: n => n.Goal.IsMetBy(problem.InitialState),
                getEdgeCost: e => costStrategy.GetCost(e.Action),
                getEstimatedCostToTarget: n => costStrategy.EstimateCost(problem.InitialState, n.Goal));

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
        private readonly Problem problem;
        private readonly Goal goal;

        public GoalSpaceNodeEdges(Problem problem, Goal goal) => (this.problem, this.goal) = (problem, goal);

        /// <inheritdoc />
        public int Count => ProblemInspector.GetRelevantGroundActions(goal, problem.ActionSchemas, problem.InitialState.GetAllConstants()).Count();

        /// <inheritdoc />
        public IEnumerator<GoalSpaceEdge> GetEnumerator()
        {
            foreach (var action in ProblemInspector.GetRelevantGroundActions(goal, problem.ActionSchemas, problem.InitialState.GetAllConstants()))
            {
                yield return new GoalSpaceEdge(problem, goal, action);
            }
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public readonly struct GoalSpaceEdge : IEdge<GoalSpaceNode, GoalSpaceEdge>
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
        public override string ToString() => new PlanFormatter(problem).Format(Action);
    }
}
