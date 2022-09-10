namespace SCAutomatedPlanning.Classical
{
    /// <summary>
    /// Shorthand factory methods for <see cref="State"/> instances.
    /// </summary>
    public class OperableStateFactory
    {
        /// <summary>
        /// Surrogate class for <see cref="State"/> that defines a &amp; operators for conjuncting additional atoms. Implicitly convertible
        /// from and to <see cref="State"/> instances.
        /// </summary>
        public class OperableState
        {
            internal OperableState(params OperableAtom[] atoms) => Atoms = atoms;

            public OperableAtom[] Atoms { get; }

            public static OperableState operator &(OperableState state, OperableAtom atom) => new OperableState(state.Atoms.Append(atom).ToArray());

            public static OperableState operator &(OperableAtom atom, OperableState state) => new OperableState(state.Atoms.Append(atom).ToArray());

            public static implicit operator State(OperableState state) => new State(state.Atoms.Select(a => (Atom)a).ToArray());

            //public static implicit operator OperableState(OperableAtom atom) => new OperableState(atom);
        }

        /// <summary>
        /// Surrogate class for <see cref="Atom"/> that defines ! and &amp; operators for negation and conjunction respectively. Implicitly convertible
        /// from and to an <see cref="Atom"/>, and to a <see cref="State"/> (consisting of just this atom).
        /// </summary>
        public class OperableAtom
        {
            internal OperableAtom(bool isNegated, object symbol, Variable[] variables) => (IsNegated, Symbol, Variables) = (IsNegated, symbol, variables);

            public bool IsNegated { get; }

            public object Symbol { get; }

            public Variable[] Variables { get; }

            public static OperableAtom operator !(OperableAtom atom) => new OperableAtom(!atom.IsNegated, atom.Symbol, atom.Variables);

            public static OperableState operator &(OperableAtom left, OperableAtom right) => new OperableState(left, right);

            public static implicit operator OperableAtom(Atom atom) => new OperableAtom(atom.IsNegated, atom.Symbol, atom.Variables);

            public static implicit operator Atom(OperableAtom atom) => new Atom(atom.IsNegated, atom.Symbol, atom.Variables);

            public static implicit operator State(OperableAtom atom) => new State(atom);
        }
    }
}
