namespace SCClassicalPlanningAlternatives.WithAbstraction
{
    public interface IPredicate
    {
        public object Identifier { get; }

        public IReadOnlyCollection<IVariable> Arguments { get; }
    }
}
