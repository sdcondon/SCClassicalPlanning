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
using SCFirstOrderLogic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace SCClassicalPlanning.Planning.GraphPlan
{
    /// <summary>
    /// <para>
    /// Representation of a proposition node in a planning graph.
    /// </para>
    /// <para>
    /// NB: We don't make use of the SCGraphTheory abstraction for the planning graph because none of the algorithms that use 
    /// it query it via graph theoretical algorithms - so it would be needless complexity.
    /// </para>
    /// </summary>
    [DebuggerDisplay("{Proposition}")]
    public class PlanningGraphPropositionNode
    {
        internal PlanningGraphPropositionNode(Literal proposition) => Proposition = proposition;

        /// <summary>
        /// Gets the proposition represented by this node.
        /// </summary>
        public Literal Proposition { get; }

        // TODO-V1: make public but read-only (might need builder or similar during construction)
        internal Collection<PlanningGraphActionNode> Actions { get; } = new();

        internal Collection<PlanningGraphActionNode> Causes { get; } = new();

        internal Collection<PlanningGraphPropositionNode> Mutexes { get; } = new();
    }
}
