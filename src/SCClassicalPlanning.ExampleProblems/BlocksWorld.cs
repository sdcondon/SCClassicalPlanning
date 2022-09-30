using SCFirstOrderLogic;
using static SCFirstOrderLogic.SentenceCreation.OperableSentenceFactory;

namespace SCClassicalPlanning.ExampleDomains
{
    /// <summary>
    /// The "Blocks World" example from section 10.1.3 of "Artificial Intelligence: A Modern Approach".
    /// </summary>
    public class BlocksWorld
    {
        /// <summary>
        /// Gets a <see cref="OwnFolModel.Domain"/ instance that encapsulates the "Blocks World" domain.
        /// </summary>
        public static Domain Domain { get; } = new Domain(
            predicates: new Predicate[]
            {
                Block(B),
                On(B, S),
                Clear(S),
                Equal(X, Y),
            },
            actions: new Action[]
            {
                Move(B, X, Y),
                MoveToTable(B, X),
            });

        public static Constant Table { get; } = new(nameof(Table));

        public static OperablePredicate On(Term above, Term below) => new Predicate(nameof(On), above, below);
        public static OperablePredicate Block(Term block) => new Predicate(nameof(Block), block);
        public static OperablePredicate Clear(Term surface) => new Predicate(nameof(Clear), surface);
        public static OperablePredicate Equal(Term x, Term y) => new Predicate(nameof(Equal), x, y);

        public static Action Move(Term block, Term from, Term toBlock) => new(
            identifier: nameof(Move),
            precondition: new Goal(
                On(block, from)
                & Clear(block)
                & Clear(toBlock)
                & Block(block)
                & Block(toBlock)
                & !Equal(block, from)
                & !Equal(block, toBlock)
                & !Equal(from, toBlock)),
            effect: new Effect(
                On(block, toBlock)
                & Clear(from)
                & !On(block, from)
                & !Clear(toBlock)));

        public static Action MoveToTable(Term block, Term from) => new(
            identifier: nameof(MoveToTable),
            precondition: new Goal(
                On(block, from)
                & Clear(block)
                & Block(block)
                & !Equal(block, from)),
            effect: new Effect(
                On(block, Table)
                & Clear(from)
                & !On(block, from)));
    }
}