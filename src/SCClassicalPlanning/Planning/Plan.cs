using System.Collections.ObjectModel;

namespace SCClassicalPlanning.Planning
{
    /// <summary>
    /// Interface for types encapsulating a plan of action.
    /// </summary>
    public class Plan
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Plan"/> class.
        /// </summary>
        /// <param name="steps">The actions that comprise the plan.</param>
        public Plan(IList<Action> steps) => Steps = new ReadOnlyCollection<Action>(steps);

        /// <summary>
        /// Gets the steps of the plan.
        /// </summary>
        public IReadOnlyCollection<Action> Steps { get; }

        /// <summary>
        /// Applies this plan to a given state.
        /// </summary>
        public State ApplyTo(State state)
        {
            foreach (var action in Steps)
            {
                if (!action.IsApplicableTo(state))
                {
                    throw new ArgumentException("Invalid plan of action - current action is not applicable in the current state");
                }

                state = action.ApplyTo(state);
            }

            return state;
        }
    }
}
