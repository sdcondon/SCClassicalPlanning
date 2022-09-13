namespace SCAutomatedPlanning.Classical
{
    /// <summary>
    /// Encapsulates some state of a problem. Braodly equivalent to a conjunction of its consituent literals.
    /// </summary>
    public class State
    {
        private readonly HashSet<Literal> atoms;

        /// <summary>
        /// Initializes a new instance of the <see cref="State"/> class.
        /// </summary>
        /// <param name="atoms">The set of atoms that comprise the state.</param>
        public State(IEnumerable<Literal> atoms) => this.atoms = new HashSet<Literal>(atoms);

        /// <summary>
        /// Initializes a new instance of the <see cref="State"/> class.
        /// </summary>
        /// <param name="atoms">The set of atoms that comprise the state.</param>
        public State(params Literal[] atoms) => this.atoms = new HashSet<Literal>(atoms);

        /// <summary>
        /// Gets the set of literal that comprise the state.
        /// </summary>
        public IReadOnlySet<Literal> Atoms => atoms;
    }
}
