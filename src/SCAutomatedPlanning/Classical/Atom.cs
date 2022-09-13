using System;
using System.Collections.ObjectModel;

namespace SCAutomatedPlanning.Classical
{
    /// <summary>
    /// Encapsulates an atomic component of the state of a problem.
    /// Broadly analagous to a literal (i.e. a predicate or negated predicate) of first order logic - except that functions are forbidden.
    /// </summary>
    public class Atom
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="Atom"/> class.
        /// </summary>
        /// <param name="symbol">The symbol of the underlying predicate.</param>
        /// <param name="argumenta">The arguments of the underlying predicate.</param>
        public Atom(object symbol, params Variable[] arguments) : this(false, symbol, (IList<Variable>)arguments) { }

        /// <summary>
        /// Initialises a new instance of the <see cref="Atom"/> class.
        /// </summary>
        /// <param name="symbol">The symbol of the underlying predicate.</param>
        /// <param name="argumenta">The arguments of the underlying predicate.</param>
        public Atom(object symbol, IList<Variable> arguments) : this(false, symbol, arguments) { }

        /// <summary>
        /// Initialises a new instance of the <see cref="Atom"/> class.
        /// </summary>
        /// <param name="isNegated">A value indicating whether this atom is a negation of the the underlying predicate.</param>
        /// <param name="symbol">The symbol of the underlying predicate.</param>
        /// <param name="arguments">The arguments of the underlying predicate.</param>
        public Atom(bool isNegated, object symbol, params Variable[] arguments) : this(isNegated, symbol, (IList<Variable>)arguments) { }

        /// <summary>
        /// Initialises a new instance of the <see cref="Atom"/> class.
        /// </summary>
        /// <param name="isNegated">A value indicating whether this atom is a negation of the the underlying predicate.</param>
        /// <param name="symbol">The symbol of the underlying predicate.</param>
        /// <param name="arguments">The arguments of the underlying predicate.</param>
        public Atom(bool isNegated, object symbol, IList<Variable> arguments) => (IsNegated, Symbol, Arguments) = (isNegated, symbol, new ReadOnlyCollection<Variable>(arguments));

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
        public ReadOnlyCollection<Variable> Arguments { get; }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj is not Atom otherAtom
                || !otherAtom.Symbol.Equals(Symbol)
                || otherAtom.Arguments.Count != Arguments.Count)
            {
                return false;
            }

            for (int i = 0; i < Arguments.Count; i++)
            {
                if (!Arguments[i].Equals(otherAtom.Arguments[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            var hashCode = new HashCode();

            hashCode.Add(Symbol);
            foreach (var argument in Arguments)
            {
                hashCode.Add(argument);
            }

            return hashCode.ToHashCode();
        }
    }
}
