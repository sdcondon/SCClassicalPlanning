using global::SCClassicalPlanning.Planners;
using SCClassicalPlanning.Planners;
using SCFirstOrderLogic.Planning.Planners;
using SCFirstOrderLogic.SentenceFormatting;
using System.Text;

namespace SCClassicalPlanning.Formatting
{
    /// <summary>
    /// Formatting logic for plans (and actions).
    /// <para/>
    /// TODO: Add and use an actionSchemas ctor parameter (IEnumerable of Action) - used for variable names. Could of course instead have a dictionary
    /// prop within actions, but its tautologous and wasteful of resources given that we only want it for explanations. This approach gives us nicer
    /// separation of concerns.
    /// </summary>
    public class PlanFormatter
    {
        public string Format(IPlan plan)
        {
            var builder = new StringBuilder();

            foreach (var step in plan.Steps)
            {
                builder.AppendLine(Format(step));
            }

            return builder.ToString();
        }

        public string Format(Action action)
        {
            return $"{action.Symbol}: {string.Join(", ", action.Effect.Atoms.Select(a => sentenceFormatter.Format(a)))})";
        }
    }
}
