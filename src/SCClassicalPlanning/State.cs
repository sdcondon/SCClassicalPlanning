namespace SCClassicalPlanning
{
    /// <summary>
    /// Surrogate class for <see cref="State"/> that defines a &amp; operators for conjuncting additional atoms. Implicitly convertible
    /// from and to <see cref="State"/> instances.
    /// </summary>
    public sealed class State
    {
        public State(IEnumerable<Literal> elements)
        {
            Elements = new HashSet<Literal>(elements);
        }

        public State(params Literal[] elements) : this((IEnumerable<Literal>)elements) { }

        /// <summary>
        /// Gets a <see cref="State"/> instance that is empty.
        /// </summary>
        public static State Empty { get; } = new State();

        public IReadOnlySet<Literal> Elements { get; }

        public static State operator &(State state, Literal atom) => new State(state.Elements.Append(atom).ToArray());

        public static State operator &(Literal atom, State state) => new State(state.Elements.Append(atom).ToArray());

        public static State operator &(State state, Predicate predicate) => new State(state.Elements.Append(new Literal(false, predicate)).ToArray());

        public static State operator &(Predicate predicate, State state) => new State(state.Elements.Append(new Literal(false, predicate)).ToArray());

        /// <summary>
        /// Defines the implicit conversion of a <see cref="Literal"/> instance to a <see cref="State"/> instance.
        /// </summary>
        /// <param name="literal">The literal being converted.</param>
        public static implicit operator State(Literal literal) => new State(literal);

        /// <summary>
        /// Defines the implicit conversion of a <see cref="Predicate"/> instance to a <see cref="State"/> instance.
        /// </summary>
        /// <param name="predicate">The predicate being converted.</param>
        public static implicit operator State(Predicate predicate) => new State(new Literal(false, predicate));
    }
}
