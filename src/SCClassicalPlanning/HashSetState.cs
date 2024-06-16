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
using SCClassicalPlanning.ProblemManipulation;
using SCFirstOrderLogic;
using SCFirstOrderLogic.SentenceManipulation;
using SCFirstOrderLogic.SentenceManipulation.Unification;
using System.Collections.Immutable;

namespace SCClassicalPlanning;

/// <summary>
/// An implementation of <see cref="IState"/> that just stores all of its predicates in a hash set.
/// </summary>
public class HashSetState : IState
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HashSetState"/> class from an enumerable of the predicates that comprise it.
    /// </summary>
    /// <param name="elements">The predicates that comprise the state.</param>
    public HashSetState(IEnumerable<Predicate> elements)
    {
        if (elements.SelectMany(e => e.Arguments).Any(a => !a.IsGroundTerm))
        {
            throw new ArgumentException("States cannot include non-ground terms", nameof(elements));
        }

        Elements = elements.ToImmutableHashSet();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HashSetState"/> class from a (params) array of the predicates that comprise it.
    /// </summary>
    /// <param name="elements">The predicates that comprise the state.</param>
    public HashSetState(params Predicate[] elements) : this((IEnumerable<Predicate>)elements) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="HashSetState"/> class from a sentence of first order logic.
    /// The sentence must be a conjunction of predicates, or an exception will be thrown.
    /// </summary>
    /// <param name="sentence">The sentence that expresses the state.</param>
    public HashSetState(Sentence sentence) : this(ConstructionVisitor.Visit(sentence)) { }

    // NB: uses argument directly, unlike public ctors. This is to avoid unnecessary GC pressure.
    // Also allows the public ctors apply validation, without forcing said validation to occur at every step of a planning process.
    internal HashSetState(ImmutableHashSet<Predicate> elements) => Elements = elements;

    /// <summary>
    /// Gets a singleton <see cref="HashSetState"/> instance that is empty.
    /// </summary>
    public static HashSetState Empty { get; } = new HashSetState();

    /// <summary>
    /// Gets the set of predicates that comprise this state.
    /// </summary>
    IQueryable<Predicate> IState.Elements => Elements.AsQueryable();

    /// <summary>
    /// Gets the set of predicates that comprise this state.
    /// </summary>
    public ImmutableHashSet<Predicate> Elements { get; }

    // TODO-PERFORMANCE: at some point look at (test me!) efficiency here. ImmutableHashSet builder stuff might be of use?
    /// <inheritdoc />
    IState IState.Apply(Effect effect) => new HashSetState(Elements.Except(effect.DeleteList).Union(effect.AddList));

    /// <summary>
    /// Applies a given <see cref="Effect"/> to the state, producing a new state.
    /// </summary>
    /// <param name="effect">The effect to apply.</param>
    /// <returns>The new state.</returns>
    public HashSetState Apply(Effect effect) => new(Elements.Except(effect.DeleteList).Union(effect.AddList));

    /// <inheritdoc />
    public bool Satisfies(Goal goal) => Elements.IsSupersetOf(goal.RequiredPredicates) && !Elements.Overlaps(goal.ForbiddenPredicates);

    /// <inheritdoc />
    public IEnumerable<VariableSubstitution> GetSatisfyingSubstitutions(Goal goal)
    {
        bool UnifiesNegativeGoalElement(VariableSubstitution substitution)
        {
            List<Literal> constraintElements = new();

            foreach (var goalElement in goal.ForbiddenPredicates)
            {
                if (Elements.Contains(substitution.ApplyTo(goalElement)))
                {
                    return true;
                }
            }

            return false;
        }

        IEnumerable<VariableSubstitution> GetPositiveGoalElementUnifiers(IEnumerable<Predicate> positiveGoalPredicates, VariableSubstitution substitution)
        {
            if (!positiveGoalPredicates.Any())
            {
                yield return substitution;
            }
            else
            {
                var firstGoalElement = positiveGoalPredicates.First();

                // Here we iterate through all elements of the state, trying to find unifications with the first goal element.
                foreach (var stateElement in Elements)
                {
                    if (Unifier.TryUpdate(firstGoalElement, stateElement, substitution, out var firstGoalElementUnifier))
                    {
                        foreach (var restOfGoalElementsUnifier in GetPositiveGoalElementUnifiers(positiveGoalPredicates.Skip(1), firstGoalElementUnifier))
                        {
                            yield return restOfGoalElementsUnifier;
                        }
                    }
                }
            }
        }

        foreach (var substitution in GetPositiveGoalElementUnifiers(goal.RequiredPredicates, new VariableSubstitution()).Distinct())
        {
            if (!UnifiesNegativeGoalElement(substitution))
            {
                yield return substitution;
            }
        }
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        // Obviously unworkable if the state is large. However, if the state is large then this isn't
        // the only problematic thing. So let's revisit this when we look at abstracting state to allow 
        // for secondary storage and indexing. It may be that for the "in-mem" version, this is fine..
        return obj is HashSetState state && state.Elements.SetEquals<Predicate>(Elements);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        
        // Again, terrible for large states.
        foreach (var element in Elements.OrderBy(e => e.GetHashCode()))
        {
            hashCode.Add(element);
        }

        return hashCode.ToHashCode();
    }

    /// <inheritdoc />
    public override string ToString() => string.Join(" ∧ ", Elements.Select(a => a.ToString()));

    /// <summary>
    /// Sentence visitor class that extracts <see cref="Predicate"/>s from a <see cref="Sentence"/> that is a conjunction of them.
    /// Used by the <see cref="HashSetState(Sentence)"/> constructor.
    /// </summary>
    private class ConstructionVisitor : RecursiveSentenceVisitor<HashSet<Predicate>>
    {
        private static readonly ConstructionVisitor Instance = new();

        public static HashSet<Predicate> Visit(Sentence sentence)
        {
            var elements = new HashSet<Predicate>();
            Instance.Visit(sentence, elements);
            return elements;
        }

        public override void Visit(Sentence sentence, HashSet<Predicate> predicates)
        {
            if (sentence is Conjunction conjunction)
            {
                base.Visit(conjunction, predicates);
            }
            else if (sentence is Predicate predicate)
            {
                predicates.Add(predicate);
            }
            else
            {
                throw new ArgumentException("States must be a conjunction of predicates");
            }
        }
    }


    /// <summary>
    /// Utility class to find <see cref="Constant"/> instances within the elements of a <see cref="IState"/>, and add them to a given <see cref="HashSet{T}"/>.
    /// </summary>
    private class StateConstantFinder : RecursiveStateVisitor<HashSet<Constant>>
    {
        /// <summary>
        /// Gets a singleton instance of the <see cref="StateConstantFinder"/> class.
        /// </summary>
        public static StateConstantFinder Instance { get; } = new();

        /// <inheritdoc/>
        public override void Visit(Constant constant, HashSet<Constant> constants) => constants.Add(constant);
    }

    /// <summary>
    /// Utility class to find <see cref="Constant"/> instances within the elements of a <see cref="SCClassicalPlanning.Goal"/>, and add them to a given <see cref="HashSet{T}"/>.
    /// </summary>
    private class GoalConstantFinder : RecursiveGoalVisitor<HashSet<Constant>>
    {
        /// <summary>
        /// Gets a singleton instance of the <see cref="GoalConstantFinder"/> class.
        /// </summary>
        public static GoalConstantFinder Instance { get; } = new();

        /// <inheritdoc/>
        public override void Visit(Constant constant, HashSet<Constant> constants) => constants.Add(constant);
    }
}
