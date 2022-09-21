using static SCClassicalPlanning.ProblemCreation.CommonVariableDeclarations;

namespace SCClassicalPlanning.ExampleDomains
{
    /// <summary>
    /// The "Blocks World" example from section 10.1.3 of "Artificial Intelligence: A Modern Approach".
    /// </summary>
    public class BlocksWorld
    {
        /// <summary>
        /// Gets a <see cref="SCClassicalPlanning.Domain"/ instance that encapsulates the "Blocks World" domain.
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

        public static Variable Table { get; } = new(nameof(Table));

        public static Predicate On(Variable above, Variable below) => new(nameof(On), above, below);
        public static Predicate Block(Variable block) => new(nameof(Block), block);
        public static Predicate Clear(Variable surface) => new(nameof(Clear), surface);
        public static Predicate Equal(Variable x, Variable y) => new(nameof(Equal), x, y);

        public static Action Move(Variable block, Variable from, Variable toBlock) => new(
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

        public static Action MoveToTable(Variable block, Variable from) => new(
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