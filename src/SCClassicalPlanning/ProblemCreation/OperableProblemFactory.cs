// Copyright 2022-2023 Simon Condon
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
using SCFirstOrderLogic;
using SCFirstOrderLogic.SentenceManipulation;
using System.Collections.Immutable;
using static SCFirstOrderLogic.SentenceCreation.OperableSentenceFactory;

namespace SCClassicalPlanning.ProblemCreation
{
    /// <summary>
    /// <para>
    /// Utility logic for the declaration of <see cref="Problem"/> instances, prioritising succinct C# above all else.
    /// </para>
    /// <para>
    /// Allows for declaring actions that have preconditions and effects stated directly as <see cref="OperableSentence"/> instances.
    /// Which, err, really doesn't make much of a difference, to be honest - there's not nearly as much value in this as there is in the equivalent functionality in SCFirstOrderLogic.
    /// That's why it's not mentioned in the user guide.
    /// </para>
    /// </summary>
    public static class OperableProblemFactory
    {
        /// <summary>
        /// Surrogate type for <see cref="Action"/> that uses <see cref="OperableGoal"/> and <see cref="OperableEffect"/>. Implicitly convertible to and from <see cref="Action"/>.
        /// </summary>
        public class OperableAction
        {
            /// <summary>
            /// Initialises a new instance of the <see cref="OperableAction"/> class.
            /// </summary>
            /// <param name="identifier">The unique identifier of the action.</param>
            /// <param name="precondition">The precondition of the action.</param>
            /// <param name="effect">The effect of the action.</param>
            public OperableAction(object identifier, OperableGoal precondition, OperableEffect effect) => (Identifier, Precondition, Effect) = (identifier, precondition, effect);

            internal object Identifier { get; }

            internal OperableGoal Precondition { get; }

            internal OperableEffect Effect { get; }

            /// <summary>
            /// Defines the implicit conversion of an <see cref="Action"/> instance to an <see cref="OperableAction"/>.
            /// </summary>
            /// <param name="action">The <see cref="Action"/> to convert.</param>
            public static implicit operator OperableAction(Action action) => new(action.Identifier, action.Precondition, action.Effect);

            /// <summary>
            /// Defines the implicit conversion of an <see cref="OperableAction"/> instance to an <see cref="Action"/>.
            /// </summary>
            /// <param name="action">The <see cref="OperableAction"/> to convert.</param>
            public static implicit operator Action(OperableAction action) => new(action.Identifier, action.Precondition, action.Effect);
        }

        /// <summary>
        /// Surrogate type for <see cref="Goal"/> that is implictly convertible from an <see cref="OperableSentence"/>. Also implicitly convertible to and from <see cref="Goal"/>.
        /// </summary>
        public class OperableGoal
        {
            internal OperableGoal(IEnumerable<Literal> elements) => Elements = elements.ToImmutableHashSet();

            internal IReadOnlySet<Literal> Elements { get; }

            /// <summary>
            /// Defines the implicit conversion of an <see cref="Goal"/> instance to an <see cref="OperableGoal"/>.
            /// </summary>
            /// <param name="goal">The <see cref="Goal"/> to convert.</param>
            public static implicit operator OperableGoal(Goal goal) => new(goal.Elements);

            /// <summary>
            /// Defines the implicit conversion of an <see cref="OperableGoal"/> instance to an <see cref="Goal"/>.
            /// </summary>
            /// <param name="goal">The <see cref="OperableGoal"/> to convert.</param>
            public static implicit operator Goal(OperableGoal goal) => new(goal.Elements);

            /// <summary>
            /// Defines the implicit conversion of an <see cref="OperableSentence"/> instance to an <see cref="OperableGoal"/>.
            /// </summary>
            /// <param name="sentence">The <see cref="Sentence"/> to convert.</param>
            public static implicit operator OperableGoal(OperableSentence sentence)
            {
                var literals = new HashSet<Literal>();
                LiteralConjunctionVisitor.Instance.Visit(sentence, literals);
                return new(literals);
            }
        }

        /// <summary>
        /// Surrogate type for <see cref="State"/> that is implictly convertible from an <see cref="OperableSentence"/>. Also implicitly convertible to and from <see cref="State"/>.
        /// </summary>
        public class OperableState
        {
            internal OperableState(IEnumerable<Predicate> elements) => Elements = elements.ToImmutableHashSet();

            internal IReadOnlySet<Predicate> Elements { get; }

            /// <summary>
            /// Defines the implicit conversion of an <see cref="State"/> instance to an <see cref="OperableState"/>.
            /// </summary>
            /// <param name="state">The <see cref="State"/> to convert.</param>
            public static implicit operator OperableState(State state) => new(state.Elements);

            /// <summary>
            /// Defines the implicit conversion of an <see cref="OperableState"/> instance to an <see cref="State"/>.
            /// </summary>
            /// <param name="state">The <see cref="OperableState"/> to convert.</param>
            public static implicit operator State(OperableState state) => new(state.Elements);

            /// <summary>
            /// Defines the implicit conversion of an <see cref="OperableSentence"/> instance to an <see cref="OperableState"/>.
            /// </summary>
            /// <param name="sentence">The <see cref="Sentence"/> to convert.</param>
            public static implicit operator OperableState(OperableSentence sentence)
            {
                var predicates = new HashSet<Predicate>();
                PredicateConjunctionVisitor.Instance.Visit(sentence, predicates);
                return new(predicates);
            }
        }

        /// <summary>
        /// Surrogate type for <see cref="Effect"/> that is implictly convertible from an <see cref="OperableSentence"/>. Also implicitly convertible to and from <see cref="Effect"/>.
        /// </summary>
        public class OperableEffect
        {
            internal OperableEffect(IEnumerable<Literal> elements) => Elements = elements.ToImmutableHashSet();

            internal IReadOnlySet<Literal> Elements { get; }

            /// <summary>
            /// Defines the implicit conversion of an <see cref="Effect"/> instance to an <see cref="OperableEffect"/>.
            /// </summary>
            /// <param name="effect">The <see cref="Effect"/> to convert.</param>
            public static implicit operator OperableEffect(Effect effect) => new(effect.Elements);

            /// <summary>
            /// Defines the implicit conversion of an <see cref="OperableEffect"/> instance to an <see cref="Effect"/>.
            /// </summary>
            /// <param name="effect">The <see cref="OperableEffect"/> to convert.</param>
            public static implicit operator Effect(OperableEffect effect) => new(effect.Elements);

            /// <summary>
            /// Defines the implicit conversion of an <see cref="OperableSentence"/> instance to an <see cref="OperableEffect"/>.
            /// </summary>
            /// <param name="sentence">The <see cref="Sentence"/> to convert.</param>
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
