using SCAutomatedPlanning.Classical;
using Action = SCAutomatedPlanning.Classical.Action;
using static SCAutomatedPlanning.Classical.OperableStateFactory;

namespace SCAutomatedPlanning.ExampleProblems.Classical
{
    internal class SpareTire
    {
        private static OperableAtom IsTire(Variable variable) => new Atom(nameof(IsTire), variable);

        private static OperableAtom IsAt(Variable item, Variable location) => new Atom(nameof(IsAt), item, location);

        private static readonly Variable Spare = new(nameof(Spare));
        private static readonly Variable Flat = new(nameof(Spare));
        private static readonly Variable Axle = new(nameof(Axle));
        private static readonly Variable Trunk = new(nameof(Trunk));
        private static readonly Variable Ground = new(nameof(Ground));

        private static readonly Variable obj = new(nameof(obj));
        private static readonly Variable loc = new(nameof(loc));
        private static readonly Variable t = new(nameof(t));

        public static Problem Problem { get; } = new Problem(
            initialState: IsTire(Spare) & IsTire(Flat) & IsAt(Flat, Axle) & IsAt(Spare, Trunk),
            goalState: IsAt(Spare, Axle),
            availableActions: new Action[]
            {
                new(
                    symbol: "Remove",
                    precondition: IsAt(obj, loc),
                    effect: !IsAt(obj, loc) & IsAt(obj, Ground)),

                new(
                    symbol: "PutOn",
                    precondition: IsTire(t) & IsAt(t, Ground) & !IsAt(Flat, Axle),
                    effect: !IsAt(t, Ground) & IsAt(t, Axle)),

                new(
                    symbol: "LeaveOvernight",
                    precondition: new(),
                    effect: !IsAt(Spare, Ground) & !IsAt(Spare, Axle) & !IsAt(Spare, Trunk) & !IsAt(Flat, Ground) & !IsAt(Flat, Axle) & !IsAt(Flat, Trunk)),
            });
    }
}
