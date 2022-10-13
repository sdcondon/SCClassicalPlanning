using SCClassicalPlanning.ProblemManipulation;
using SCFirstOrderLogic;
using SCFirstOrderLogic.SentenceManipulation;
using SCFirstOrderLogic.SentenceManipulation.Unification;

namespace SCClassicalPlanning.Planning
{
    /// <summary>
    /// Various problem inspection methods, useful to planners.
    /// <para/>
    /// TODO-EXTENSIBILITY: Might ultimately be useful to extract an interface(s) from this and allow planners to use other strategies
    /// (that leverage extensions such as type systems and axioms..). But for now at least, its just static.
    /// </summary>
    public static class ProblemInspector
    {
        /// <summary>
        /// Gets the (action schema, ground variable substitution) pairings that represent actions that are applicable from a given state in a given problem.
        /// </summary>
        /// <param name="problem">The problem being solved.</param>
        /// <param name="state">The state to retrieve the applicable actions for.</param>
        /// <returns>The actions that are applicable from the given state.</returns>
        public static IEnumerable<(Action schema, VariableSubstitution substitution)> GetApplicableActionSchemaSubstitutions(Problem problem, State state)
        {
            // Local method to (recursively) match a set of (remaining) goal elements to the given state.
            // goalElements: The remaining elements of the goal to be matched
            // unifier: The VariableSubstitution established so far (by matching earlier goal elements)
            // returns: An enumerable of VariableSubstitutions that can be applied to the goal elements to make them satisfied by the given state
            IEnumerable<VariableSubstitution> MatchWithState(IEnumerable<Literal> goalElements, VariableSubstitution unifier)
            {
                if (!goalElements.Any())
                {
                    yield return unifier;
                }
                else
                {
                    var firstGoalElement = goalElements.First();

                    if (firstGoalElement.IsPositive)
                    {
                        // The first of the remaining goal elements is positive.
                        // Here we iterate through ALL elements of the state, trying to find unifications with the goal element.
                        // Using some kind of index here would of course speed things up (support for this is a TODO).
                        // For each unification found, we then recurse for the rest of the elements of the goal.
                        foreach (var stateElement in state.Elements)
                        {
                            var firstGoalElementUnifier = new VariableSubstitution(unifier);

                            if (LiteralUnifier.TryUpdateUnsafe(stateElement, firstGoalElement, firstGoalElementUnifier))
                            {
                                foreach (var restOfGoalElementsUnifier in MatchWithState(goalElements.Skip(1), firstGoalElementUnifier))
                                {
                                    yield return restOfGoalElementsUnifier;
                                }
                            }
                        }
                    }
                    else
                    {
                        // The first of the remaining goal elements is negative.
                        // At this point we have a unifier that includes a valid binding for all the variables that occur in earlier
                        // elements of the goal. If this covers all the variables that occur in this goal element, all we need to do is check
                        // that the goal element transformed by the existing unifier does not occur in the state. However, if there are any that
                        // do not occur, we need to check for the existence of the predicate formed by substituting EVERY combination of objects
                        // in the problem for the as yet unbound variables. Noting that we do all the positive goal elements first (see below),
                        // one would hope that it is a very rare scenario to have a variable that doesn't occur in ANY positive goal elements (most
                        // will at least occur in e.g. a 'type' predicate). But this is obviously VERY expensive when it occurs - though I guess
                        // clever indexing could help (support for indexing is TODO).
                        foreach (var firstGoalElementUnifier in GetAllPossibleSubstitutions(problem.Objects, firstGoalElement.Predicate, unifier))
                        {
                            var possiblePredicate = firstGoalElementUnifier.ApplyTo(firstGoalElement.Predicate).Predicate;

                            if (!state.Elements.Contains(possiblePredicate))
                            {
                                foreach (var restOfGoalElementsUnifier in MatchWithState(goalElements.Skip(1), firstGoalElementUnifier))
                                {
                                    yield return restOfGoalElementsUnifier;
                                }
                            }
                        }
                    }
                }
            }

            // The overall task to be accomplished here is to find (action schema, variable substitution) pairings such that
            // the state's elements satisfy the action precondition (after the variable substitution is applied to it).
            // First, we iterate the action schemas:
            foreach (var actionSchema in problem.Domain.Actions)
            {
                // Note than when trying to match elements of the precondition to elements of the state, we consider positive
                // elements of the goal first - on the assumption that these will narrow down the search far more than negative elements
                // (i.e. there'll be far fewer objects for which a given predicate DOES hold than for which a given predicate DOESN'T hold).
                // Beyond that, there's no specific element ordering - we just look at them in the order they happen to fall.
                // Ideally, we'd do more to order the elements in a way that minimises the amount of work we have to do - but it would require
                // some analysis of the problem, and is something we'd likely want to abstract to allow for different approaches (this is a TODO).
                var orderedElements = actionSchema.Precondition.PositiveElements.Concat(actionSchema.Precondition.NegativeElements);

                // Now we can try to find appropriate variable substitutions, which is what this (recursive) MatchWithState method does:
                foreach (var substitution in MatchWithState(orderedElements, new VariableSubstitution()))
                {
                    yield return (actionSchema, substitution);
                }
            }
        }

        /// <summary>
        /// Gets the ground actions that are applicable from a given state in a given problem.
        /// </summary>
        /// <param name="problem">The problem being solved.</param>
        /// <param name="state">The state to retrieve the applicable actions for.</param>
        /// <returns>The actions that are applicable from the given state.</returns>
        public static IEnumerable<Action> GetApplicableActions(Problem problem, State state)
        {
            // Now we can try to find appropriate variable substitutions, which is what this (recursive) MatchWithState method does:
            foreach (var (Schema, Substitution) in GetApplicableActionSchemaSubstitutions(problem, state))
            {
                // For each substitution, apply it to the action schema and return it:
                yield return new Action(
                    Schema.Identifier,
                    new VariableSubstitutionGoalTransformation(Substitution).ApplyTo(Schema.Precondition),
                    new VariableSubstitutionEffectTransformation(Substitution).ApplyTo(Schema.Effect));
            }
        }

        /// <summary>
        /// Gets the (action schema, *ground* variable substitution) pairings that represent actions that are relevant to a given goal in a given problem.
        /// <para/>
        /// NB: All the results here are ground results - which is of course rather (potentially extremely) inefficient if the problem is large.
        /// It'd obviously be nice to be leave variables alone is its feasible (perhaps even allowing for constraints when some values are okay but others are
        /// not). Having played with this idea a little though, there are.. some subtleties - which go some way to explaining why even the earliest versions
        /// of PDDL have things like types and axioms..
        /// </summary>
        /// <param name="problem">The problem being solved.</param>
        /// <param name="goal">The goal to retrieve the relevant actions for.</param>
        /// <returns>The actions that are relevant to the given state.</returns>
        public static IEnumerable<(Action schema, VariableSubstitution substitution)> GetRelevantActionSchemaSubstitutions(Problem problem, Goal goal)
        {
            // Local method to check that a given unifier results in an unconstrained non-match with the negation of the elements of the goal.
            // Worth it because ExpandNonMatchesWithGoalNegation is potentially so expensive.
            bool IsUnconstrainedNonMatchWithGoalNegation(IEnumerable<Literal> effectElements, VariableSubstitution unifier)
            {
                foreach (var effectElement in effectElements)
                {
                    foreach (var goalElement in goal.Elements)
                    {
                        if (LiteralUnifier.TryUpdateUnsafe(goalElement, unifier.ApplyTo(effectElement).Negate(), new VariableSubstitution(unifier)))
                        {
                            return false;
                        }
                    }
                }

                return true;
            }

            // Local method to create variable subsitutions such that the negation of the effects elements transformed by the substitution do not match any of the goal's elements.
            // effectElements: The (remaining) elements of the effect to be matched.
            // returns: An enumerable of VariableSubstitutions that can be applied to the effect elements to make none of them match the negation of a goal element
            IEnumerable<VariableSubstitution> ExpandNonMatchesWithGoalNegation(IEnumerable<Literal> effectElements, VariableSubstitution unifier)
            {
                if (!effectElements.Any())
                {
                    yield return unifier;
                }
                else
                {
                    var firstEffectElement = effectElements.First();

                    // At this point we have a unifier that includes a valid binding to match at least one of the effects elements to an
                    // element of the goal, and none of the preceding effect elements to an effect of the goal.
                    // If this covers all the variables that occur in *this* effect element, all we need to do is check that the effect element
                    // transformed by the existing unifier does not occur in the goal. However, if there are any that
                    // do not occur, we need to check for the existence of the negation of the literal formed by substituting EVERY combination of
                    // objects in the problem for the as yet unbound variables. This is obviously VERY expensive for large problems with lots of objects -
                    // though I guess clever indexing could help (support for indexing is TODO).
                    IEnumerable<VariableSubstitution> allPossibleUnifiers = new List<VariableSubstitution>() { unifier };
                    var unboundVariables = firstEffectElement.Predicate.Arguments.OfType<VariableReference>().Except(unifier.Bindings.Keys);
                    foreach (var unboundVariable in unboundVariables)
                    {
                        allPossibleUnifiers = allPossibleUnifiers.SelectMany(u => problem.Objects.Select(o =>
                        {
                            var newBindings = new Dictionary<VariableReference, Term>(u.Bindings)
                            {
                                [unboundVariable] = o
                            };

                            return new VariableSubstitution(newBindings);
                        }));
                    }

                    foreach (var firstEffectElementUnifier in allPossibleUnifiers)
                    {
                        var possibleLiteral = firstEffectElementUnifier.ApplyTo(firstEffectElement);

                        if (!goal.Elements.Contains(possibleLiteral.Negate()))
                        {
                            foreach (var restOfGoalElementsUnifier in ExpandNonMatchesWithGoalNegation(effectElements.Skip(1), firstEffectElementUnifier))
                            {
                                yield return restOfGoalElementsUnifier;
                            }
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

            foreach (var schema in problem.Domain.Actions)
            {
                foreach (var potentialSubstitution in MatchWithGoal(schema.Effect.Elements))
                {
                    ////if (IsUnconstrainedNonMatchWithGoalNegation(schema.Effect.Elements, potentialSubstitution))
                    ////{
                    ////    yield return (schema, potentialSubstitution);
                    ////}
                    ////else
                    ////{
                        foreach (var substitution in ExpandNonMatchesWithGoalNegation(schema.Effect.Elements, potentialSubstitution))
                        {
                            yield return (schema, substitution);
                        }
                    ////}
                }
            }
        }

        /// <summary>
        /// Gets the *ground* actions that are relevant to a given goal in a given problem.
        /// <para/>
        /// NB: All the results here are ground results - which is of course rather (potentially extremely) inefficient if the problem is large.
        /// It'd be nice to be able to have an equivalent nethod (in <see cref="Domain"/>) that can return <see cref="Action"/>s that have
        /// some variable references in them, with constraints on the substitutions that can be made if necessary. Having played with this idea a little
        /// though, there are.. some subtleties - which provide some insight into explaining why even the earliest versions of PDDL have things like axioms and types..
        /// </summary>
        /// <param name="problem">The problem being solved.</param>
        /// <param name="goal">The goal to retrieve the relevant actions for.</param>
        /// <returns>The actions that are relevant to the given state.</returns>
        public static IEnumerable<Action> GetRelevantActions(Problem problem, Goal goal)
        {
            foreach (var (Schema, Substitution) in GetRelevantActionSchemaSubstitutions(problem, goal))
            {
                yield return new Action(
                    Schema.Identifier,
                    new VariableSubstitutionGoalTransformation(Substitution).ApplyTo(Schema.Precondition),
                    new VariableSubstitutionEffectTransformation(Substitution).ApplyTo(Schema.Effect));
            }
        }

        public static IEnumerable<VariableSubstitution> GetAllPossibleSubstitutions(IEnumerable<Constant> objects, Predicate predicate, VariableSubstitution unifier)
        {
            IEnumerable<VariableSubstitution> allPossibleSubstitutions = new List<VariableSubstitution>() { unifier };
            var unboundVariables = predicate.Arguments.OfType<VariableReference>().Except(unifier.Bindings.Keys);
            foreach (var unboundVariable in unboundVariables)
            {
                allPossibleSubstitutions = allPossibleSubstitutions.SelectMany(u => objects.Select(o =>
                {
                    var newBindings = new Dictionary<VariableReference, Term>(u.Bindings)
                    {
                        [unboundVariable] = o
                    };

                    return new VariableSubstitution(newBindings);
                }));
            }

            return allPossibleSubstitutions;
        }

        /// <summary>
        /// Utility class to transform <see cref="Goal"/> instances using a given <see cref="VariableSubstitution"/>.
        /// </summary>
        private class VariableSubstitutionGoalTransformation : RecursiveGoalTransformation
        {
            private readonly VariableSubstitution substitution;

            public VariableSubstitutionGoalTransformation(VariableSubstitution substitution) => this.substitution = substitution;

            public override Literal ApplyTo(Literal literal) => substitution.ApplyTo(literal);
        }

        /// <summary>
        /// Utility class to transform <see cref="Effect"/> instances using a given <see cref="VariableSubstitution"/>.
        /// </summary>
        private class VariableSubstitutionEffectTransformation : RecursiveEffectTransformation
        {
            private readonly VariableSubstitution substitution;

            public VariableSubstitutionEffectTransformation(VariableSubstitution substitution) => this.substitution = substitution;

            public override Literal ApplyTo(Literal literal) => substitution.ApplyTo(literal);
        }
    }
}
