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
using SCClassicalPlanning.InternalUtilities;
using SCFirstOrderLogic;
using SCFirstOrderLogic.SentenceManipulation;
using SCFirstOrderLogic.SentenceManipulation.VariableManipulation;
using System.Collections.Immutable;

namespace SCClassicalPlanning;

/// <summary>
/// <para>
/// Container for information about a goal.
/// </para>
/// <para>
/// A <see cref="Goal"/> is essentially just a set of <see cref="Literal"/>s. The positive ones indicate predicates that must exist
/// in a <see cref="IState"/> for it to meet the goal. The negative ones indicate predicates that must NOT exist in a state for
/// it to meet the goal. Goals are used to describe the end goal of <see cref="Problem"/>s, as well as the precondition
/// of <see cref="Action"/>s.
/// </para>
/// </summary>
public class Goal
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Goal"/> class from an enumerable of the literals that comprise it.
    /// </summary>
    /// <param name="elements">The literals that comprise the goal.</param>
    public Goal(IEnumerable<Literal> elements) => Elements = elements.ToImmutableHashSet();

    /// <summary>
    /// Initializes a new instance of the <see cref="Goal"/> class from a (params) array of the literals that comprise it.
    /// </summary>
    /// <param name="elements">The literals that comprise the goal.</param>
    public Goal(params Literal[] elements) : this((IEnumerable<Literal>)elements) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Goal" /> class from a sentence of first order logic. The sentence must be a conjunction of literals, or an exception will be thrown.
    /// </summary>
    /// <param name="sentence">The sentence that expresses the goal.</param>
    public Goal(Sentence sentence) : this(ConstructionVisitor.Visit(sentence)) { }

    // NB: uses argument directly, unlike public ctors. This is to avoid unnecessary GC pressure.
    // Also allows the public ctors apply validation, without forcing said validation to occur at every step of a planning process.
    internal Goal(ImmutableHashSet<Literal> elements) => Elements = elements;

    /// <summary>
    /// Gets a singleton <see cref="Goal"/> instance that is empty.
    /// </summary>
    public static Goal Empty { get; } = new Goal();

    /// <summary>
    /// Gets the set of literals that comprise this goal.
    /// The positive ones indicate predicates that must exist in a <see cref="IState"/> in order for the goal to be met.
    /// The negative ones indicate predicates that must NOT exist in a <see cref="IState"/> in order for the goal to be met.
    /// </summary>
    // One could perhaps argue that we should store positive and negative elements separately, for performance.
    // After all, application and Regression are going to be far more common than wanting to get all elements.
    // However, this would be problematic if this is ever expanded to richer PDDL functionality
    // (or if we want to allow extension - unlikely given the motivator for the project, but..).
    // So leaving it like this for the time being at least.
    public ImmutableHashSet<Literal> Elements { get; }

    /// <summary>
    /// Gets the positive elements of the goal - those whose predicates must exist in a <see cref="IState"/> in order for this goal to be met.
    /// </summary>
    public IEnumerable<Literal> PositiveElements => Elements.Where(l => l.IsPositive);

    /// <summary>
    /// Gets the negative elements of the goal - those whose predicates must NOT exist in a <see cref="IState"/> in order for this goal to be met.
    /// </summary>
    public IEnumerable<Literal> NegativeElements => Elements.Where(l => l.IsNegated);

    /// <summary>
    /// Gets the required predicates of the goal - those that must exist in a <see cref="IState"/> in order for this goal to be met.
    /// </summary>
    public IEnumerable<Predicate> RequiredPredicates => PositiveElements.Select(l => l.Predicate);

    /// <summary>
    /// Gets the forbidden predicates of the goal - those that must NOT exist in a <see cref="IState"/> in order for this goal to be met.
    /// </summary>
    public IEnumerable<Predicate> ForbiddenPredicates => NegativeElements.Select(l => l.Predicate);

    /// <summary>
    /// Gets a value indicating whether this goal is met by a particular state.
    /// </summary>
    /// <param name="state">The state to check.</param>
    /// <returns>True if this goal is met by the given state; otherwise false.</returns>
    public bool IsMetBy(IState state) => state.Meets(this);

    /// <summary>
    /// Gets the variable substitutions that can be applied to this goal so that a given state meets it.
    /// </summary>
    /// <param name="state">The state to check.</param>
    /// <returns>An enumerable of variable substitutions that can be made to this goal to give a goal that the given state meets.</returns>
    public IEnumerable<VariableSubstitution> GetSubstitutionsToBeMetBy(IState state) => state.GetSubstitutionsToMeet(this);

    /// <summary>
    /// <para>
    /// Returns a value indicating whether a given effect is conceivably a useful final step in achieving this goal.
    /// </para>
    /// <para>
    /// Specifically, an effect is relevant to a goal if it accomplishes at least one element of the goal, and does not undo anything.
    /// That is, the effect's elements overlap with the goals elements, and the set comprised of the negation of each of the effect's elements does not.
    /// </para>
    /// </summary>
    /// <param name="effect">The effect to determine relevancy of.</param>
    /// <returns>A value indicating whether the given effect is relevant to this goal.</returns>
    public bool IsRelevant(Effect effect) => Elements.Overlaps(effect.Elements) && !Elements.Overlaps(effect.Elements.Select(l => l.Negate()));

    /// <summary>
    /// Returns the goal that must be met prior to performing a given action, in order to ensure that this goal is met after the action is performed. 
    /// </summary>
    /// <param name="action">The action to regress over.</param>
    /// <returns>The goal that must be met prior to performing the given action.</returns>
    public Goal Regress(Action action) => new(Elements.Except(action.Effect.Elements).Union(action.Precondition.Elements));

    /// <summary>
    /// <para>
    /// Determines whether the specified object is equal to the current object.
    /// </para>
    /// <para>
    /// Goals implement value semantics for equality - two Goals are equal if they share the same Elements.
    /// </para>
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
    public override bool Equals(object? obj)
    {
        // Goals should be small-ish, so I'm not too worried by the inefficiency here.
        // Otherwise could think about sorting the set of elements (e.g. using ImmutableSortedSet sorted by hash code), maybe?
        // Would need testing whether the benefit is outweighed by constructing the ordered set in first place.
        return obj is Goal goal && goal.Elements.SetEquals<Literal>(Elements);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hashCode = new HashCode();

        // As with Equals, asserting that this is okay given that goals should be small-ish.
        // Though worth noting we'd avoid it if using e.g. ImmutableSortedSet for the elements.
        foreach (var element in Elements.OrderBy(e => e.GetHashCode()))
        {
            hashCode.Add(element);
        }

        return hashCode.ToHashCode();
    }

    /// <inheritdoc />
    public override string ToString() => string.Join(" ∧ ", Elements.Select(a => a.ToString()));

    /// <summary>
    /// Sentence visitor class that extracts <see cref="Literal"/>s from a <see cref="Sentence"/> that is a conjunction of them.
    /// Used by the <see cref="Goal(Sentence)"/> constructor.
    /// </summary>
    private class ConstructionVisitor : RecursiveSentenceVisitor<HashSet<Literal>>
    {
        private static readonly ConstructionVisitor Instance = new();

        public static HashSet<Literal> Visit(Sentence sentence)
        {
            var elements = new HashSet<Literal>();
            Instance.Visit(sentence, elements);
            return elements;
        }

        /// <inheritdoc/>
        public override void Visit(Sentence sentence, HashSet<Literal> literals)
        {
            if (sentence is Conjunction conjunction)
            {
                // The sentence is assumed to be a conjunction of literals - so just skip past all the conjunctions at the root.
                base.Visit(conjunction, literals);
            }
            else
            {
                // Assume we've hit a literal - literal ctor will throw if its not.
                literals.Add(new Literal(sentence));
            }
        }
    }
}
