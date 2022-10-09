using SCFirstOrderLogic.SentenceManipulation;
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
        private readonly Domain domain;

        public PlanFormatter(Domain domain) => this.domain = domain;

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
            VariableSubstitution substitution = DomainInspector.GetMappingFromSchema(domain, action);
            var orderedBindings = substitution.Bindings.OrderBy(b => b.Key.Symbol.ToString());
            return $"{action.Identifier}({string.Join(", ", orderedBindings.Select(b => b.Key.ToString() + ": " + b.Value.ToString()))})";
        }
    }
}
