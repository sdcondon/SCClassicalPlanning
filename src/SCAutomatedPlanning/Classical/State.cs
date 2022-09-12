namespace SCAutomatedPlanning.Classical
{
    /// <summary>
    /// Encapsulates some state of a problem.
    /// Braodly analagous to a sentence of first order logic - consisting of a conjunction of literals.
    /// </summary>
    public class State
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="State"/> class.
        /// </summary>
        /// <param name="atoms"></param>
        public State(params Atom[] atoms) => Atoms = atoms;

        /// <summary>
        /// Gets the set of atoms that comprise the state.
        /// </summary>
        public IReadOnlyCollection<Atom> Atoms { get; }

        // TODO: public State Add(Atom atom)

        // TODO: public State Remove(Atom atom)
    }
}
