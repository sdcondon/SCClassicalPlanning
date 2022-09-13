using System.Collections.ObjectModel;

namespace SCAutomatedPlanning.Classical.StateCreation
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

            public static implicit operator State(OperableState state) => new State(state.Atoms.Select(a => (Literal)a).ToArray());

            //public static implicit operator OperableState(OperableAtom atom) => new OperableState(atom);
        }

        /// <summary>
        /// Surrogate class for <see cref="Literal"/> that defines ! and &amp; operators for negation and conjunction respectively. Implicitly convertible
        /// from and to an <see cref="Literal"/>, and to a <see cref="State"/> (consisting of just this atom).
        /// </summary>
        public class OperableAtom
        {
            internal OperableAtom(bool isNegated, object symbol, IList<Variable> variables) => (IsNegated, Symbol, Arguments) = (IsNegated, symbol, new ReadOnlyCollection<Variable>(variables));

            public bool IsNegated { get; }

            public object Symbol { get; }

            public ReadOnlyCollection<Variable> Arguments { get; }

            public static OperableAtom operator !(OperableAtom atom) => new OperableAtom(!atom.IsNegated, atom.Symbol, atom.Arguments);

            public static OperableState operator &(OperableAtom left, OperableAtom right) => new OperableState(left, right);

            public static implicit operator OperableAtom(Literal atom) => new OperableAtom(atom.IsNegated, atom.Symbol, atom.Arguments);

            public static implicit operator Literal(OperableAtom atom) => new Literal(atom.IsNegated, atom.Symbol, atom.Arguments);

            public static implicit operator State(OperableAtom atom) => new State(atom);
        }

        public static readonly Variable A = new(nameof(A));
        public static readonly Variable B = new(nameof(B));
        public static readonly Variable C = new(nameof(C));
        public static readonly Variable D = new(nameof(D));
        public static readonly Variable E = new(nameof(E));
        public static readonly Variable F = new(nameof(F));
        public static readonly Variable G = new(nameof(G));
        public static readonly Variable H = new(nameof(H));
        public static readonly Variable I = new(nameof(I));
        public static readonly Variable J = new(nameof(J));
        public static readonly Variable K = new(nameof(K));
        public static readonly Variable L = new(nameof(L));
        public static readonly Variable M = new(nameof(M));
        public static readonly Variable N = new(nameof(N));
        public static readonly Variable O = new(nameof(O));
        public static readonly Variable P = new(nameof(P));
        public static readonly Variable Q = new(nameof(Q));
        public static readonly Variable R = new(nameof(R));
        public static readonly Variable S = new(nameof(S));
        public static readonly Variable T = new(nameof(T));
        public static readonly Variable U = new(nameof(U));
        public static readonly Variable V = new(nameof(V));
        public static readonly Variable W = new(nameof(W));
        public static readonly Variable X = new(nameof(X));
        public static readonly Variable Y = new(nameof(Y));
        public static readonly Variable Z = new(nameof(Z));
    }
}
