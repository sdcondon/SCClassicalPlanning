using SCFirstOrderLogic;
using System.Collections.Immutable;

namespace SCClassicalPlanning
{
    /// <summary>
    ///
    /// <para/>
    /// TODO: talk (briefly) about the differences and similarities between this and GD/Effect in PDDL.
    /// <para/>
    /// TODO: probably should add some verification that all literals are functionless. TODO: Should also probably store add and delete lists separately,
    /// for performance. Application and Regression are going to be far more common than wanting to get all elements of a state.
    /// </summary>
    public sealed class GoalDescription
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Effect"/> class from an enumerable of the literals that comprise it.
        /// <para/>
        /// TODO: explain that in general (i.e. in PDDL) doesn't have to be a conjunction of literals - in general can have disjunctions, implications etc - but *does* have to be functionless.
        /// </summary>
        /// <param name="elements">The literals that comprise the state.</param>
        public GoalDescription(IEnumerable<Literal> elements)
        {
            Elements = elements.ToImmutableHashSet();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Effect"/> class from a (params) array of the literals that comprise it.
        /// </summary>
        /// <param name="elements">The literals that comprise the state.</param>
        public GoalDescription(params Literal[] elements) : this((IEnumerable<Literal>)elements) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Effect" /> class from a sentence of first order logic. The sentence must normalize to a conjunction of literals, or an exception will be thrown.
        /// </summary>
        /// <param name="sentence"></param>
        public GoalDescription(Sentence sentence)
        {
            var elements = new HashSet<Literal>();

            foreach (var clause in new CNFSentence(sentence).Clauses)
            {
                if (!clause.IsUnitClause)
                {
                    throw new ArgumentException();
                }

                elements.Add(clause.Literals.First());
            }

            Elements = elements.ToImmutableHashSet();
        }

        /// <summary>
        /// Gets a singleton <see cref="GoalDescription"/> instance that is empty.
        /// </summary>
        public static GoalDescription Empty { get; } = new GoalDescription();

        /// <summary>
        /// Gets the set of literals that comprise this effect.
        /// </summary>
        public IReadOnlySet<Literal> Elements { get; }
    }
}
