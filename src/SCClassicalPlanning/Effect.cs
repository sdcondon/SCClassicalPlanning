using SCFirstOrderLogic;
using System.Collections.Immutable;

namespace SCClassicalPlanning
{
    /// <summary>
    /// Container for information about an effect - some change of the state of a system. These occur as the result of <see cref="Action"/>s.
    /// <para/>
    /// <see cref="Effect"/>s are essentially a set of (functionless) <see cref="Literal"/>s. The positive ones indicates predicates that are added to a
    /// state by the effect's application. The negative ones indicate predicates that are removed, and absent ones remain unchanged.
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

        // TODO?: public Effect(IEnumerable<Predicate> addList, IEnumerable<Predicate> deleteList)

        /// <summary>
        /// Initializes a new instance of the <see cref="Effect" /> class from a sentence of first order logic. The sentence must normalize to a conjunction of literals, or an exception will be thrown.
        /// </summary>
        /// <param name="sentence"></param>
        public Effect(Sentence sentence)
        {
            var elements = new HashSet<Literal>();

            foreach (var clause in new CNFSentence(sentence).Clauses)
            {
                if (!clause.IsUnitClause)
                {
                    throw new ArgumentException();
                }

                elements.Add(clause.Literals.First());
            }

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
    }
}
