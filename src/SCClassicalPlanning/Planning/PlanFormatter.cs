using SCFirstOrderLogic.SentenceFormatting;
using System.Text;

namespace SCClassicalPlanning.Planning
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
        private readonly SentenceFormatter sentenceFormatter = new SentenceFormatter();

        public string Format(Plan plan)
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
            return $"[{action.Identifier}] Effect: {string.Join(" & ", action.Effect.Elements.Select(e => sentenceFormatter.Format(e)))}";
        }

        public string Format(State state)
        {
            return $"{string.Join(" & ", state.Elements.Select(e => sentenceFormatter.Format(e)))}";
        }
    }
}
