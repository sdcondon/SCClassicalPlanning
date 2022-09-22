namespace SCClassicalPlanning
{
    /// <summary>
    /// Surrogate class for <see cref="State"/> that defines a &amp; operators for conjuncting additional atoms. Implicitly convertible
    /// from and to <see cref="State"/> instances.
    /// <para/>
    /// TODO: talk (briefly) about the differences and similarities between this and GD/Effect in PDDL.
    /// </summary>
    public sealed class State
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="State"/> class from an enumerable of the literals that comprise it.
        /// </summary>
        /// <param name="elements">The literals that comprise the state.</param>
        public State(IEnumerable<Literal> elements)
        {
            Elements = new HashSet<Literal>(elements);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="State"/> class from a (params) array of the literals that comprise it.
        /// </summary>
        /// <param name="elements">The literals that comprise the state.</param>
        public State(params Literal[] elements) : this((IEnumerable<Literal>)elements) { }

        /// <summary>
        /// Gets a singleton <see cref="State"/> instance that is empty.
        /// </summary>
        public static State Empty { get; } = new State();

        /// <summary>
        /// Gets the set of literals that comprise this state.
        /// </summary>
        public IReadOnlySet<Literal> Elements { get; }

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
