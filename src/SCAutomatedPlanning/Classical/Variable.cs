namespace SCAutomatedPlanning.Classical
{
    /// <summary>
    /// 
    /// </summary>
    public class Variable : IEquatable<Variable>
    {
        /// <summary>
        /// Initialixes a new instance of the <see cref="Variable"/> class.
        /// </summary>
        /// <param name="symbol">The symbol of the variable.</param>
        public Variable(object symbol) => Symbol = symbol;

        public object Symbol { get; }

        /// <inheritdoc />
        public bool Equals(Variable? variable) => variable != null && variable.Symbol.Equals(Symbol);

        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is Variable variable && Equals(variable);

        /// <inheritdoc /> 
        public override int GetHashCode() => HashCode.Combine(Symbol);
    }
}
