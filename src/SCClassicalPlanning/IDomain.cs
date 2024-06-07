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

namespace SCClassicalPlanning;

/// <summary>
/// <para>
/// Interface for containers of information about a domain.
/// </para>
/// <para>
/// A domain defines the aspects that are common to of all problems that occur within it.
/// Specifically, the <see cref="Action"/>s available within it.
/// </para>
/// </summary>
public interface IDomain
{
    /// <summary>
    /// Gets the set of actions that exist within the domain.
    /// </summary>
    public IQueryable<Action> Actions { get; }

    /// <summary>
    /// Gets the set of predicates that exist within the domain.
    /// </summary>
    public IQueryable<Predicate> Predicates { get; }

    /// <summary>
    /// Gets the set of constants that exist within the domain
    /// </summary>
    public IQueryable<Constant> Constants { get; }

    //// TODO-FEATURE: It is increasingly looking like adding the following would be useful.
    //// This could be used to represent both the :timeless and :axioms of PDDL, possibly 
    //// in addition to predicate typing (assuming we don't decide to do this via a richer
    //// predicate-wrapping object) - for example, MyPredicate(x, y) => IsOfMyType(x) & IsOfMyOtherType(y).
    //// public IQueryable<Sentence> Invariants { get; }
    //// though actually just public IKnowledgeBase Invariants { get; } might be a better call
    //// (again because of potential IO - clauses might/probably will need to be in storage,
    //// and SCFoL doesn't have a universal clause store - its specific to KB type.. Of course,
    //// issue disappears if we allow for IQueryable<CNFClause> clause stores in SCFoL)
}
