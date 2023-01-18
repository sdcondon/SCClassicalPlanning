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
using System.Collections.ObjectModel;

namespace SCClassicalPlanning.Planning
{
    /// <summary>
    /// Interface for types encapsulating a plan of action.
    /// </summary>
    public class Plan
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Plan"/> class.
        /// </summary>
        /// <param name="steps">The actions that comprise the plan.</param>
        public Plan(IList<Action> steps) => Steps = new ReadOnlyCollection<Action>(steps);

        /// <summary>
        /// Gets the steps of the plan.
        /// </summary>
        public IReadOnlyCollection<Action> Steps { get; }

        /// <summary>
        /// Applies this plan to a given state.
        /// </summary>
        public State ApplyTo(State state)
        {
            foreach (var action in Steps)
            {
                if (!action.IsApplicableTo(state))
                {
                    throw new ArgumentException("Invalid plan of action - current action is not applicable in the current state");
                }

                state = action.ApplyTo(state);
            }

            return state;
        }
    }
}
