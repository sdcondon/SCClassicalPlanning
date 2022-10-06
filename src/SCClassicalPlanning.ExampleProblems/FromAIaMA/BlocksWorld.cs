﻿using SCFirstOrderLogic;
using static SCClassicalPlanning.ProblemCreation.OperableProblemFactory;
using static SCFirstOrderLogic.SentenceCreation.OperableSentenceFactory;

namespace SCClassicalPlanning.ExampleDomains.FromAIaMA
{
    /// <summary>
    /// The "Blocks World" example from section 10.1.3 of "Artificial Intelligence: A Modern Approach".
    /// </summary>
    public static class BlocksWorld
    {
        /// <summary>
        /// Gets a <see cref="SCClassicalPlanning.Domain"/ instance that encapsulates the "Blocks World" domain.
        /// </summary>
        public static Domain Domain => new Domain(
            actions: new Action[]
            {
                Move(B, X, Y),
                MoveToTable(B, X),
            },
            predicates: new Predicate[]
            {
                Block(B),
                On(B, S),
                Clear(S),
                Equal(X, Y),
            });

        public static Constant Table { get; } = new(nameof(Table));

        public static OperablePredicate On(Term above, Term below) => new Predicate(nameof(On), above, below);
        public static OperablePredicate Block(Term block) => new Predicate(nameof(Block), block);
        public static OperablePredicate Clear(Term surface) => new Predicate(nameof(Clear), surface);
        public static OperablePredicate Equal(Term x, Term y) => new Predicate(nameof(Equal), x, y);

        public static Action Move(Term block, Term from, Term toBlock) => new OperableAction(
            identifier: nameof(Move),
            precondition:
                On(block, from)
                & Clear(block)
                & Clear(toBlock)
                & Block(block)
                & Block(toBlock)
                & !Equal(block, from)
                & !Equal(block, toBlock)
                & !Equal(from, toBlock),
            effect:
                On(block, toBlock)
                & Clear(from)
                & !On(block, from)
                & !Clear(toBlock));

        public static Action MoveToTable(Term block, Term from) => new OperableAction(
            identifier: nameof(MoveToTable),
            precondition:
                On(block, from)
                & Clear(block)
                & Block(block)
                & !Equal(block, from),
            effect:
                On(block, Table)
                & Clear(from)
                & !On(block, from));
    }
}