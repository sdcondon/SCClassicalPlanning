using System.Collections.ObjectModel;

namespace SCClassicalPlanningAlternatives.OwnFolModel
{
    /// <summary>
    /// Represents a predicate of first order logic. That is, some fact (taking zero or more terms as arguments) that either holds true or does not.
    /// </summary>
    public sealed class Predicate
    {
        public Predicate(object identifier, IList<Variable> arguments) => (Identifier, Arguments) = (identifier, new ReadOnlyCollection<Variable>(arguments));

        public Predicate(object identifier, params Variable[] arguments) : this(identifier, (IList<Variable>)arguments) { }

        /// <summary>
        /// Gets the unique identifier of the predicate.
        /// </summary>
        public object Identifier { get; }

        /// <summary>
        /// Gets the arguments of the predicate.
        /// </summary>
        public IReadOnlyCollection<Variable> Arguments { get; }

        public static State operator &(Predicate left, Predicate right) => new State(new Literal(false, left), new Literal(false, right));

        public static Literal operator !(Predicate predicate) => new Literal(true, predicate);
    }
}
