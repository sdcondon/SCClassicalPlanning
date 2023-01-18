// Copyright 2022-2023 Simon Condon
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
namespace SCClassicalPlanning.Planning.Search.CostStrategies
{
    /// <summary>
    /// Implementation of <see cref="ICostStrategy"/> that just invokes given delegates.
    /// </summary>
    public class DelegateCostStrategy : ICostStrategy
    {
        private readonly Func<Action, float> getCost;
        private readonly Func<State, Goal, float> estimateCost;

        /// <summary>
        /// Initialises a new instance of the <see cref="DelegateCostStrategy"/> class.
        /// </summary>
        /// <param name="getCost">The delegate to invoke to get the cost of an action.</param>
        /// <param name="estimateCost">The delegate to invoke to estimate the total cost of getting from a given state to a state that satisfies a given goal.</param>
        public DelegateCostStrategy(Func<Action, float> getCost, Func<State, Goal, float> estimateCost)
        {
            this.getCost = getCost;
            this.estimateCost = estimateCost;
        }

        /// <inheritdoc/>
        public float GetCost(Action action) => getCost(action);

        /// <inheritdoc/>
        public float EstimateCost(State state, Goal goal) => estimateCost(state, goal);
    }
}
