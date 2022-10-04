﻿using SCFirstOrderLogic;
using SCFirstOrderLogic.SentenceManipulation;
using System.Collections.Immutable;

namespace SCClassicalPlanning
{
    /// <summary>
    /// Container for information about a state.
    /// <para/>
    /// A state is essentially just a set of ground (i.e. variable-free), functionless <see cref="Predicate"/>s. State instances occur as the initial state of <see cref="Problem"/>
    /// instances - and are also used by some planning algorithms to track intermediate states while looking for a solution to a problem.
    /// <para/>
    /// TODO: probably should add some verification in ctor that all elements are ground and functionless. Or.. not - don't want to sap performane by validating on
    /// every step in plan creation.. Best of both worlds would be nice.
    /// </summary>
    public class State
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="State"/> class from an enumerable of the predicates that comprise it.
        /// </summary>
        /// <param name="elements">The predicates that comprise the state.</param>
        public State(IEnumerable<Predicate> elements) => Elements = elements.ToImmutableHashSet();

        /// <summary>
        /// Initializes a new instance of the <see cref="State"/> class from a (params) array of the predicates that comprise it.
        /// </summary>
        /// <param name="elements">The predicates that comprise the state.</param>
        public State(params Predicate[] elements) : this((IEnumerable<Predicate>)elements) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="State"/> class from a sentence of first order logic.
        /// The sentence must normalize to a conjunction of positive literals, or an exception will be thrown.
        /// </summary>
        /// <param name="sentence"></param>
        public State(Sentence sentence)
        {
            var elements = new HashSet<Predicate>();
            StateConstructionVisitor.Instance.Visit(sentence, ref elements);
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

        /// <summary>
        /// Sentence visitor class that extracts <see cref="Predicate"/>s from a <see cref="Sentence"/> that is a conjunction of them.
        /// Used by the <see cref="State(Sentence)"/> constructor.
        /// </summary>
        private class StateConstructionVisitor : RecursiveSentenceVisitor<HashSet<Predicate>>
        {
            /// <summary>
            /// Gets a singleton instance of this class.
            /// </summary>
            public static StateConstructionVisitor Instance { get; } = new StateConstructionVisitor();

            /// <inheritdoc/>
            public override void Visit(Sentence sentence, ref HashSet<Predicate> predicates)
            {
                if (sentence is Conjunction conjunction)
                {
                    base.Visit(conjunction, ref predicates);
                }
                else if (sentence is Predicate predicate)
                {
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
