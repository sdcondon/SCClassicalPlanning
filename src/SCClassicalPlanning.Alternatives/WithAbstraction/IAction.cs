namespace SCClassicalPlanningAlternatives.WithAbstraction
{
    public interface IAction
    {
        public object Identifier { get; }

        public IState Precondition { get; }

        public IState Effect { get; }
    }
}
