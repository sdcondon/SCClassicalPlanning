﻿using SCFirstOrderLogic;
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
    public sealed class Effect
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Effect"/> class from an enumerable of the literals that comprise it.
        /// </summary>
        /// <param name="elements">The literals that comprise the state.</param>
        public Effect(IEnumerable<Literal> elements)
        {
            Elements = elements.ToImmutableHashSet();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Effect"/> class from a (params) array of the literals that comprise it.
        /// </summary>
        /// <param name="elements">The literals that comprise the state.</param>
        public Effect(params Literal[] elements) : this((IEnumerable<Literal>)elements) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Effect" /> class from a sentence of first order logic. The sentence must normalize to a conjunction of literals, or an exception will be thrown.
        /// </summary>
        /// <param name="sentence"></param>
        public Effect(Sentence sentence)
        {
            // NB: it is important NOT to standardise variables at this point, because we will generally
            // definitions that are common across e.g. the precondition and goal of an action.
            var elements = new HashSet<Literal>();
            EffectConstructor.Instance.Visit(sentence, ref elements);
            Elements = elements.ToImmutableHashSet();
        }

        /// <summary>
        /// Gets the set of literals that comprise this effect.
        /// </summary>
        public IReadOnlySet<Literal> Elements { get; }

        /// <summary>
        /// Gets the "add list" of the action - the non-negated atoms in the action's effect.
        /// </summary>
        public IEnumerable<Predicate> AddList => Elements.Where(a => !a.IsNegated).Select(l => l.Predicate);

        /// <summary>
        /// Gets the "delete list" of the action - the negated atoms in the action's effect.
        /// </summary>
        public IEnumerable<Predicate> DeleteList => Elements.Where(a => a.IsNegated).Select(l => l.Predicate);

        /// <summary>
        /// Applies this action to a given state, producing a new state.
        /// </summary>
        /// <param name="state">The state to apply the action to.</param>
        /// <returns>The new state.</returns>
        public State ApplyTo(State state) => new State(state.Elements.Except(DeleteList).Union(AddList));

        private class EffectConstructor : RecursiveSentenceVisitor<HashSet<Literal>>
        {
            public static EffectConstructor Instance { get; } = new EffectConstructor();

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
