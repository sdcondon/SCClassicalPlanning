namespace SCAutomatedPlanning.Classical
{
    /// <summary>
    /// Encapsulates an atomic component of the state of a problem.
    /// Broadly analagous to a literal (i.e. a predicate or negated predicate) of first order logic.
    /// </summary>
    public class Atom
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="Atom"/> class.
        /// </summary>
        /// <param name="symbol">The symbol of the underlying predicate.</param>
        /// <param name="variables">The arguments of the underlying predicate.</param>
        public Atom(object symbol, params Variable[] variables) => (IsNegated, Symbol, Variables) = (false, symbol, variables);

        /// <summary>
        /// Initialises a new instance of the <see cref="Atom"/> class.
        /// </summary>
        /// <param name="isNegated">A value indicating whether this atom is a negation of the the underlying predicate.</param>
        /// <param name="symbol">The symbol of the underlying predicate.</param>
        /// <param name="variables">The arguments of the underlying predicate.</param>
        public Atom(bool isNegated, object symbol, params Variable[] variables) => (IsNegated, Symbol, Variables) = (isNegated, symbol, variables);

        /// <summary>
        /// Gets a value indicating whether this atom is a negation of the the underlying predicate.
        /// </summary>
        public bool IsNegated { get; }

        /// <summary>
        /// Gets the symbol of the underlying predicate.
        /// </summary>
        public object Symbol { get; }

        /// <summary>
        /// Gets the arguments of the underlying predicate.
        /// </summary>
        public Variable[] Variables { get; }

        // TODO: public override bool Equals(Atom atom)
    }
}
