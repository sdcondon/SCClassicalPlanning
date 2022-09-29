using SCFirstOrderLogic;
using System.Collections.Immutable;

namespace SCClassicalPlanning
{
    /// <summary>
    ///
    /// <para/>
    /// TODO: talk (briefly) about the differences and similarities between this and GD/Effect in PDDL.
    /// <para/>
    /// TODO: probably should add some verification that all literals are functionless. TODO: Should also probably store add and delete lists separately,
    /// for performance. Application and Regression are going to be far more common than wanting to get all elements of a state.
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

        //public static State operator &(State state, Literal atom) => new State(state.Elements.Append(atom).ToArray());

        //public static State operator &(Literal atom, State state) => new State(state.Elements.Append(atom).ToArray());

        //public static State operator &(State state, Predicate predicate) => new State(state.Elements.Append(new Literal(predicate)).ToArray());

        //public static State operator &(Predicate predicate, State state) => new State(state.Elements.Append(new Literal(predicate)).ToArray());

        ///// <summary>
        ///// Defines the implicit conversion of a <see cref="Literal"/> instance to a <see cref="State"/> instance.
        ///// </summary>
        ///// <param name="literal">The literal being converted.</param>
        //public static implicit operator State(Literal literal) => new State(literal);

        ///// <summary>
        ///// Defines the implicit conversion of a <see cref="Predicate"/> instance to a <see cref="State"/> instance.
        ///// </summary>
        ///// <param name="predicate">The predicate being converted.</param>
        //public static implicit operator State(Predicate predicate) => new State(new Literal(predicate));
    }
}
