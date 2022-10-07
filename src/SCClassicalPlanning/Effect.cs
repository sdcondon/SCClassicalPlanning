using SCFirstOrderLogic;
using SCFirstOrderLogic.SentenceManipulation;
using System.Collections.Immutable;

namespace SCClassicalPlanning
{
    /// <summary>
    /// Container for information about an effect - some change in the state of a system.
    /// <para/>
    /// <see cref="Effect"/>s are essentially a set of (functionless) <see cref="Literal"/>s. The positive ones indicates predicates that are added to a
    /// state by the effect's application. The negative ones indicate predicates that are removed. Effects are applied as the result of <see cref="Action"/>s.
    /// <para/>
    /// TODO: probably should add some verification that all literals are functionless.
    /// <br/>TODO: Should also probably store add and delete lists separately,
    /// for performance. Application and Regression are going to be far more common than wanting to get all elements. Then again, if and this is expanded to richer
    /// PDDL functionality (or if we want to allow extension.. - unlikely given the motivator for the project, but..)
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

        /// <summary>
        /// Gets a singleton <see cref="Effect"/> instance that is empty.
        /// </summary>
        public static Effect Empty { get; } = new Effect();

        /// <summary>
        /// Gets the set of literals that comprise this effect.
        /// </summary>
        public ImmutableHashSet<Literal> Elements { get; }

        /// <summary>
        /// Gets the "add list" of the effect - the non-negated predicates within the <see cref="Elements"/> set.
        /// These are removed from a <see cref="State"/> when this effect is applied.
        /// </summary>
        public IEnumerable<Predicate> AddList => Elements.Where(a => !a.IsNegated).Select(l => l.Predicate);

        /// <summary>
        /// Gets the "delete list" of the effect - the non-negated predicates within the <see cref="Elements"/> set.
        /// These are removed from a <see cref="State"/> when this effect is applied.
        /// </summary>
        public IEnumerable<Predicate> DeleteList => Elements.Where(a => a.IsNegated).Select(l => l.Predicate);

        /// <summary>
        /// Gets a value indicating whether this effect is relevant to a given goal.
        /// <para/>
        /// An effect is relevant to a goal if it accomplishes at least one element of the goal, and does not undo anything.
        /// That is, the effect's elements overlap with the goals elements, and the set comprised of the negation of each of the effect's elements does not.
        /// </summary>
        /// <param name="goal">The goal to determine relevancy to.</param>
        /// <returns>A value indicating whether this effect is relevant to a given goal.</returns>
        public bool IsRelevantTo(Goal goal)
        {
            return goal.Elements.Overlaps(Elements) && !goal.Elements.Overlaps(Elements.Select(l => l.Negate()));
        }

        /// <summary>
        /// Applies this action to a given state, producing a new state.
        /// </summary>
        /// <param name="state">The state to apply the effect to.</param>
        /// <returns>The new state.</returns>
        public State ApplyTo(State state) => state.Apply(this);

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// <para/>
        /// Effects implement value semantics for equality - two Effects are equal if they share the same Elements.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object? obj)
        {
            // Effects should be small-ish, so I'm not too worried by the inefficiency here.
            // Otherwise could think about sorting the set of elements (e.g. using ImmutableSortedSet sorted by hash code), maybe?
            // Would need testing whether benefit is outweighed by constructing the ordered set in first place..
            return obj is Effect effect && effect.Elements.IsSubsetOf(Elements) && Elements.IsSubsetOf(effect.Elements);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            var hashCode = new HashCode();

            // As with Equals, asserting that this is okay given that effects should be small-ish.
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
            private static readonly ConstructionVisitor Instance = new ConstructionVisitor();

            public static HashSet<Literal> Visit(Sentence sentence)
            {
                var elements = new HashSet<Literal>();
                Instance.Visit(sentence, ref elements);
                return elements;
            }

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
                    // Assume we've hit a literal. NB: ctor will throw if its not actually a literal.
                    // Afterwards, we don't need to look any further down the tree for the purposes of this class (though the Literal ctor that
                    // we invoke here does so to figure out the details of the literal). So we can just return rather than invoking base.Visit.
                    literals.Add(new Literal(sentence));
                }
            }
        }
    }
}
