namespace SCClassicalPlanning.Planning
{
    /// <summary>
    /// Interface for types encapsulating a plan of action.
    /// </summary>
    public static class IPlanExtensions
    {
        /// <summary>
        /// Applies "this" plan to a given state.
        /// </summary>
        public static State ApplyTo(this IPlan plan, State state)
        {
            foreach(var action in plan.Steps)
            {
                if (!action.IsApplicableTo(state))
                {

                }

                state = action.ApplyTo(state);
            }

            return state;
        }
    }
}
