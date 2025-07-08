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
using System.Collections.Immutable;

namespace SCClassicalPlanning;

/// <summary>
/// <para>
/// Container for information about an effect - some change in the state of a system.
/// </para>
/// <para>
/// <see cref="Effect"/>s are essentially a set of <see cref="Literal"/>s. The positive ones indicate predicates that are added to a
/// state by the effect's application. The negative ones indicate predicates that are removed. Effects are applied as the result of <see cref="Action"/>s.
/// </para>
/// </summary>
public class Effect
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Effect"/> class from an enumerable of the literals that comprise it.
    /// </summary>
    /// <param name="elements">The literals that comprise the effect.</param>
    public Effect(IEnumerable<Literal> elements) => Elements = elements.ToImmutableHashSet();

    /// <summary>
    /// Initializes a new instance of the <see cref="Effect"/> class from a (params) array of the literals that comprise it.
    /// </summary>
    /// <param name="elements">The literals that comprise the effect.</param>
    public Effect(params Literal[] elements) : this((IEnumerable<Literal>)elements) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Effect" /> class from a sentence of first order logic. The sentence must be a conjunction of literals, or an exception will be thrown.
    /// </summary>
    /// <param name="sentence">The sentence that expresses the effect.</param>
    public Effect(Sentence sentence) : this(ConstructionVisitor.Visit(sentence)) { }

    // NB: uses argument directly, unlike public ctors. This is to avoid unnecessary GC pressure.
    internal Effect(ImmutableHashSet<Literal> elements) => Elements = elements;

    /// <summary>
    /// Gets a singleton <see cref="Effect"/> instance that is empty.
    /// </summary>
    public static Effect Empty { get; } = new Effect();

    /// <summary>
    /// Gets the set of literals that comprise this effect.
    /// </summary>
    // TODO-PERFORMANCE: Should perhaps store add and delete lists separately, for performance.
    // Application and Regression are going to be far more common than wanting to get all elements.
    // Then again, that would be problematic if this is ever expanded to richer PDDL functionality
    // (or if we want to allow extension.. - unlikely given the motivator for the project, but..).
    // In any case, add some benchmarks before making any changes. Worth bearing in mind that effects
    // will probably be relatively small in most cases?
    public ImmutableHashSet<Literal> Elements { get; }

    /// <summary>
    /// Gets the "add list" of the effect - the non-negated predicates within the <see cref="Elements"/> set.
    /// These are added to a <see cref="IState"/> when this effect is applied.
    /// </summary>
    public IEnumerable<Predicate> AddList => Elements.Where(a => !a.IsNegated).Select(l => l.Predicate);

    /// <summary>
    /// Gets the "delete list" of the effect - the negated predicates within the <see cref="Elements"/> set.
    /// These are removed from a <see cref="IState"/> when this effect is applied.
    /// </summary>
    public IEnumerable<Predicate> DeleteList => Elements.Where(a => a.IsNegated).Select(l => l.Predicate);

    /// <summary>
    /// <para>
    /// Returns a value indicating whether this effect is conceivably a useful final step in achieving a given goal.
    /// </para>
    /// <para>
    /// An effect is relevant to a goal if it accomplishes at least one element of the goal, and does not undo anything.
    /// That is, if the effect's elements overlap with the goals elements, but the set of the negation of each of the effect's elements does not.
    /// </para>
    /// </summary>
    /// <param name="goal">The goal to determine relevancy to.</param>
    /// <returns>A value indicating whether this effect is relevant to a given goal.</returns>
    public bool IsRelevantTo(Goal goal) => goal.IsRelevant(this);

    /// <summary>
    /// Applies this action to a given state, producing a new state.
    /// </summary>
    /// <param name="state">The state to apply the effect to.</param>
    /// <returns>The new state.</returns>
    public IState ApplyTo(IState state) => state.Apply(this);

    /// <summary>
    /// <para>
    /// Determines whether the specified object is equal to the current object.
    /// </para>
    /// <para>
    /// Effects implement value semantics for equality - two Effects are equal if they share the same Elements.
    /// </para>
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
    public override bool Equals(object? obj)
    {
        return obj is Effect effect && effect.Elements.SetEquals<Literal>(Elements);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hashCode = new HashCode();

        // As with Equals, I'm asserting that this is okay given that effects should be small-ish.
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
    /// Used by the <see cref="Effect(Sentence)"/> constructor.
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
                // Assume we've hit a literal. NB: ctor will throw if its not actually a literal.
                // Afterwards, we don't need to look any further down the tree for the purposes of this class (though the Literal ctor that
                // we invoke here does so to figure out the details of the literal). So we can just return rather than invoking base.Visit.
                literals.Add(new Literal(sentence));
            }
        }
    }
}
