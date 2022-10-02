using SCFirstOrderLogic;

namespace SCClassicalPlanning.ProblemManipulation
{
    /// <summary>
    /// Interface for transformations of <see cref="Goal"/> instances.
    /// <para/>
    /// NB: Essentially an interface for visitors with a return value.
    /// </summary>
    /// <typeparam name="TOut">The type that the transformation transforms the goal to.</typeparam>
    public interface IGoalTransformation<out TOut>
    {
        /// <summary>
        /// Applies the transformation to a <see cref="Goal"/> instance.
        /// </summary>
        /// <param name="goal">The goal to transform.</param>
        TOut ApplyTo(Goal goal);
    }
}
