// Copyright 2022-2023 Simon Condon
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
using SCFirstOrderLogic;
using SCFirstOrderLogic.SentenceManipulation;
using SCFirstOrderLogic.SentenceManipulation.Unification;

namespace SCClassicalPlanning.Alternatives.Planning.Utilities;

/// <summary>
/// Just includes a version of ProblemInspector.GetRelevantSchemaSubstitutions that attempts to be more efficient,
/// but unfortunately doesn't quite do the job. Worth keeping around for pondering, though.
/// </summary>
public static class ProblemInspector_MergedUnmatchLogic
{
    /// <summary>
    /// Gets the (action schema, *ground* variable substitution) pairings that represent actions that are relevant to a given goal in a given problem.
    /// </summary>
    /// <param name="problem">The problem being solved.</param>
    /// <param name="goal">The goal to retrieve the relevant actions for.</param>
    /// <returns>The actions that are relevant to the given state.</returns>
    public static IEnumerable<(Action schema, VariableSubstitution substitution)> GetRelevantSchemaSubstitutions(Problem problem, Goal goal)
    {
        // Local method to create variable subsitutions such that the negation of the effects elements transformed by the substitution do not match any of the goal's elements.
        // effectElements: The (remaining) elements of the effect to be matched.
        // returns: An enumerable of VariableSubstitutions that can be applied to the effect elements to make none of them match the negation of a goal element
        IEnumerable<VariableSubstitution> UnmatchWithGoalNegation(IEnumerable<Literal> effectElements, VariableSubstitution unifier)
        {
            if (!effectElements.Any())
            {
                yield return unifier;
            }
            else
            {
                var firstEffectElement = effectElements.First();

                // if the current unifier applied to the element matches - its a nono
                // if *no* unifiers exist with any goal elements, we don't need to constrain and can recurse
                // if unifiers do exist, we need to constrain.
                if (goal.Elements.Any(e => unifier.ApplyTo(firstEffectElement).Negate().Equals(e)))
                {
                    // This effect element transformed by the unifier so far then negated,
                    // exactly matches with an element of the goal. Discard this unifier.
                    yield break;
                }
                else if (goal.Elements.Any(e => LiteralUnifier.TryUpdateUnsafe(e, unifier.ApplyTo(firstEffectElement).Negate(), new VariableSubstitution(unifier))))
                {
                    // There exists a unifier such that this effect element transformed by the unifier so far then negated,
                    // exactly matches with an element of the goal. Need to explode out into all possible substitutions.
                    // Potentially SLOOW for large domains. Would need to be able to track constraints back through the problem to have this work otherwise.

                    // We need to check for the existence of the negation of the literal formed by substituting EVERY combination of
                    // objects in the problem for the as yet unbound variables. This is obviously VERY expensive for large problems with lots of objects -
                    // though I guess clever indexing could help (support for indexing is TODO).
                    IEnumerable<VariableSubstitution> allPossibleUnifiers = new List<VariableSubstitution>() { unifier };
                    var unboundVariables = firstEffectElement.Predicate.Arguments.OfType<VariableReference>().Except(unifier.Bindings.Keys);
                    foreach (var unboundVariable in unboundVariables)
                    {
                        allPossibleUnifiers = allPossibleUnifiers.SelectMany(u => problem.Constants.Select(o =>
                        {
                            var newBindings = new Dictionary<VariableReference, Term>(u.Bindings);
                            newBindings[unboundVariable] = o;
                            return new VariableSubstitution(newBindings);
                        }));
                    }

                    foreach (var firstEffectElementUnifier in allPossibleUnifiers)
                    {
                        foreach (var restOfGoalElementsUnifier in UnmatchWithGoalNegation(effectElements.Skip(1), firstEffectElementUnifier))
                        {
                            yield return restOfGoalElementsUnifier;
                        }
                    }
                }
                else
                {
                    // There's no substitution that'll make this element match an element of the goal -
                    // its safe, so we can just move on to the next element of the effect
                    foreach (var substitution in UnmatchWithGoalNegation(effectElements.Skip(1), unifier))
                    {
                        yield return substitution;
                    }
                }
            }
        }

        // Local method to create variable subsitutions such that the effects elements transformed by the substitution contain at least one match to the goal's elements.
        // effectElements: The elements of the effect to be matched.
        // returns: An enumerable of VariableSubstitutions that can be applied to the effect elements to make at least one of the match a goal element
        IEnumerable<VariableSubstitution> MatchWithGoal(IEnumerable<Literal> effectElements)
        {
            foreach (var effectElement in effectElements)
            {
                // Here we iterate through ALL elements of the goal, trying to find unifications with the effect element.
                // Using some kind of index here would of course speed things up (support for this is a TODO).
                // We return each unification we find immediately - for an effect to be relevant it only needs to match at least one element of the goal.
                foreach (var goalElement in goal.Elements)
                {
                    // TODO: using LiteralUnifier is perhaps overkill given that we know we're functionless,
                    // but will do for now. (doesn't necessarily cost more..)
                    if (LiteralUnifier.TryCreate(goalElement, effectElement, out var unifier))
                    {
                        yield return unifier;
                    }
                }
            }
        }

        foreach (var schema in problem.Domain.Actions)
        {
            foreach (var potentialSubsitution in MatchWithGoal(schema.Effect.Elements))
            {
                foreach (var substitution in UnmatchWithGoalNegation(schema.Effect.Elements, potentialSubsitution))
                {
                    yield return (schema, substitution);
                }
            }
        }
    }
}
