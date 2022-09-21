namespace SCClassicalPlanningAlternatives.WithAbstraction
{
    public interface IState
    {
        IReadOnlyCollection<ILiteral> Elements { get; }
    }
}
