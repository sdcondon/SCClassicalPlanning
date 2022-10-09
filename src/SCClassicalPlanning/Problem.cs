using SCClassicalPlanning.ProblemManipulation;
using SCFirstOrderLogic;
using System.Collections.ObjectModel;

namespace SCClassicalPlanning
{
    /// <summary>
    /// Encapsulates a planning problem.
    /// <para/>
    /// Problems exist within a <see cref="SCClassicalPlanning.Domain"/>, and consist of an initial <see cref="State"/>, an end <see cref="SCClassicalPlanning.Goal"/>,
    /// and a set of objects (represented by <see cref="Constant"/>s from the SCFirstOrderLogic library) that exist within the scope of the problem.
    /// </summary>
    public class Problem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Problem"/> class.
        /// </summary>
        /// <param name="domain">The domain in which this problem resides.</param>
        /// <param name="initialState">The initial state of the problem.</param>
        /// <param name="goal">The goal of the problem.</param>
        /// <param name="objects">The objects that exist in this problem.</param>
        // TODO-SIGNIFICANT: Problematic design-wise.. Large? IO? Fairly big deal because could have significant impact.
        // Should Objects and InitialState ctor params be replaced with something like an 'IStateStore'? 
        // Perhaps also leave existing ctor that implicitly uses MemoryStateStore.
        // Explore this. Later.
        public Problem(Domain domain, State initialState, Goal goal, IList<Constant> objects)
        {
            Domain = domain;
            Objects = new ReadOnlyCollection<Constant>(objects);
            InitialState = initialState;
            Goal = goal;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Problem"/> class, in which the objects that exist are inferred from the constants that are present in the initial state and the goal.
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
        public ReadOnlyCollection<Constant> Objects { get; }

        /// <summary>
        /// Gets the initial state of the problem.
        /// </summary>
        public State InitialState { get; }

        /// <summary>
        /// Gets the goal of the problem.
        /// </summary>
        public Goal Goal { get; }

        /// <summary>
        /// Utility class to find <see cref="Constant"/> instances within the elements of a <see cref="State"/>, and add them to a given <see cref="HashSet{T}"/>.
        /// </summary>
        private class StateConstantFinder : RecursiveStateVisitor<HashSet<Constant>>
        {
            /// <summary>
            /// Gets a singleton instance of the <see cref="StateConstantFinder"/> class.
            /// </summary>
            public static StateConstantFinder Instance { get; } = new();

            /// <inheritdoc/>
            public override void Visit(Constant constant, HashSet<Constant> constants) => constants.Add(constant);
        }

        /// <summary>
        /// Utility class to find <see cref="Constant"/> instances within the elements of a <see cref="SCClassicalPlanning.Goal"/>, and add them to a given <see cref="HashSet{T}"/>.
        /// </summary>
        private class GoalConstantFinder : RecursiveGoalVisitor<HashSet<Constant>>
        {
            /// <summary>
            /// Gets a singleton instance of the <see cref="GoalConstantFinder"/> class.
            /// </summary>
            public static GoalConstantFinder Instance { get; } = new();

            /// <inheritdoc/>
            public override void Visit(Constant constant, HashSet<Constant> constants) => constants.Add(constant);
        }
    }
}
