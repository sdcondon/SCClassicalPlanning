using SCFirstOrderLogic;
using static SCClassicalPlanning.ProblemCreation.OperableProblemFactory;
using static SCFirstOrderLogic.SentenceCreation.OperableSentenceFactory;

namespace SCClassicalPlanning.ExampleDomains.FromAIaMA
{
    /// <summary>
    /// The "Spare Tire" example from section 10.1.2 of "Artificial Intelligence: A Modern Approach".
    /// </summary>
    public static class SpareTire
    {
        /// <summary>
        /// Gets a <see cref="SCClassicalPlanning.Domain"/ instance that encapsulates the "Spare Tire" domain.
        /// </summary>
        public static Domain Domain => new Domain(new Action[]
        {
            Remove(Var("object"), Var("location")),
            PutOn(Var("tire")),
            LeaveOvernight(),
        });

        public static Constant Spare { get; } = new(nameof(Spare));
        public static Constant Flat { get; } = new(nameof(Flat));
        public static Constant Ground { get; } = new(nameof(Ground));
        public static Constant Axle { get; } = new(nameof(Axle));
        public static Constant Trunk { get; } = new(nameof(Trunk));

        /// <summary>
        /// Gets the implicit state of the world, that will never change as the result of actions.
        /// </summary>
        public static OperableSentence ImplicitState => IsTire(Spare) & IsTire(Flat);

        public static OperablePredicate IsTire(Term tire) => new Predicate(nameof(IsTire), tire);

        public static OperablePredicate IsAt(Term item, Term location) => new Predicate(nameof(IsAt), item, location);

        public static Action Remove(Term @object, Term location) => new OperableAction(
            identifier: nameof(Remove),
            precondition: IsAt(@object, location),
            effect: !IsAt(@object, location) & IsAt(@object, Ground));

        public static Action PutOn(Term tire) => new OperableAction(
            identifier: nameof(PutOn),
            precondition: IsTire(tire) & IsAt(tire, Ground) & !IsAt(Flat, Axle),
            effect: !IsAt(tire, Ground) & IsAt(tire, Axle));

        public static Action LeaveOvernight() => new OperableAction(
            identifier: nameof(LeaveOvernight),
            precondition: Goal.Empty,
            effect:
                !IsAt(Spare, Ground)
                & !IsAt(Spare, Axle)
                & !IsAt(Spare, Trunk)
                & !IsAt(Flat, Ground)
                & !IsAt(Flat, Axle)
                & !IsAt(Flat, Trunk));
    }
}
