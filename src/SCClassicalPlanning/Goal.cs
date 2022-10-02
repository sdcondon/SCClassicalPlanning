using SCFirstOrderLogic;
using System.Collections.Immutable;

namespace SCClassicalPlanning
{
    /// <summary>
    /// Container for information about a goal.
    /// <para/>
    /// A <see cref="Goal"/> is essentially just a set of (functionless) <see cref="Literal"/>s. The positive ones indicate predicates that must exist in a <see cref="State"/> for it 
    /// to satisfy the goal. The negative ones indicate predicates that must NOT exist in a state for it to satisfy the goal. Goals are used to describe the end goal of <see cref="Problem"/>
    /// instances, as well as the precondition of <see cref="Action"/> instances.
    /// <para/>
    /// TODO: probably should add some verification that all literals are functionless. TODO: Should also probably store add and delete lists separately,
    /// for performance. Application and Regression are going to be far more common than wanting to get all elements of a state.
    /// </summary>
    public sealed class Goal
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Effect"/> class from an enumerable of the literals that comprise it.
        /// <para/>
        /// TODO: explain that in general (i.e. in PDDL) doesn't have to be a conjunction of literals - in general can have disjunctions, implications etc - but *does* have to be functionless.
        /// </summary>
        /// <param name="elements">The literals that comprise the state.</param>
        public Goal(IEnumerable<Literal> elements)
        {
            Elements = elements.ToImmutableHashSet();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Effect"/> class from a (params) array of the literals that comprise it.
        /// </summary>
        /// <param name="elements">The literals that comprise the state.</param>
        public Goal(params Literal[] elements) : this((IEnumerable<Literal>)elements) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Effect" /> class from a sentence of first order logic. The sentence must normalize to a conjunction of literals, or an exception will be thrown.
        /// </summary>
        /// <param name="sentence"></param>
        public Goal(Sentence sentence)
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
        /// Gets a singleton <see cref="Goal"/> instance that is empty.
        /// </summary>
        public static Goal Empty { get; } = new Goal();

        /// <summary>
        /// Gets the set of literals that comprise this goal.
        /// The positive ones indicate predicates that must exist in a <see cref="State"/> in order for the goal to be satisfied.
        /// The negative ones indicate predicates that must NOT exist in a <see cref="State"/> in order for the goal to be satisfied.
        /// </summary>
        public IReadOnlySet<Literal> Elements { get; }

        /// <summary>
        /// Gets the predicates that must exist in a <see cref="State"/> in order for this goal to be satisfied.
        /// </summary>
        public IEnumerable<Predicate> PositiveElements => Elements.Where(l => l.IsPositive).Select(l => l.Predicate);

        /// <summary>
        /// Gets the predicates that must NOT exist in a <see cref="State"/> in order for this goal to be satisfied.
        /// </summary>
        public IEnumerable<Predicate> NegativeElements => Elements.Where(l => l.IsNegated).Select(l => l.Predicate);

        /// <summary>
        /// Gets a value indicating whether this goal is satisfied by a particular state.
        /// <para/>
        /// A goal is satisfied by a state if all of its positive elements and none of its negative elements are present in the state.
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public bool IsSatisfiedBy(State state)
        {
            return state.Elements.IsSupersetOf(Elements.Where(l => l.IsPositive).Select(l => l.Predicate))
                && !state.Elements.Overlaps(Elements.Where(l => l.IsNegated).Select(l => l.Predicate));
        }
    }
}
