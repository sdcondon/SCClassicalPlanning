﻿using SCFirstOrderLogic;
using System.Collections.Immutable;

namespace SCClassicalPlanning
{
    /// <summary>
    /// Container for a particular state of a domain. Instances occur as the initial state of <see cref="Problem"/> instances - and can
    /// also be used by planning algorithms to track intermediate states while looking for a solution to a problem.
    /// <para/>
    /// A state is essentially just a set of ground (i.e. variable-free), functionless predicates.
    /// <para/>
    /// TODO: probably should add some verification in ctor that all elements are ground and functionless.
    /// </summary>
    public sealed class State
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="State"/> class from an enumerable of the predicates that comprise it.
        /// </summary>
        /// <param name="elements">The predicates that comprise the state.</param>
        public State(IEnumerable<Predicate> elements)
        {
            // TODO-PERFORMANCE: this will be invariant under Effect application (as long as the effect isn't a badly created one..) - don't need to keep checking it.
            // Don't prematurely optimise though.. Will come back to this.
            if (elements.SelectMany(e => e.Arguments).Any(a => !a.IsGroundTerm))
            {
                throw new ArgumentException("States cannot include non-ground terms");
            }

            Elements = elements.ToImmutableHashSet();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="State"/> class from a (params) array of the predicates that comprise it.
        /// </summary>
        /// <param name="elements">The predicates that comprise the state.</param>
        public State(params Predicate[] elements) : this((IEnumerable<Predicate>)elements) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="State" /> class from a sentence of first order logic. The sentence must normalize to a conjunction of positive literals, or an exception will be thrown.
        /// </summary>
        /// <param name="sentence"></param>
        public State(Sentence sentence)
        {
            var elements = new HashSet<Predicate>();

            foreach (var clause in new CNFSentence(sentence).Clauses)
            {
                if (!clause.IsUnitClause)
                {
                    throw new ArgumentException("States must be expressable as a conjunction of literals");
                }

                var literal = clause.Literals.First();

                if (literal.IsNegated)
                {
                    // TODO?: Rather than throw, should we just ignore?
                    throw new ArgumentException("States make the closed-world assumption (any unmentioned fluent is false) - so negated predicates should just be omitted instead");
                }

                if (literal.Predicate.Arguments.Any(a => !a.IsGroundTerm))
                {
                    throw new ArgumentException("States cannot include non-ground terms");
                }

                elements.Add(literal.Predicate);
            }

            Elements = elements.ToImmutableHashSet();
        }

        /// <summary>
        /// Gets a singleton <see cref="State"/> instance that is empty.
        /// </summary>
        public static State Empty { get; } = new State();

        /// <summary>
        /// Gets the set of predicates that comprise this state.
        /// </summary>
        public IReadOnlySet<Predicate> Elements { get; }
    }
}
