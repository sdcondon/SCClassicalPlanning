namespace SCClassicalPlanningAlternatives.WithAbstraction
{
    public interface ILiteral
    {
        public bool IsNegated { get; }

        public IPredicate Predicate { get; }
    }
}
