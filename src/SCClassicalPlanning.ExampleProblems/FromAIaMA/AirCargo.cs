using SCFirstOrderLogic;
using static SCFirstOrderLogic.SentenceCreation.OperableSentenceFactory;
using static SCClassicalPlanning.ProblemCreation.OperableProblemFactory;

namespace SCClassicalPlanning.ExampleDomains.FromAIaMA
{
    /// <summary>
    /// The "Air Cargo" example from section 10.1.1 of "Artificial Intelligence: A Modern Approach".
    /// </summary>
    public static class AirCargo
    {
        static AirCargo()
        {
            Domain = MakeDomain();

            Constant cargo1 = new(nameof(cargo1));
            Constant cargo2 = new(nameof(cargo2));
            Constant plane1 = new(nameof(plane1));
            Constant plane2 = new(nameof(plane2));
            Constant airport1 = new(nameof(airport1));
            Constant airport2 = new(nameof(airport2));

            ExampleProblem = MakeProblem(
                initialState: new(
                    Cargo(cargo1)
                    & Cargo(cargo2)
                    & Plane(plane1)
                    & Plane(plane2)
                    & Airport(airport1)
                    & Airport(airport2)
                    & At(cargo1, airport1)
                    & At(cargo2, airport2)
                    & At(plane1, airport1)
                    & At(plane2, airport2)),
                goal: new(
                    At(cargo2, airport1)
                    & At(cargo1, airport2)));
        }

        /// <summary>
        /// Gets a <see cref="SCClassicalPlanning.Domain"/ instance that encapsulates Air Cargo domain.
        /// </summary>
        public static Domain Domain { get; }

        /// <summary>
        /// Gets an instance of the customary example problem in this domain.
        /// Consists of two airports, two planes and two pieces of cargo.
        /// In the initial state, plane1 and cargo1 are at airport1; plane2 and cargo2 are at airport2.
        /// The goal is to get cargo2 to airport1 and cargo1 to airport2.
        /// </summary>
        public static Problem ExampleProblem { get; }

        public static OperablePredicate Cargo(Term cargo) => new Predicate(nameof(Cargo), cargo);
        public static OperablePredicate Plane(Term plane) => new Predicate(nameof(Plane), plane);
        public static OperablePredicate Airport(Term airport) => new Predicate(nameof(Airport), airport);
        public static OperablePredicate At(Term @object, Term location) => new Predicate(nameof(At), @object, location);
        public static OperablePredicate In(Term @object, Term container) => new Predicate(nameof(In), @object, container);
        ////public static OperablePredicate Equal(Term x, Term y) => new Predicate(EqualitySymbol.Instance, x, y);

        public static Action Load(Term cargo, Term plane, Term airport) => new OperableAction(
            identifier: nameof(Load),
            precondition:
                At(cargo, airport)
                & At(plane, airport)
                & Cargo(cargo)
                & Plane(plane)
                & Airport(airport),
            effect:
                !At(cargo, airport)
                & In(cargo, plane));

        public static Action Unload(Term cargo, Term plane, Term airport) => new OperableAction(
            identifier: nameof(Unload),
            precondition:
                In(cargo, plane)
                & At(plane, airport)
                & Cargo(cargo)
                & Plane(plane)
                & Airport(airport),
            effect:
                At(cargo, airport)
                & !In(cargo, plane));

        public static Action Fly(Term plane, Term from, Term to) => new OperableAction(
            identifier: nameof(Fly),
            precondition:
                At(plane, from)
                & Plane(plane)
                & Airport(from)
                & Airport(to),
                ////& !Equal(from, to),
            effect:
                !At(plane, from)
                & At(plane, to));

        /// <summary>
        /// Creates a new <see cref="Problem"/> instance that refers to this domain.
        /// </summary>
        /// <param name="initialState">The initial state of the problem.</param>
        /// <param name="goal">The initial state of the problem.</param>
        /// <returns>A new <see cref="Problem"/> instance that refers to this domain.</returns>
        public static Problem MakeProblem(State initialState, Goal goal) => new(Domain, initialState, goal);

        // NB: This is in its own method rather than the static ctor just so that we can run tests against domain construction.
        internal static Domain MakeDomain()
        {
            VariableDeclaration cargo = new(nameof(cargo));
            VariableDeclaration plane = new(nameof(plane));
            VariableDeclaration airport = new(nameof(airport));
            VariableDeclaration from = new(nameof(from));
            VariableDeclaration to = new(nameof(to));

            return new Domain(new Action[]
            {
                Load(cargo, plane, airport),
                Unload(cargo, plane, airport),
                Fly(plane, from, to),
            });
        }
    }
}