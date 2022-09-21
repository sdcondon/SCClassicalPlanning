using System.Collections.ObjectModel;

namespace SCClassicalPlanningAlternatives.WithAbstraction.ProblemCreation
{
    /// <summary>
    /// Shorthand factory methods for <see cref="IState"/> instances.
    /// </summary>
    public class OperableProblemFactory
    {
        public static Variable A { get; } = new Variable(nameof(A));
        public static Variable B { get; } = new Variable(nameof(B));
        public static Variable C { get; } = new Variable(nameof(C));
        public static Variable D { get; } = new Variable(nameof(D));
        public static Variable E { get; } = new Variable(nameof(E));
        public static Variable F { get; } = new Variable(nameof(F));
        public static Variable G { get; } = new Variable(nameof(G));
        public static Variable H { get; } = new Variable(nameof(H));
        public static Variable I { get; } = new Variable(nameof(I));
        public static Variable J { get; } = new Variable(nameof(J));
        public static Variable K { get; } = new Variable(nameof(K));
        public static Variable L { get; } = new Variable(nameof(L));
        public static Variable M { get; } = new Variable(nameof(M));
        public static Variable N { get; } = new Variable(nameof(N));
        public static Variable O { get; } = new Variable(nameof(O));
        public static Variable P { get; } = new Variable(nameof(P));
        public static Variable Q { get; } = new Variable(nameof(Q));
        public static Variable R { get; } = new Variable(nameof(R));
        public static Variable S { get; } = new Variable(nameof(S));
        public static Variable T { get; } = new Variable(nameof(T));
        public static Variable U { get; } = new Variable(nameof(U));
        public static Variable V { get; } = new Variable(nameof(V));
        public static Variable W { get; } = new Variable(nameof(W));
        public static Variable X { get; } = new Variable(nameof(X));
        public static Variable Y { get; } = new Variable(nameof(Y));
        public static Variable Z { get; } = new Variable(nameof(Z));

        public static Variable Var(object identifier) => new Variable(identifier);

        /// <summary>
        /// Surrogate class for <see cref="State"/> that defines a &amp; operators for conjuncting additional atoms. Implicitly convertible
        /// from and to <see cref="State"/> instances.
        /// </summary>
        public class State : IState
        {
            public State(IList<Literal> elements) => Elements = new ReadOnlyCollection<Literal>(elements);

            public State(params Literal[] elements) : this((IList<Literal>)elements) { }

            /// <inheritdoc />
            IReadOnlyCollection<ILiteral> IState.Elements => Elements;

            public IReadOnlyCollection<Literal> Elements { get; }

            public static State operator &(State state, Literal atom) => new State(state.Elements.Append(atom).ToArray());

            public static State operator &(Literal atom, State state) => new State(state.Elements.Append(atom).ToArray());

            public static State operator &(State state, Predicate predicate) => new State(state.Elements.Append(new Literal(false, predicate)).ToArray());

            public static State operator &(Predicate predicate, State state) => new State(state.Elements.Append(new Literal(false, predicate)).ToArray());

            public static implicit operator State(Literal literal) => new State(literal);
        }

        /// <summary>
        /// Surrogate class for <see cref="Literal"/> that defines ! and &amp; operators for negation and conjunction respectively. Implicitly convertible
        /// from and to an <see cref="Literal"/>, and to a <see cref="State"/> (consisting of just this atom).
        /// </summary>
        public class Literal : ILiteral
        {
            public Literal(bool isNegated, Predicate predicate) => (IsNegated, Predicate) = (IsNegated, Predicate);

            /// <inheritdoc />
            public bool IsNegated { get; }

            /// <inheritdoc />
            IPredicate ILiteral.Predicate => Predicate;

            public Predicate Predicate { get; }

            public static Literal operator !(Literal literal) => new Literal(!literal.IsNegated, literal.Predicate);

            public static State operator &(Literal left, Literal right) => new State(left, right);

            public static State operator &(Literal left, Predicate right) => new State(left, new Literal(false, right));

            public static State operator &(Predicate left, Literal right) => new State(new Literal(false, left), right);

            public static implicit operator Literal(Predicate predicate) => new Literal(false, predicate);
        }

        public class Predicate : IPredicate
        {
            public Predicate(object identifier, IList<Variable> arguments) => (Identifier, Arguments) = (identifier, new ReadOnlyCollection<Variable>(arguments));

            public Predicate(object identifier, params Variable[] arguments) : this(identifier, (IList<Variable>)arguments) { }

            /// <inheritdoc />
            public object Identifier { get; }

            /// <inheritdoc />
            public IReadOnlyCollection<IVariable> Arguments { get; }

            public static State operator &(Predicate left, Predicate right) => new State(new Literal(false, left), new Literal(false, right));

            public static Literal operator !(Predicate predicate) => new Literal(true, predicate);
        }

        public class Variable : IVariable
        {
            public Variable(object identifier) => Identifier = identifier;

            public object Identifier { get; }
        }

        public class Action : IAction
        {
            public Action(object identifier, IState precondition, IState effect) => (Identifier, Precondition, Effect) = (identifier, precondition, effect);

            public object Identifier { get; }

            public IState Precondition { get; }

            public IState Effect { get; }
        }

        public class Domain : IDomain
        {
            public Domain(IList<Predicate> predicates, IList<Action> actions)
            {
                Predicates = new ReadOnlyCollection<Predicate>(predicates);
                Actions = new ReadOnlyCollection<Action>(actions);
            }

            public IReadOnlyCollection<IPredicate> Predicates { get; }

            public IReadOnlyCollection<IAction> Actions { get; }
        }

        public class Problem : IProblem
        {
            public Problem(Domain domain, IList<Variable> objects, State initialState, State goalState)
            {
                Domain = domain;
                Objects = new ReadOnlyCollection<Variable>(objects);
                InitialState = initialState;
                GoalState = goalState;
            }

            public IDomain Domain { get; }

            public IReadOnlyCollection<IVariable> Objects { get; }

            public IState InitialState { get; }

            public IState GoalState { get; }
        }
    }
}
