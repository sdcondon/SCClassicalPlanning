using SCClassicalPlanning.ProblemManipulation;
using SCFirstOrderLogic;
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
        /// </summary>
        public IReadOnlyCollection<Constant> Objects { get; }

        /// <summary>
        /// Gets the initial state of the problem.
        /// </summary>
        public State InitialState { get; }

        /// <summary>
        /// Gets the goal of the problem.
        /// </summary>
        public Goal Goal { get; }

        /// <summary>
        /// Gets the available actions from a given state.
        /// <para/>
        /// Iterates all action templates, trying to unify (to a ground state - i.e. using known objects) the precondition of each with some subset of the current state.
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public IEnumerable<Action> GetAvailableActions(State state)
        {
            // todo
            throw new NotImplementedException();
        }

        public IEnumerable<Action> GetRelevantActions(Goal goal)
        {
            // todo
            throw new NotImplementedException();
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
