namespace SCAutomatedPlanning.Classical.Planners
{
    public interface IPlan
    {
        public IReadOnlyCollection<Action> Steps { get; } 
    }
}
