﻿// Copyright 2022-2023 Simon Condon
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
namespace SCClassicalPlanning.Planning.StateAndGoalSpace.CostStrategies;

/// <summary>
/// <para>
/// Very simplistic cost strategy that just (gives each action a cost of 1 and) counts the number of
/// unsatisfied goals to provide cost estimates. That is, it adds the number of positive elements
/// of the goal that do not occur in the state to the number of negative elements that do (that is,
/// it assumes that we need to carry out one action per element of the goal that isn't currently
/// satisfied).
/// </para>
/// <para>
/// This is generally a pretty terrible heuristic - as are all heuristics that don't take into account the details of the
/// problem being solved. Consider using something else. DEFINITELY use something else for goal space searches.
/// One of the things that this heuristic simply can't do is tell when preconditions are unsatisfiable - which is very bad 
/// news for a goal space search, because you rely on being able to do this to prune branches.
/// </para>
/// </summary>
public class UnsatisfiedGoalCount : ICostStrategy
{
    /// <inheritdoc/>
    public float GetCost(Action action) => 1f;

    /// <inheritdoc/>
    public float EstimateCost(State state, Goal goal)
    {
        return goal.RequiredPredicates.Except(state.Elements).Count()
            + goal.ForbiddenPredicates.Intersect(state.Elements).Count();
    }
}
