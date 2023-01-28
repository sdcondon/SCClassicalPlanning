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
using System.Diagnostics;

namespace SCClassicalPlanning.Planning.GraphPlan
{
    /// <summary>
    /// <para>
    /// Representation of an action node in a planning graph.
    /// </para>
    /// <para>
    /// NB: We don't make use of SCGraphTheory abstraction for the planning graph because none of the algorithms that use 
    /// it query it via graph theoretical algorithms - so it would be needless complexity.
    /// </para>
    /// </summary>
    [DebuggerDisplay("{Action.Identifier}: {Action.Effect}")]
    public class PlanningGraphActionNode
    {
        internal PlanningGraphActionNode(Action action) => Action = action;

        /// <summary>
        /// Gets the action represented by this node.
        /// </summary>
        public Action Action { get; }

        // TODO-V1: make public but read-only (might need builder or similar during construction)
        internal Collection<PlanningGraphPropositionNode> Effects { get; } = new();

        internal Collection<PlanningGraphPropositionNode> Preconditions { get; } = new();

        internal Collection<PlanningGraphActionNode> Mutexes { get; } = new();

        /// <summary>
        /// Returns a value indicating whether this node indicates mutual exclusion with any of an enumerable of others.
        /// </summary>
        /// <param name="nodes">The other action nodes to examine.</param>
        /// <returns><see langword="true"/> if this node has a mutex link with any of the given others; otherwise <see langword="false"/>.</returns>
        public bool IsMutexWithAny(IEnumerable<PlanningGraphActionNode> nodes)
        {
            foreach (var node in nodes)
            {
                if (Mutexes.Any(m => m.Action.Equals(node.Action)))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
