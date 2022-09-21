using SCClassicalPlanningAlternatives.UsingSCFirstOrderLogic.ProblemManipulation;
using SCFirstOrderLogic;
using System.Collections.ObjectModel;

namespace SCClassicalPlanningAlternatives.UsingSCFirstOrderLogic
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
        /// <param name="goalState">the goal state of the problem.</param>
        public Problem(Domain domain, IList<Constant> objects, State initialState, State goalState)
        {
            Domain = domain;
            Objects = new ReadOnlyCollection<Constant>(objects);
            InitialState = initialState;
            GoalState = goalState;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Problem"/> class, in which the objects that exist are inferred from the variables that are present in the initial and goal states.
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="initialState"></param>
        /// <param name="goalState"></param>
        public Problem(Domain domain, State initialState, State goalState)
        {
            Domain = domain;
            InitialState = initialState;
            GoalState = goalState;

            var variableFinder = new ConstantFinder();
            var variables = new HashSet<Constant>();
            variableFinder.Visit(initialState, variables);
            variableFinder.Visit(goalState, variables);
            Objects = new ReadOnlyCollection<Constant>(variables.ToArray());
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
        /// Gets the initial state of the problem.
        /// </summary>
        public State GoalState { get; }

        private class ConstantFinder : RecursiveStateVisitor<HashSet<Constant>>
        {
            public override void Visit(Constant constant, HashSet<Constant> constants) => constants.Add(constant);
        }
    }
}
