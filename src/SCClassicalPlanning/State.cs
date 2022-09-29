using SCFirstOrderLogic;
using System.Collections.Immutable;

namespace SCClassicalPlanning
{
    /// <summary>
    ///
    /// <para/>
    /// Is essentially just a set of ground predicates - that collectively describe the current state of the system
    /// <para/>
    /// TODO: probably should add some verification that all literals are functionless. also i think needs to verify that its ground?
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

                //// TODO: Belongs in other ctor
                ////if (literal.Predicate.Arguments.Any(a => !a.IsGroundTerm))
                ////{
                ////    throw new ArgumentException();
                ////}
                
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
    }
}
