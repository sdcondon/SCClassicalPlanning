// Copyright 2022-2024 Simon Condon
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
using SCClassicalPlanning.Planning.Utilities;
using SCFirstOrderLogic.SentenceManipulation;
using SCFirstOrderLogic.SentenceManipulation.VariableManipulation;
using System.Text;

namespace SCClassicalPlanning.Planning;

/// <summary>
/// Formatting logic for plans (and actions).
/// </summary>
public class PlanFormatter
{
    private readonly Problem problem;

    /// <summary>
    /// Initialises a new instance of the <see cref="PlanFormatter"/> class.
    /// </summary>
    /// <param name="problem">The problem of the plans that will be formatted by this instance. Used to establish succinct output for individual actions.</param>
    public PlanFormatter(Problem problem) => this.problem = problem;

    /// <summary>
    /// Creates a human-readable string representation of a given plan.
    /// </summary>
    /// <param name="plan">The plan to format.</param>
    /// <returns>A string representing the given plan. Each line of the output describes the next action in the plan.</returns>
    public string Format(Plan plan)
    {
        var builder = new StringBuilder();

        foreach (var step in plan.Steps)
        {
            builder.AppendLine(Format(step));
        }

        return builder.ToString();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    public string Format(Action action)
    {
        VariableSubstitution substitution = ProblemInspector.GetMappingFromSchema(action, problem.ActionSchemas, out var extraPreconditions);
        var orderedBindings = substitution.Bindings.OrderBy(b => b.Key.Identifier.ToString());
        var formattedBindings = $"{action.Identifier}({string.Join(", ", orderedBindings.Select(b => b.Key.ToString() + ": " + b.Value.ToString()))})";

        if (extraPreconditions.Any())
        {
            return $"{formattedBindings} where {string.Join(" ∧ ", extraPreconditions)}";
        }
        else
        {
            return formattedBindings;
        }
    }
}
