using SCAutomatedPlanning.Classical;
using static SCAutomatedPlanning.Classical.StateCreation.OperableStateFactory;
using Action = SCAutomatedPlanning.Classical.Action;

namespace SCAutomatedPlanning.ExampleProblems.Classical
{
    /// <summary>
    /// The "Spare Tire" example from section 10.1.2 of "Artificial Intelligence: A Modern Approach".
    /// </summary>
    public class SpareTire
    {
        public static IEnumerable<Action> Actions => new Action[]
        {
            Remove(O, L),
            PutOn(T),
            LeaveOvernight(),
        };

        public static Variable Spare { get; } = new(nameof(Spare));
        public static Variable Flat { get; } = new(nameof(Spare));
        public static Variable Axle { get; } = new(nameof(Axle));
        public static Variable Trunk { get; } = new(nameof(Trunk));
        public static Variable Ground { get; } = new(nameof(Ground));

        /// <summary>
        /// Gets the implicit state of the world, that will never change as the result of actions.
        /// </summary>
        public static OperableState ImplicitState => IsTire(Spare) & IsTire(Flat);

        public static OperableAtom IsTire(Variable variable) => new Atom(nameof(IsTire), variable);

        public static OperableAtom IsAt(Variable item, Variable location) => new Atom(nameof(IsAt), item, location);

        public static Action Remove(Variable @object, Variable location) => new(
            symbol: nameof(Remove),
            precondition: IsAt(@object, location),
            effect: !IsAt(@object, location) & IsAt(@object, Ground));

        public static Action PutOn(Variable tire) => new(
            symbol: nameof(PutOn),
            precondition: IsTire(tire) & IsAt(tire, Ground) & !IsAt(Flat, Axle),
            effect: !IsAt(tire, Ground) & IsAt(tire, Axle));

        public static Action LeaveOvernight() => new(
            symbol: nameof(LeaveOvernight),
            precondition: new(),
            effect:
                !IsAt(Spare, Ground)
                & !IsAt(Spare, Axle)
                & !IsAt(Spare, Trunk)
                & !IsAt(Flat, Ground)
                & !IsAt(Flat, Axle)
                & !IsAt(Flat, Trunk));
    }
}
