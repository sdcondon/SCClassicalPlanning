namespace SCAutomatedPlanning.Classical
{
    /// <summary>
    /// Encapsulates some state of a problem.
    /// </summary>
    public class State
    {
        private readonly HashSet<Atom> atoms;

        /// <summary>
        /// Initializes a new instance of the <see cref="State"/> class.
        /// </summary>
        /// <param name="atoms">The set of atoms that comprise the state.</param>
        public State(IEnumerable<Atom> atoms) => this.atoms = new HashSet<Atom>(atoms);

        /// <summary>
        /// Initializes a new instance of the <see cref="State"/> class.
        /// </summary>
        /// <param name="atoms">The set of atoms that comprise the state.</param>
        public State(params Atom[] atoms) => this.atoms = new HashSet<Atom>(atoms);

        /// <summary>
        /// Gets the set of atoms that comprise the state.
        /// </summary>
        public IReadOnlySet<Atom> Atoms => atoms;

        // TODO: public State Add(Atom atom)

        // TODO: public State Remove(Atom atom)
    }
}
