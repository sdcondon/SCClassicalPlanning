namespace SCAutomatedPlanning.Classical
{
    public class Variable : IEquatable<Variable>
    {
        public Variable(object symbol) => Symbol = symbol;

        public object Symbol { get; }

        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is Variable variable && Equals(variable);

        /// <inheritdoc />
        public bool Equals(Variable? variable) => variable != null && variable.Symbol.Equals(Symbol);
    }
}
