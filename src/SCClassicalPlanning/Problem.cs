using SCClassicalPlanning.ProblemManipulation;
using SCFirstOrderLogic;
using SCFirstOrderLogic.SentenceManipulation;
using SCFirstOrderLogic.SentenceManipulation.Unification;
using System.Collections.ObjectModel;

namespace SCClassicalPlanning
{
    /// <summary>
    /// Encapsulates a planning problem.
    /// </summary>
    public sealed class Problem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Problem"/> class.
        /// </summary>
        /// <param name="domain">The domain in which this problem resides.</param>
        /// <param name="objects">The objects that exist in this problem.</param>
        /// <param name="initialState">The initial state of the problem.</param>
        /// <param name="goal">The goal of the problem.</param>
        public Problem(Domain domain, IList<Constant> objects, State initialState, Goal goal)
        {
            Domain = domain;
            Objects = new ReadOnlyCollection<Constant>(objects);
            InitialState = initialState;
            Goal = goal;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Problem"/> class, in which the objects that exist are inferred from the constants that are present in the initial and goal states.
        /// </summary>
        /// <param name="domain">The domain in which this problem resides.</param>
        /// <param name="initialState">The initial state of the problem.</param>
        /// <param name="goal">The goal of the problem.</param>
        public Problem(Domain domain, State initialState, Goal goal)
        {
            Domain = domain;
            InitialState = initialState;
            Goal = goal;

            var constants = new HashSet<Constant>();
            StateConstantFinder.Instance.Visit(initialState, constants);
            GoalConstantFinder.Instance.Visit(goal, constants);
            Objects = new ReadOnlyCollection<Constant>(constants.ToArray());
        }

        /// <summary>
        /// Gets the domain in which this problem resides.
        /// </summary>
        public Domain Domain { get; }

        /// <summary>
        /// Gets the objects that exist in the problem.
        /// <para/>
        /// TODO-SIGNIFICANT: Problematic design-wise.. Large? IO? Fairly big deal because could have significant impact.
        /// Should Objects and InitialState be replaced with something like an 'IStateStore'? Which would perhaps mean that State should be IState?
        /// And everything that entails - Effect.ApplyTo(Effect) probably becomes IState.ApplY(Effect) and so on. Explore this. Later.
        /// </summary>
        public IReadOnlyCollection<Constant> Objects { get; }

        /// <summary>
        /// Gets the initial state of the problem.
        /// <para/>
        /// TODO-SIGNIFICANT: Problematic design-wise.. Large? IO? Fairly big deal because could have significant impact.
        /// Should Objects and InitialState be replaced with something like an 'IStateStore'? Which would perhaps mean that State should be IState?
        /// And everything that entails - Effect.ApplyTo(Effect) probably becomes IState.Apply(Effect) and so on. Explore this. Later.
        /// </summary>
        public State InitialState { get; }

        /// <summary>
        /// Gets the goal of the problem.
        /// </summary>
        public Goal Goal { get; }

        /// <summary>
        /// Gets the (ground) actions that are applicable from a given state.
        /// </summary>
        /// <param name="state">The state to retrieve the applicable actions for.</param>
        /// <returns>The actions that are applicable from the given state.</returns>
        public IEnumerable<Action> GetApplicableActions(State state)
        {
            foreach (var actionTemplate in Domain.Actions)
            {
                // NB: no specific element ordering here - just look at them in the order they happen to fall.
                // In a production scenario, we'd at least TRY to order the precondition conjuncts in a way that minimises
                // the amount of work we have to do. And this is where we'd do it.
                var orderedElements = actionTemplate.Precondition.PositiveElements.Concat(actionTemplate.Precondition.NegativeElements);

                foreach (var substitution in MatchWithKnownObjects(state, actionTemplate.Precondition.Elements, new VariableSubstitution()))
                {
                    // TODO: transform..
                    yield return new Action(
                        actionTemplate.Identifier,
                        actionTemplate.Precondition,
                        actionTemplate.Effect);
                }
            }
        }

        /// <summary>
        /// Gets the (ground) actions that are relevant to a given goal.
        /// </summary>
        /// <param name="state">The goal to retrieve the relevant actions for.</param>
        /// <returns>The actions that are relevant to the given state.</returns>
        public IEnumerable<Action> GetRelevantActions(Goal goal)
        {
            // todo
            throw new NotImplementedException();
        }

        private IEnumerable<VariableSubstitution> MatchWithKnownObjects(State state, IEnumerable<Literal> goalElements, VariableSubstitution unifier)
        {
            if (!goalElements.Any())
            {
                yield return unifier;
            }
            else
            {
                var firstGoalElement = goalElements.First();

                if (firstGoalElement.IsPositive)
                {
                    // Here we just iterate through ALL elements of the state, trying to find something that unifies with the first element of the goal (that is positive).
                    // We'd use an index here in anything approaching a production scenario:
                    foreach (var stateElement in state.Elements)
                    {
                        var firstGoalElementUnifier = new VariableSubstitution(unifier);

                        if (LiteralUnifier.TryUpdateUnsafe(stateElement, goalElements.First(), firstGoalElementUnifier))
                        {
                            foreach (var restOfGoalElementsUnifier in MatchWithKnownObjects(state, goalElements.Skip(1), firstGoalElementUnifier))
                            {
                                yield return restOfGoalElementsUnifier;
                            }
                        }
                    }
                }
                else
                {
                    // ...
                }
            }
        }

        private class StateConstantFinder : RecursiveStateVisitor<HashSet<Constant>>
        {
            public static StateConstantFinder Instance { get; } = new();

            public override void Visit(Constant constant, HashSet<Constant> constants) => constants.Add(constant);
        }

        private class GoalConstantFinder : RecursiveGoalVisitor<HashSet<Constant>>
        {
            public static GoalConstantFinder Instance { get; } = new();

            public override void Visit(Constant constant, HashSet<Constant> constants) => constants.Add(constant);
        }
    }
}
