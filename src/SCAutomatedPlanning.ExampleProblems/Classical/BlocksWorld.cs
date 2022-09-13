using SCAutomatedPlanning.Classical;
using static SCAutomatedPlanning.Classical.StateCreation.OperableStateFactory;
using Action = SCAutomatedPlanning.Classical.Action;

namespace SCAutomatedPlanning.ExampleProblems.Classical
{
    /// <summary>
    /// The "Blocks World" example from section 10.1.3 of "Artificial Intelligence: A Modern Approach".
    /// </summary>
    public class BlocksWorld
    {
        /// <summary>
        /// Gets the available actions.
        /// </summary>
        public static IEnumerable<Action> Actions { get; } = new Action[]
        {
            Move(B, X, Y),
            MoveToTable(B, X),
        };

        public static Variable Table { get; } = new(nameof(Table));
        public static Variable BlockA { get; } = new(nameof(BlockA));
        public static Variable BlockB { get; } = new(nameof(BlockB));
        public static Variable BlockC { get; } = new(nameof(BlockC));

        /// <summary>
        /// Gets the implicit state of the world, that will never change as the result of actions.
        /// </summary>
        public static OperableState ImplicitState => Block(BlockA) & Block(BlockB) & Block(BlockC);

        public static OperableAtom On(Variable above, Variable below) => new Literal(nameof(On), above, below);
        public static OperableAtom Block(Variable block) => new Literal(nameof(Block), block);
        public static OperableAtom Clear(Variable surface) => new Literal(nameof(Clear), surface);
        public static OperableAtom Equal(Variable x, Variable y) => new Literal(nameof(Equal), x, y);

        public static Action Move(Variable block, Variable from, Variable toBlock) => new(
            symbol: nameof(Move),
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
            symbol: nameof(MoveToTable),
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