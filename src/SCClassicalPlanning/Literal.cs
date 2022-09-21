namespace SCClassicalPlanning
{
    /// <summary>
    /// Represents a literal of first-order logic. That is, a predicate or negated predicate.
    /// </summary>
    public sealed class Literal
    {
        public Literal(bool isNegated, Predicate predicate) => (IsNegated, Predicate) = (isNegated, predicate);

        public bool IsNegated { get; }

        public Predicate Predicate { get; }

        public static Literal operator !(Literal literal) => new Literal(!literal.IsNegated, literal.Predicate);

        public static State operator &(Literal left, Literal right) => new State(left, right);

        public static State operator &(Literal left, Predicate right) => new State(left, new Literal(false, right));

        public static State operator &(Predicate left, Literal right) => new State(new Literal(false, left), right);

        public static implicit operator Literal(Predicate predicate) => new Literal(false, predicate);
    }
}
