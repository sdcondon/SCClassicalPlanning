using SCFirstOrderLogic;

namespace SCClassicalPlanning.ProblemManipulation
{
    /// <summary>
    /// Interface for visitors of <see cref="Effect"/> instances.
    /// </summary>
    /// <typeparam name="TState">The type of state that this visitor works with.</typeparam>
    public interface IEffectVisitor<TState>
    {
        /// <summary>
        /// Visits a <see cref="Effect"/> instance.
        /// </summary>
        /// <param name="effect">The <see cref="Effect"/> instance to visit.</param>
        /// <param name="visitationState">A reference to the state of this visitation.</param>
        void Visit(Effect effect, TState visitationState);
    }
}
