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
namespace SCClassicalPlanning.Planning.Search.Heuristics
{
    /// <summary>
    /// Heuristic implementation that just invokes a given delegate.
    /// </summary>
    public class DelegateHeuristic : IHeuristic
    {
        private readonly Func<State, Goal, float> estimateCost;

        /// <summary>
        /// Initialises a new instance of the <see cref="DelegateHeuristic"/> class.
        /// </summary>
        /// <param name="estimateCost">The delegate to invoke.</param>
        public DelegateHeuristic(Func<State, Goal, float> estimateCost) => this.estimateCost = estimateCost;

        /// <inheritdoc/>
        public float EstimateCost(State state, Goal goal) => estimateCost(state, goal);
    }
}