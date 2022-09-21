using SCFirstOrderLogic;
using SCFirstOrderLogic.SentenceManipulation;

namespace SCClassicalPlanningAlternatives.UsingSCFirstOrderLogic
{
    /// <summary>
    /// Surrogate class for <see cref="State"/> that defines a &amp; operators for conjuncting additional atoms. Implicitly convertible
    /// from and to <see cref="State"/> instances.
    /// </summary>
    public sealed class State
    {
        public State(IEnumerable<CNFLiteral> elements)
        {
            Elements = new HashSet<CNFLiteral>(elements);
        }

        public State(params CNFLiteral[] elements) : this((IEnumerable<CNFLiteral>)elements) { }

        /// <summary>
        /// Gets a <see cref="State"/> instance that is empty.
        /// </summary>
        public static State Empty { get; } = new State();

        public IReadOnlySet<CNFLiteral> Elements { get; }

        public static State operator &(State state, CNFLiteral atom) => new State(state.Elements.Append(atom).ToArray());

        public static State operator &(CNFLiteral atom, State state) => new State(state.Elements.Append(atom).ToArray());

        public static State operator &(State state, Predicate predicate) => new State(state.Elements.Append(new CNFLiteral(predicate)).ToArray());

        public static State operator &(Predicate predicate, State state) => new State(state.Elements.Append(new CNFLiteral(predicate)).ToArray());

        /// <summary>
        /// Defines the implicit conversion of a <see cref="CNFLiteral"/> instance to a <see cref="State"/> instance.
        /// </summary>
        /// <param name="literal">The literal being converted.</param>
        public static implicit operator State(CNFLiteral literal) => new State(literal);

        /// <summary>
        /// Defines the implicit conversion of a <see cref="Predicate"/> instance to a <see cref="State"/> instance.
        /// </summary>
        /// <param name="predicate">The predicate being converted.</param>
        public static implicit operator State(Predicate predicate) => new State(new CNFLiteral(predicate));
    }
}
