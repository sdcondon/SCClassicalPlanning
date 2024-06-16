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
using SCGraphTheory;

namespace SCClassicalPlanning.Planning.StateAndGoalSpace;

/// <summary>
/// Represents an edge in the state space of a planning problem.
/// </summary>
// NB: three ref-valued fields puts this on the verge of being too large for a struct (24 bytes on a 64-bit system).
// Probably worth comparing performance with a class-based graph at some point, but meh, it'll do for now.
public readonly struct StateSpaceEdge : IEdge<StateSpaceNode, StateSpaceEdge>
{
    /// <summary>
    /// The problem whose state space this edge is a member of.
    /// </summary>
    public readonly Problem Problem;

    /// <summary>
    /// The state represented by the node that this edge connects from.
    /// </summary>
    public readonly IState FromState;

    /// <summary>
    /// The action that this edge represents the application of.
    /// </summary>
    public readonly Action Action;

    /// <summary>
    /// Initialises a new instance of the <see cref="StateSpaceEdge"/> struct.
    /// </summary>
    /// <param name="problem">The problem whose state space this edge is a member of.</param>
    /// <param name="fromState">The state represented by the node that this edge connects from.</param>
    /// <param name="action">The action that this edge represents the application of.</param>
    public StateSpaceEdge(Problem problem, IState fromState, Action action)
    {
        Problem = problem;
        FromState = fromState;
        Action = action;
    }

    /// <inheritdoc />
    public StateSpaceNode From => new(Problem, FromState);

    /// <inheritdoc />
    public StateSpaceNode To => new(Problem, Action.ApplyTo(FromState));

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString() => new PlanFormatter(Problem).Format(Action);
}
