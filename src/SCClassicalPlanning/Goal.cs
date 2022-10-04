﻿using SCFirstOrderLogic;
using SCFirstOrderLogic.SentenceManipulation;
using System.Collections.Immutable;

namespace SCClassicalPlanning
{
    /// <summary>
    /// Container for information about a goal.
    /// <para/>
    /// A <see cref="Goal"/> is essentially just a set of (functionless) <see cref="Literal"/>s. The positive ones indicate predicates that must exist in a <see cref="State"/> for it 
    /// to satisfy the goal. The negative ones indicate predicates that must NOT exist in a state for it to satisfy the goal. Goals are used to describe the end goal of <see cref="Problem"/>
    /// instances, as well as the precondition of <see cref="Action"/> instances.
    /// <para/>
    /// TODO: probably should add some verification that all literals are functionless. TODO: Should also probably store add and delete lists separately,
    /// for performance. Application and Regression are going to be far more common than wanting to get all elements of a state.
    /// </summary>
    public class Goal
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Effect"/> class from an enumerable of the literals that comprise it.
        /// <para/>
        /// TODO: explain that in general (i.e. in PDDL) doesn't have to be a conjunction of literals - in general can have disjunctions, implications etc - but *does* have to be functionless.
        /// </summary>
        /// <param name="elements">The literals that comprise the state.</param>
        public Goal(IEnumerable<Literal> elements)
        {
            Elements = elements.ToImmutableHashSet();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Effect"/> class from a (params) array of the literals that comprise it.
        /// </summary>
        /// <param name="elements">The literals that comprise the state.</param>
        public Goal(params Literal[] elements) : this((IEnumerable<Literal>)elements) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Effect" /> class from a sentence of first order logic. The sentence must be a conjunction of literals, or an exception will be thrown.
        /// </summary>
        /// <param name="sentence"></param>
        public Goal(Sentence sentence)
        {
            // NB: it is important NOT to standardise variables at this point, because we will generally
            // want variable definitions that are common across e.g. the precondition and goal of an action.
            var elements = new HashSet<Literal>();
            GoalConstructionVisitor.Instance.Visit(sentence, ref elements);
            Elements = elements.ToImmutableHashSet();
        }

        /// <summary>
        /// Gets a singleton <see cref="Goal"/> instance that is empty.
        /// </summary>
        public static Goal Empty { get; } = new Goal();

        /// <summary>
        /// Gets the set of literals that comprise this goal.
        /// The positive ones indicate predicates that must exist in a <see cref="State"/> in order for the goal to be satisfied.
        /// The negative ones indicate predicates that must NOT exist in a <see cref="State"/> in order for the goal to be satisfied.
        /// </summary>
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
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public bool IsSatisfiedBy(State state) => state.Satisfies(this);

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
            // Would need testing whether benefit is outweighed by constructed set in first place..
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

        /// <summary>
        /// Sentence visitor class that extracts <see cref="Literal"/>s from a <see cref="Sentence"/> that is a conjunction of them.
        /// Used by the <see cref="Goal(Sentence)"/> constructor.
        /// </summary>
        private class GoalConstructionVisitor : RecursiveSentenceVisitor<HashSet<Literal>>
        {
            /// <summary>
            /// Gets a singleton instance of this class.
            /// </summary>
            public static GoalConstructionVisitor Instance { get; } = new GoalConstructionVisitor();

            /// <inheritdoc/>
            public override void Visit(Sentence sentence, ref HashSet<Literal> literals)
            {
                if (sentence is Conjunction conjunction)
                {
                    // The sentence is assumed to be a conjunction of literals - so just skip past all the conjunctions at the root.
                    base.Visit(conjunction, ref literals);
                }
                else
                {
                    // Assume we've hit a literal. NB will throw if its not actually a literal.
                    // Afterwards, we don't need to look any further down the tree for the purposes of this class (though the Literal ctor that
                    // we invoke here does so to figure out the details of the literal). So we can just return rather than invoking base.Visit.
                    literals.Add(new Literal(sentence));
                }
            }
        }
    }
}
