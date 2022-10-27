// Copyright 2022 Simon Condon
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
using SCFirstOrderLogic.SentenceManipulation;
using SCFirstOrderLogic.SentenceManipulation.Unification;
using System.Collections.Immutable;

namespace SCClassicalPlanning
{
    /// <summary>
    /// Container for information about a state.
    /// <para/>
    /// A state is essentially just a set of ground (i.e. variable-free), functionless <see cref="Predicate"/>s. State instances occur as the initial state of <see cref="Problem"/>
    /// instances - and are also used by some planning algorithms to track intermediate states while looking for a solution to a problem.
    /// <para/>
    /// TODO: probably should add some verification in ctor that all elements are ground and functionless. Or.. not - don't want to sap performane by validating on
    /// every step in plan creation.. Best of both worlds would be nice.
    /// </summary>
    public class State
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="State"/> class from an enumerable of the predicates that comprise it.
        /// </summary>
        /// <param name="elements">The predicates that comprise the state.</param>
        public State(IEnumerable<Predicate> elements) => Elements = elements.ToImmutableHashSet();

        /// <summary>
        /// Initializes a new instance of the <see cref="State"/> class from a (params) array of the predicates that comprise it.
        /// </summary>
        /// <param name="elements">The predicates that comprise the state.</param>
        public State(params Predicate[] elements) : this((IEnumerable<Predicate>)elements) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="State"/> class from a sentence of first order logic.
        /// The sentence must be a conjunction of predicates, or an exception will be thrown.
        /// </summary>
        /// <param name="sentence">The sentence that expresses the state.</param>
        public State(Sentence sentence) : this(ConstructionVisitor.Visit(sentence)) { }

        /// <summary>
        /// Gets a singleton <see cref="State"/> instance that is empty.
        /// </summary>
        public static State Empty { get; } = new State();

        /// <summary>
        /// Gets the set of predicates that comprise this state.
        /// </summary>
        public ImmutableHashSet<Predicate> Elements { get; }

        /// <summary>
        /// Applies a given <see cref="Effect"/> to the state, producing a new state.
        /// </summary>
        /// <param name="effect">The effect to apply.</param>
        /// <returns>The new state.</returns>
        // TODO: at some point look at (test me!) efficiency here. ImmutableHashSet builder stuff might be of use?
        public State Apply(Effect effect) => new State(Elements.Except(effect.DeleteList).Union(effect.AddList));

        /// <summary>
        /// Gets a value indicating whether this state satisfies a given goal.
        /// <para/>
        /// TODO: assumes ground goal. Depending on performance diff, change or add alternative that doesn't.
        /// <para/>
        /// A goal is satisfied by a state if all of its positive elements and none of its negative elements are present in the state.
        /// </summary>
        /// <param name="goal">The goal to check.</param>
        /// <returns>A value indicating whether this state satisfies a given goal.</returns>
        public bool Satisfies(Goal goal) => Elements.IsSupersetOf(goal.PositivePredicates) && !Elements.Overlaps(goal.NegativePredicates);

        /// <summary>
        /// Gets the substitutions (if any) that can be applied to this state to satisfy the given goal.
        /// </summary>
        /// <param name="goal">The goal to check.</param>
        /// <returns>An enumerable of substitutions that satisfy the goal.</returns>
        public IEnumerable<VariableSubstitution> GetSatisfyingSubstitutions(Goal goal)
        {
            bool UnifiesNegativeGoalElement(VariableSubstitution substitution)
            {
                List<Literal> constraintElements = new();

                foreach (var goalElement in goal.NegativePredicates)
                {
                    if (Elements.Contains((Predicate)substitution.ApplyTo(goalElement).ToSentence()))
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
                        var firstGoalElementUnifier = new VariableSubstitution(substitution);

                        if (LiteralUnifier.TryUpdateUnsafe(firstGoalElement, stateElement, firstGoalElementUnifier))
                        {
                            foreach (var restOfGoalElementsUnifier in GetPositiveGoalElementUnifiers(positiveGoalPredicates.Skip(1), firstGoalElementUnifier))
                            {
                                yield return restOfGoalElementsUnifier;
                            }
                        }
                    }
                }
            }

            foreach (var substitution in GetPositiveGoalElementUnifiers(goal.PositivePredicates, new VariableSubstitution()).Distinct())
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
            return obj is State state && state.Elements.IsSubsetOf(Elements) && Elements.IsSubsetOf(state.Elements);
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
        /// Used by the <see cref="State(Sentence)"/> constructor.
        /// </summary>
        private class ConstructionVisitor : RecursiveSentenceVisitor<HashSet<Predicate>>
        {
            private static readonly ConstructionVisitor Instance = new ConstructionVisitor();

            public static HashSet<Predicate> Visit(Sentence sentence)
            {
                var elements = new HashSet<Predicate>();
                Instance.Visit(sentence, elements);
                return elements;
            }

            /// <inheritdoc/>
            public override void Visit(Sentence sentence, HashSet<Predicate> predicates)
            {
                if (sentence is Conjunction conjunction)
                {
                    base.Visit(conjunction, predicates);
                }
                else if (sentence is Predicate predicate)
                {
                    if (predicate.Arguments.Any(a => !a.IsGroundTerm))
                    {
                        throw new ArgumentException("States cannot include non-ground terms");
                    }

                    predicates.Add(predicate);
                }
                else
                {
                    throw new ArgumentException("States must be a conjunction of predicates");
                }
            }
        }
    }
}
