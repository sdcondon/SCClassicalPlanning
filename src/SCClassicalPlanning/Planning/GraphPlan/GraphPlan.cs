namespace SCClassicalPlanning.Planning.GraphPlan
{
    public class GraphPlan : IPlanner
    {
        public Task<Plan> CreatePlanAsync(Problem problem, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
