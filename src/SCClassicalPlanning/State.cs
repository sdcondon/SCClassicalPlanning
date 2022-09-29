using SCFirstOrderLogic;
using System.Collections.Immutable;

namespace SCClassicalPlanning
{
    /// <summary>
    ///
    /// <para/>
    /// TODO: talk (briefly) about the differences and similarities between this and GD/Effect in PDDL.
    /// <para/>
    /// TODO: probably should add some verification that all literals are functionless.
    /// </summary>
    public sealed class State
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="State"/> class from an enumerable of the predicates that comprise it.
        /// </summary>
        /// <param name="elements">The predicates that comprise the state.</param>
        public State(IEnumerable<Predicate> elements)
        {
            Elements = elements.ToImmutableHashSet();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="State"/> class from a (params) array of the predicates that comprise it.
        /// </summary>
        /// <param name="elements">The predicates that comprise the state.</param>
        public State(params Predicate[] elements) : this((IEnumerable<Predicate>)elements) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="State" /> class from a sentence of first order logic. The sentence must normalize to a conjunction of positive literals, or an exception will be thrown.
        /// </summary>
        /// <param name="sentence"></param>
        public State(Sentence sentence)
        {
            var elements = new HashSet<Predicate>();

            foreach (var clause in new CNFSentence(sentence).Clauses)
            {
                if (!clause.IsUnitClause)
                {
                    throw new ArgumentException();
                }

                var literal = clause.Literals.First();

                if (literal.IsNegated)
                {
                    throw new ArgumentException();
                }

                elements.Add(literal.Predicate);
            }

            Elements = elements.ToImmutableHashSet();
        }

        /// <summary>
        /// Gets a singleton <see cref="State"/> instance that is empty.
        /// </summary>
        public static State Empty { get; } = new State();

        /// <summary>
        /// Gets the set of predicates that comprise this state.
        /// </summary>
        public IReadOnlySet<Predicate> Elements { get; }

        /// <summary>
        /// Gets a value indicating whether this state's elements are a subset of the given state's elements.
        /// </summary>
        /// <param name="state">The state to compare this state to.</param>
        /// <returns>True if and only if this state's elements are a subset of the given state's elements.</returns>
        public bool IsSubstateOf(State state) => Elements.IsSubsetOf(state.Elements);

        /// <summary>
        /// Gets a value indicating whether this state's elements are a superset of the given state's elements.
        /// </summary>
        /// <param name="state">The state to compare this state to.</param>
        /// <returns>True if and only if this state's elements are a superset of the given state's elements.</returns>
        public bool IsSuperstateOf(State state) => Elements.IsSupersetOf(state.Elements);

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
