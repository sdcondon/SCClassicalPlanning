using SCClassicalPlanning.ProblemManipulation;
using SCFirstOrderLogic;
using SCFirstOrderLogic.SentenceManipulation;
using SCFirstOrderLogic.SentenceManipulation.Unification;

namespace SCClassicalPlanning.Planning
{
    /// <summary>
    /// Various domain inspection methods, useful to planners.
    /// </summary>
    public static class DomainInspector
    {
        /// <summary>
        /// Gets the variable substitution that must be made to transform the matching action schema (with the matching identifier)
        /// in the <see cref="Domain.Actions"/> of the given domain to the given action.
        /// <para/>
        /// Intended to be useful for succinct output of plan steps. We don't want to "bloat" our action model with this
        /// information (planners won't and shouldn't care what the original variable name was), but it is useful when
        /// producing human-readable information.
        /// <para/>
        /// Note that we are effectively recreating the substitutions built by the relevant <see cref="ProblemInspector"/> methods, here.
        /// An alternative approach would of course be for those methods to return both the schema and the substitution (rather than just the
        /// transformed action), so that the algorithm can keep track itself if it wants to. For now at least though, I'm prioritising keep the
        /// actual planning as lean and mean as possible over making the action formatting super snappy.
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

        /// <summary>
        /// Gets the (action schema, variable substitution) pairings that represent actions that are relevant to a given goal in a given problem.
        /// </summary>
        /// <param name="domain">The domain of the problem being solved.</param>
        /// <param name="goal">The goal to retrieve the relevant actions for.</param>
        /// <returns>The actions that are relevant to the given state.</returns>
        public static IEnumerable<(Action schema, VariableSubstitution substitution, Goal constraints)> GetRelevantActionSchemaSubstitutions(Domain domain, Goal goal)
        {
            // Local method to check that a given unifier results in an unconstrained non-match with the negation of the elements of the goal.
            // Worth it because ExpandNonMatchesWithGoalNegation is potentially so expensive.
            bool TryGetConstraints(IEnumerable<Literal> effectElements, VariableSubstitution substitution, out Goal constraints)
            {
                List<Literal> constraintElements = new();

                foreach (var effectElement in effectElements)
                {
                    foreach (var goalElement in goal.Elements)
                    {
                        var clashingSubstitution = new VariableSubstitution(substitution);

                        if (LiteralUnifier.TryUpdateUnsafe(goalElement, substitution.ApplyTo(effectElement).Negate(), clashingSubstitution))
                        {
                            var constrainedVars = clashingSubstitution.Bindings.Where(k => !substitution.Bindings.ContainsKey(k.Key));

                            if (!constrainedVars.Any())
                            {
                                // We didn't need to specify any further vars to get a conflict - so this substitution won't work at all.
                                constraints = null;
                                return false;
                            }
                            else
                            {
                                foreach (var kvp in constrainedVars)
                                {
                                    constraintElements.Add(new Literal(new Predicate(EqualitySymbol.Instance, kvp.Key, kvp.Value), true));
                                }
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
                // TODO: "standardise" here.

                foreach (var substitution in MatchWithGoal(schema.Effect.Elements).Distinct())
                {
                    if (TryGetConstraints(schema.Effect.Elements, substitution, out var constraints))
                    {
                        yield return (schema, substitution, constraints);
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
            foreach (var (Schema, Substitution, Constraints) in GetRelevantActionSchemaSubstitutions(domain, goal))
            {
                yield return new VariableSubstitutionActionTransformation(Substitution, Constraints).ApplyTo(Schema);
            }
        }

        /// <summary>
        /// Utility class to transform <see cref="Action"/> instances using a given <see cref="VariableSubstitution"/>.
        /// </summary>
        private class VariableSubstitutionActionTransformation : RecursiveActionTransformation
        {
            private readonly VariableSubstitution substitution;
            private readonly Goal constraints;

            public VariableSubstitutionActionTransformation(VariableSubstitution substitution, Goal constraints)
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
