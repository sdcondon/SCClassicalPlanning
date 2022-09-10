using SCAutomatedPlanning.Classical;
using static SCAutomatedPlanning.Classical.OperableStateFactory;
using Action = SCAutomatedPlanning.Classical.Action;

namespace SCAutomatedPlanning.ExampleProblems.Classical
{
    public class BlocksWorld
    {
        private static OperableAtom On(Variable above, Variable below) => new Atom(nameof(On), above, below);

        private static OperableAtom Block(Variable block) => new Atom(nameof(Block), block);

        private static OperableAtom Clear(Variable surface) => new Atom(nameof(Clear), surface);

        private static OperableAtom Equal(Variable x, Variable y) => new Atom(nameof(Equal), x, y);

        private static readonly Variable Table = new(nameof(Table));
        private static readonly Variable A = new(nameof(A));
        private static readonly Variable B = new(nameof(B));
        private static readonly Variable C = new(nameof(C));

        private static readonly Variable b = new(nameof(b));
        private static readonly Variable x = new(nameof(x));
        private static readonly Variable y = new(nameof(y));

        public static Problem Problem { get; } = new Problem(
            initialState: On(A, Table) & On(B, Table) & On(C, A) & Block(A) & Block(B) & Block(C) & Clear(B) & Clear(C),
            goalState: On(A, B) & On(B, C),
            availableActions: new Action[]
            {
                new(
                    symbol: "Move",
                    precondition: On(b, x) & Clear(b) & Clear(y) & Block(b) & Block(y) & !Equal(b, x) & !Equal(b, y) & !Equal(x, y),
                    effect: On(b, y) & Clear(x) & !On(b, x) & !Clear(y)),

                new(
                    symbol: "MoveToTable",
                    precondition: On(b, x) & Clear(b) & Block(b) & !Equal(b, x),
                    effect: On(b, Table) & Clear(x) & !On(b, x)),
            });
    }
}