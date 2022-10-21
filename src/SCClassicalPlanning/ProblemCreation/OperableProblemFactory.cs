using SCFirstOrderLogic;
using SCFirstOrderLogic.SentenceManipulation;
using System.Collections.Immutable;
using static SCFirstOrderLogic.SentenceCreation.OperableSentenceFactory;

namespace SCClassicalPlanning.ProblemCreation
{
    /// <summary>
    /// Utility logic for the declaration of <see cref="Problem"/> instances, prioritising succinct C# above all else.
    /// <para/>
    /// Allows for declaring actions that have preconditions and effects stated directly as <see cref="OperableSentenceFactory.OperableSentence"/> instances.
    /// Which, err, really doesn't make much of a different, to be honest - there's not nearly as much value in this as there is in the equivalent functionality in SCFirstOrderLogic.
    /// That's why it's not mentioned in the user guide.
    /// </summary>
    public static class OperableProblemFactory
    {
        public class OperableAction
        {
            public OperableAction(object identifier, OperableGoal precondition, OperableEffect effect) => (Identifier, Precondition, Effect) = (identifier, precondition, effect);

            internal object Identifier { get; }

            internal OperableGoal Precondition { get; }

            internal OperableEffect Effect { get; }

            public static implicit operator OperableAction(Action action) => new(action.Identifier, action.Precondition, action.Effect);

            public static implicit operator Action(OperableAction action) => new(action.Identifier, action.Precondition, action.Effect);
        }

        public class OperableGoal
        {
            internal OperableGoal(IEnumerable<Literal> elements) => Elements = elements.ToImmutableHashSet();

            internal IReadOnlySet<Literal> Elements { get; }

            public static implicit operator OperableGoal(Goal goal) => new(goal.Elements);

            public static implicit operator Goal(OperableGoal goal) => new(goal.Elements);

            public static implicit operator OperableGoal(OperableSentence sentence)
            {
                var literals = new HashSet<Literal>();
                LiteralConjunctionVisitor.Instance.Visit(sentence, literals);
                return new(literals);
            }
        }

        public class OperableState
        {
            internal OperableState(IEnumerable<Predicate> elements) => Elements = elements.ToImmutableHashSet();

            internal IReadOnlySet<Predicate> Elements { get; }

            public static implicit operator OperableState(State state) => new(state.Elements);

            public static implicit operator State(OperableState state) => new(state.Elements);

            public static implicit operator OperableState(OperableSentence sentence)
            {
                var predicates = new HashSet<Predicate>();
                PredicateConjunctionVisitor.Instance.Visit(sentence, predicates);
                return new(predicates);
            }
        } 

        public class OperableEffect
        {
            public OperableEffect(IEnumerable<Literal> elements) => Elements = elements.ToImmutableHashSet();

            public IReadOnlySet<Literal> Elements { get; }

            public static implicit operator OperableEffect(Effect effect) => new(effect.Elements);

            public static implicit operator Effect(OperableEffect effect) => new(effect.Elements);

            public static implicit operator OperableEffect(OperableSentence sentence)
            {
                var literals = new HashSet<Literal>();
                LiteralConjunctionVisitor.Instance.Visit(sentence, literals);
                return new(literals);
            }
        }

        /// <summary>
        /// Sentence visitor class that extracts <see cref="Literal"/>s from a <see cref="Sentence"/> that is a conjunction of them.
        /// </summary>
        private class LiteralConjunctionVisitor : RecursiveSentenceVisitor<HashSet<Literal>>
        {
            /// <summary>
            /// Gets a singleton instance of this class.
            /// </summary>
            public static LiteralConjunctionVisitor Instance { get; } = new LiteralConjunctionVisitor();

            /// <inheritdoc/>
            public override void Visit(Sentence sentence, HashSet<Literal> literals)
            {
                if (sentence is Conjunction conjunction)
                {
                    // The sentence is assumed to be a conjunction of literals - so just skip past all the conjunctions at the root.
                    base.Visit(conjunction, literals);
                }
                else
                {
                    // Assume we've hit a literal. NB: ctor will throw if its not actually a literal.
                    // Afterwards, we don't need to look any further down the tree for the purposes of this class (though the Literal ctor that
                    // we invoke here does so to figure out the details of the literal). So we can just return rather than invoking base.Visit.
                    literals.Add(new Literal(sentence));
                }
            }
        }

        /// <summary>
        /// Sentence visitor class that extracts <see cref="Predicate"/>s from a <see cref="Sentence"/> that is a conjunction of them.
        /// </summary>
        private class PredicateConjunctionVisitor : RecursiveSentenceVisitor<HashSet<Predicate>>
        {
            /// <summary>
            /// Gets a singleton instance of this class.
            /// </summary>
            public static PredicateConjunctionVisitor Instance { get; } = new PredicateConjunctionVisitor();

            /// <inheritdoc/>
            public override void Visit(Sentence sentence, HashSet<Predicate> predicates)
            {
                if (sentence is Conjunction conjunction)
                {
                    base.Visit(conjunction, predicates);
                }
                else if (sentence is Predicate predicate)
                {
                    // TODO: check for functions and throw..?

                    if (predicate.Arguments.Any(a => !a.IsGroundTerm))
                    {
                        throw new ArgumentException("States cannot include non-ground terms");
                    }

                    predicates.Add(predicate);
                }
                else
                {
                    throw new ArgumentException("States must be a conjunction of predicates");
                }
            }
        }
    }
}
