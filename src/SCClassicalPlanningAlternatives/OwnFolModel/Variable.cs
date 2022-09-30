namespace SCClassicalPlanningAlternatives.OwnFolModel
{
    public sealed class Variable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Variable"/> class.
        /// </summary>
        /// <param name="identifier">The identifier for this variable.</param>
        public Variable(object identifier) => Identifier = identifier;

        /// <summary>
        /// Gets the identifier for this variable.
        /// </summary>
        public object Identifier { get; }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is Variable variable && variable.Identifier.Equals(Identifier);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Identifier);
        }
    }
}
