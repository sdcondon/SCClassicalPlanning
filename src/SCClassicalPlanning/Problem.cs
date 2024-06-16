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
namespace SCClassicalPlanning;

/// <summary>
/// <para>
/// Encapsulates a planning problem.
/// </para>
/// <para>
/// Problems consist of an initial <see cref="IState"/>, an end <see cref="Goal"/>, and a set of allowed <see cref="Action"/>s.
/// </para>
/// </summary>
public class Problem
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Problem"/> class.
    /// </summary>
    /// <param name="initialState">The initial state of the problem.</param>
    /// <param name="endGoal">The end goal of the problem.</param>
    /// <param name="actions">The actions that are available within the problem.</param>
    public Problem(IState initialState, Goal endGoal, IQueryable<Action> actions)
    {
        InitialState = initialState;
        EndGoal = endGoal;
        ActionSchemas = actions;
    }

    /// <summary>
    /// Gets the initial state of the problem.
    /// </summary>
    public IState InitialState { get; }

    /// <summary>
    /// Gets the end goal of the problem.
    /// </summary>
    public Goal EndGoal { get; }

    /// <summary>
    /// Gets the actions that are available.
    /// </summary>
    public IQueryable<Action> ActionSchemas { get; }

    //// TODO-FEATURE: It is increasingly looking like adding the following would be useful.
    //// This could be used to represent both the :timeless and :axioms of PDDL, possibly 
    //// in addition to predicate typing (assuming we don't decide to do this via a richer
    //// predicate-wrapping object) - for example, MyPredicate(x, y) => IsOfMyType(x) & IsOfMyOtherType(y).
    //// You also have things like certain predicates that never change as a result of actions (e.g. "typing"
    //// predicates) - so the subset of initial state that refers to these predicates is invariant.
    //// E.g. IsOfMyType(MyObject). And of course predicates that change "together" in certain ways.
    //// Having said this, I worry a little about SoC..
    //// public ImmutableHashSet<Sentence> Invariants { get; }
    //// or (to ease duplication concerns):
    //// public ImmutableHashSet<CNFClause> Invariants { get; }
    //// though actually just public IKnowledgeBase Invariants { get; } might be a better call
    //// (again because of potential IO - clauses might/probably will need to be in storage,
    //// and SCFoL doesn't have a universal clause store - its specific to KB type.. Of course,
    //// issue disappears if we allow for IQueryable<CNFClause> clause stores in SCFoL)
}
