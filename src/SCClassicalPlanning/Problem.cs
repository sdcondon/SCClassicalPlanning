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
using SCClassicalPlanning.ProblemManipulation;
using SCFirstOrderLogic;
using System.Collections.Immutable;

namespace SCClassicalPlanning;

/// <summary>
/// <para>
/// Encapsulates a planning problem.
/// </para>
/// <para>
/// Problems exist within a <see cref="IDomain"/>, and consist of an initial <see cref="IState"/>,
/// an end <see cref="SCClassicalPlanning.Goal"/>, and a set of domain elements (represented by <see cref="Constant"/>s
/// from the SCFirstOrderLogic library) that exist within the scope of the problem.
/// </para>
/// </summary>
public class Problem
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Problem"/> class.
    /// </summary>
    /// <param name="domain">The domain in which this problem resides.</param>
    /// <param name="initialState">The initial state of the problem.</param>
    /// <param name="goal">The goal of the problem.</param>
    public Problem(IDomain domain, IState initialState, Goal goal)
    {
        Domain = domain;
        InitialState = initialState;
        Goal = goal;
    }

    /// <summary>
    /// Gets the domain in which this problem resides.
    /// </summary>
    public IDomain Domain { get; }

    /// <summary>
    /// Gets the initial state of the problem.
    /// </summary>
    public IState InitialState { get; }

    /// <summary>
    /// Gets the goal of the problem.
    /// </summary>
    public Goal Goal { get; }

    //// TODO-FEATURE: It is increasingly looking like adding the following would be useful.
    //// The Domain's Invariants are relevant of course, but you also have things like certain
    //// predicates that never change as a result of actions (e.g. "typing" predicates) - so the subset
    //// of initial state that refers to these predicates is invariant. E.g. IsOfMyType(MyObject).
    //// And of course predicates that change "together" in certain ways.
    //// Having said this, I worry a little about SoC..
    //// public ImmutableHashSet<Sentence> Invariants { get; }
    //// or (to ease duplication concerns):
    //// public ImmutableHashSet<CNFClause> Invariants { get; }
}
