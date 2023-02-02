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
using SCClassicalPlanning.ProblemManipulation;
using SCFirstOrderLogic;
using SCFirstOrderLogic.SentenceManipulation;
using SCFirstOrderLogic.SentenceManipulation.Unification;
using System.Diagnostics.CodeAnalysis;

namespace SCClassicalPlanning.Planning.Utilities
{
    /// <summary>
    /// <para>
    /// Various domain inspection methods, useful to planners.
    /// </para>
    /// <para>
    /// TODO: consider making this instantiable - domain passed to ctor. Opens way for caching etc.
    /// </para>
    /// </summary>
    public static class DomainInspector
    {
        /// <summary>
        /// Gets the (action schema, variable substitution, variable constraints) tuples that represent actions that are relevant to a given goal in a given problem.
        /// </summary>
        /// <param name="domain">The domain of the problem being solved.</param>
        /// <param name="goal">The goal to retrieve the relevant actions for.</param>
        /// <returns>The actions that are relevant to the given state.</returns>
        public static IEnumerable<(Action schema, VariableSubstitution substitution, Goal constraints)> GetRelevantActionDetails(Domain domain, Goal goal)
        {
            // Local method to find any constraints that apply to a given substitution for none of the goal's elements to be negated.
            bool TryGetConstraints(IEnumerable<Literal> effectElements, VariableSubstitution substitution, [MaybeNullWhen(false)] out Goal constraints)
            {
                List<Literal> constraintElements = new();

                foreach (var effectElement in effectElements)
                {
                    foreach (var goalElement in goal.Elements)
                    {
                        var clashingSubstitution = new VariableSubstitution(substitution);

                        if (LiteralUnifier.TryUpdateUnsafe(goalElement, substitution.ApplyTo(effectElement).Negate(), clashingSubstitution))
                        {
                            var precludedBindings = clashingSubstitution.Bindings.Where(k => !substitution.Bindings.ContainsKey(k.Key));

                            if (precludedBindings.Any())
                            {
                                foreach (var kvp in precludedBindings)
                                {
                                    constraintElements.Add(new Literal(new Predicate(EqualitySymbol.Instance, kvp.Key, kvp.Value), true));
                                }
                            }
                            else
                            {
                                // We didn't need to specify any further vars to get a conflict - so this substitution negates a
                                // goal element *as-is* and thus can't be used at all.
                                constraints = null;
                                return false;
                            }
                        }
                    }
                }

                constraints = new Goal(constraintElements);
                return true;
            }

            // Local method to create variable subsitutions such that the effects elements transformed by the substitution contain at least one match to the goal's elements.
            // effectElements: The elements of the effect to be matched.
            // returns: An enumerable of VariableSubstitutions that can be applied to the effect elements to make at least one of the match a goal element
            IEnumerable<VariableSubstitution> MatchWithGoal(IEnumerable<Literal> effectElements)
            {
                foreach (var effectElement in effectElements)
                {
                    // Here we iterate through all elements of the goal, trying to find unifications with the effect element.
                    // We return each unification we find immediately - for an effect to be relevant it only needs to match at least one element of the goal.
                    foreach (var goalElement in goal.Elements)
                    {
                        if (LiteralUnifier.TryCreate(goalElement, effectElement, out var unifier))
                        {
                            yield return unifier;
                        }
                    }
                }
            }

            foreach (var schema in domain.Actions)
            {
                var standardisedSchema = new Standardisation().ApplyTo(schema);

                foreach (var substitution in MatchWithGoal(standardisedSchema.Effect.Elements).Distinct())
                {
                    if (TryGetConstraints(standardisedSchema.Effect.Elements, substitution, out var constraints))
                    {
                        yield return (standardisedSchema, substitution, constraints);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the actions that are relevant to a given goal in a given problem.
        /// </summary>
        /// <param name="domain">The domain of the problem being solved.</param>
        /// <param name="goal">The goal to retrieve the relevant actions for.</param>
        /// <returns>The actions that are relevant to the given state.</returns>
        public static IEnumerable<Action> GetRelevantActions(Domain domain, Goal goal)
        {
            foreach (var (Schema, Substitution, Constraints) in GetRelevantActionDetails(domain, goal))
            {
                yield return new SchemaTransformation(Substitution, Constraints).ApplyTo(Schema);
            }
        }

        /// <summary>
        /// <para>
        /// Gets the variable substitution that must be made to transform the matching action schema (with the matching identifier)
        /// in the <see cref="Domain.Actions"/> of the given domain to the given action.
        /// </para>
        /// <para>
        /// Intended to be useful for succinct output of plan steps. We don't want to "bloat" our action model with this
        /// information (planners won't and shouldn't care what the original variable name was), but it is useful when
        /// producing human-readable information.
        /// </para>
        /// <para>
        /// Note that we are effectively recreating the substitutions built by the relevant <see cref="ProblemInspector"/> methods, here.
        /// An alternative approach would of course be for those methods to return both the schema and the substitution (rather than just the
        /// transformed action), so that the algorithm can keep track itself if it wants to. For now at least though, I'm prioritising keep the
        /// actual planning as lean and mean as possible over making the action formatting super snappy.
        /// </para>
        /// </summary>
        /// <returns>A <see cref="VariableSubstitution"/> that maps the variables as defined in the schema to the terms referred to in the provided action.</returns>
        public static VariableSubstitution GetMappingFromSchema(Domain domain, Action action)
        {
            // TODO: All rationalisations aside, this is a bit naff. Sort me out.

            // Note that this is mostly awkward due to the unordered nature of elements. if we preserved the order of things in our model classes then
            // matching would of course be much easier. HOWEVER, of course when it comes to equality (super important) we need order NOT to matter.
            // We could of course offer the best of both worlds, but I'm not ready to add any more complexity than is absolutely needed to our model just yet..
            IEnumerable<VariableSubstitution> MatchWithSchemaElements(IEnumerable<Literal> actionElements, IEnumerable<Literal> schemaElements, VariableSubstitution unifier)
            {
                if (!actionElements.Any())
                {
                    yield return unifier;
                }
                else
                {
                    var firstActionElement = actionElements.First();

                    foreach (var schemaElement in schemaElements)
                    {
                        var firstActionElementUnifier = new VariableSubstitution(unifier);

                        if (LiteralUnifier.TryUpdateUnsafe(schemaElement, firstActionElement, firstActionElementUnifier))
                        {
                            foreach (var restOfActionElementsUnifier in MatchWithSchemaElements(actionElements.Skip(1), schemaElements.Except(new[] { schemaElement }), firstActionElementUnifier))
                            {
                                yield return restOfActionElementsUnifier;
                            }
                        }
                    }
                }
            }

            IEnumerable<VariableSubstitution> MatchWithSchema(Action action, Action schema, VariableSubstitution unifier)
            {
                foreach (var preconditionSub in MatchWithSchemaElements(action.Precondition.Elements, schema.Precondition.Elements, unifier))
                {
                    foreach (var sub in MatchWithSchemaElements(action.Effect.Elements, schema.Effect.Elements, unifier))
                    {
                        yield return sub;
                    }
                }
            }

            var schema =
                domain.Actions.SingleOrDefault(a => a.Identifier.Equals(action.Identifier))
                ?? throw new ArgumentException($"Action not found! There is no action in the domain with identifier '{action.Identifier}'");

            // TODO: I suspect its possible for this to return more than one result.. investigate, write tests
            return MatchWithSchema(action, schema, new VariableSubstitution()).Single();
        }

        private class Standardisation : RecursiveActionTransformation
        {
            private readonly Dictionary<VariableDeclaration, VariableDeclaration> mapping = new();

            public override VariableDeclaration ApplyTo(VariableDeclaration variableDeclaration)
            {
                if (!mapping.TryGetValue(variableDeclaration, out var standardisedVariableDeclaration))
                {
                    standardisedVariableDeclaration = mapping[variableDeclaration] = new VariableDeclaration(new StandardisedVariableSymbol(variableDeclaration.Symbol));
                }

                return standardisedVariableDeclaration;
            }
        }

        // NB: does not override equality - so equality has reference semantics.
        // This is important to be able to use the same action more than once in a plan.
        // TODO-BUG-MAJOR: Probable issue - when identifying distinct goals in state space search,
        // some degree of recognition of variables being the same would be useful.
        // Might need to be logic in Goal, not here..
        internal class StandardisedVariableSymbol
        {
            public StandardisedVariableSymbol(object originalSymbol) => OriginalSymbol = originalSymbol;

            internal object OriginalSymbol { get; }

            public override string ToString() => $"<{OriginalSymbol}>";
        }

        private class SchemaTransformation : RecursiveActionTransformation
        {
            private readonly VariableSubstitution substitution;
            private readonly Goal constraints;

            public SchemaTransformation(VariableSubstitution substitution, Goal constraints)
            {
                this.substitution = substitution;
                this.constraints = constraints;
            }

            public override Action ApplyTo(Action action)
            {
                action = base.ApplyTo(action);

                if (!constraints.Equals(Goal.Empty))
                {
                    // There are constraints - add them to the precondition of the action
                    return new Action(
                        identifier: action.Identifier,
                        precondition: new Goal(action.Precondition.Elements.Concat(constraints.Elements)),
                        effect: action.Effect);
                }
                else
                {
                    return action;
                }
            }

            public override Literal ApplyTo(Literal literal) => substitution.ApplyTo(literal);
        }
    }
}
