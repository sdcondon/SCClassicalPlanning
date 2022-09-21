namespace SCClassicalPlanningAlternatives.UsingSCFirstOrderLogic
{
    public sealed class Action
    {
        public Action(object identifier, State precondition, State effect) => (Identifier, Precondition, Effect) = (identifier, precondition, effect);

        public object Identifier { get; }

        public State Precondition { get; }

        public State Effect { get; }
    }
}
