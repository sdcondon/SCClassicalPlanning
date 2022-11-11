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
using System.Collections.Immutable;

namespace SCClassicalPlanning
{
    /// <summary>
    /// Container for information about a goal.
    /// <para/>
    /// A <see cref="Goal"/> is essentially just a set of <see cref="Literal"/>s. The positive ones indicate predicates that must exist in a <see cref="State"/> for it 
    /// to satisfy the goal. The negative ones indicate predicates that must NOT exist in a state for it to satisfy the goal. Goals are used to describe the end goal of <see cref="Problem"/>
    /// instances, as well as the precondition of <see cref="Action"/> instances.
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

        // NB: uses argument directly, unlike public ctors. This is to slightly reduce GC pressure.
        // Also opens the way to add more validation to the public ctors at some point.
        private Goal(ImmutableHashSet<Literal> elements) => Elements = elements;

        /// <summary>
        /// Gets a singleton <see cref="Goal"/> instance that is empty.
        /// </summary>
        public static Goal Empty { get; } = new Goal();

        /// <summary>
        /// Gets the set of literals that comprise this goal.
        /// The positive ones indicate predicates that must exist in a <see cref="State"/> in order for the goal to be satisfied.
        /// The negative ones indicate predicates that must NOT exist in a <see cref="State"/> in order for the goal to be satisfied.
        /// </summary>
        // TODO-PERFORMANCE: Should perhaps store positive and negative elements separately, for performance.
        // Application and Regression are going to be far more common than wanting to get all elements.
        // Then again, if and this is expanded to richer PDDL functionality (or if we want to allow extension.. - unlikely given the motivator for the project, but..).
        // In any case, add some benchmarks before making any changes.
        public ImmutableHashSet<Literal> Elements { get; }

        /// <summary>
        /// Gets the positive elements of the goal - those whose predicates must exist in a <see cref="State"/> in order for this goal to be satisfied.
        /// </summary>
        public IEnumerable<Literal> PositiveElements => Elements.Where(l => l.IsPositive);

        /// <summary>
        /// Gets the negative elements of the goal - those whose predicates must NOT exist in a <see cref="State"/> in order for this goal to be satisfied.
        /// </summary>
        public IEnumerable<Literal> NegativeElements => Elements.Where(l => l.IsNegated);

        /// <summary>
        /// Gets the positive predicates of the goal - those that must exist in a <see cref="State"/> in order for this goal to be satisfied.
        /// </summary>
        public IEnumerable<Predicate> PositivePredicates => PositiveElements.Select(l => l.Predicate);

        /// <summary>
        /// Gets the negative predicates of the goal - those that must NOT exist in a <see cref="State"/> in order for this goal to be satisfied.
        /// </summary>
        public IEnumerable<Predicate> NegativePredicates => NegativeElements.Select(l => l.Predicate);

        /// <summary>
        /// Gets a value indicating whether this goal is satisfied by a particular state.
        /// <para/>
        /// A goal is satisfied by a state if all of its positive elements and none of its negative elements are present in the state.
        /// NB: This methods checks only if the goal is satisfied by the state exactly - meaning that it'll never return true if the goal
        /// has variables in it. I'm still indecisive as to whether the unification logic that is required to be more intelligent belongs
        /// in here (well, in State - but you know what I mean) or elsewhere.
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        // TODO: Will need to decide the above when adding support variables in goals
        public bool IsSatisfiedBy(State state) => state.Satisfies(this);

        /// <summary>
        /// Returns the goal that must be satisfied prior to performing a given action, in order to ensure that this goal is satisfied after the action is performed. 
        /// <para/>
        /// NB: AIaMA gets regression a bit.. err.. wrong, because it gets a little confused between states (which, under the
        /// closed-world assumption, need include only positive fluents) and goals (which can include both positive and negative fluents).
        /// It treats add lists and delete lists fundamentally differently, but shouldn't.
        /// <para/>
        /// A sound way to reason about regression is to first note that it operates on a goal to give a goal - that is, both positive and negative fluents appear can
        /// occur in both the argument and the return value. Then note that any element of the given goal (positive or negative) that is applied by the action doesn't
        /// have to hold in the returned goal (because it'll be applied by the action) - so is removed. However, all preconditions do have to hold - so they are added.
        /// </summary>
        /// <param name="action">The action to regress over.</param>
        /// <returns>The goal that must be satisfied prior to performing the given action.</returns>
        public Goal Regress(Action action) => new(Elements.Except(action.Effect.Elements).Union(action.Precondition.Elements));

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// <para/>
        /// Goals implement value semantics for equality - two Goals are equal if they share the same Elements.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object? obj)
        {
            // Goals should be small-ish, so I'm not too worried by the inefficiency here.
            // Otherwise could think about sorting the set of elements (e.g. using ImmutableSortedSet sorted by hash code), maybe?
            // Would need testing whether the benefit is outweighed by constructing the ordered set in first place.
            return obj is Goal goal && goal.Elements.IsSubsetOf(Elements) && Elements.IsSubsetOf(goal.Elements);
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
}
