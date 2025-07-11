﻿// Copyright 2022-2024 Simon Condon
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
using SCFirstOrderLogic.SentenceManipulation.VariableManipulation;
using System.Diagnostics.CodeAnalysis;

namespace SCClassicalPlanning.Planning.Utilities;

/// <summary>
/// <para>
/// Various problem inspection methods, useful to planners.
/// </para>
/// <para>
/// TODO-EXTENSIBILITY: Might ultimately be useful to extract an interface(s) from this and allow planners to use other strategies
/// (that leverage extensions such as type systems and axioms..). But for now at least, its just static.
/// </para>
/// </summary>
public static class ProblemInspector
{
    /// <summary>
    /// Gets the (action schema, ground variable substitution) pairings that represent actions that are applicable from a given state in a given problem.
    /// </summary>
    /// <param name="state">The state to retrieve the applicable actions for.</param>
    /// <param name="actionSchemas">The available action schemas.</param>
    /// <returns>The actions that are applicable from the given state.</returns>
    public static IEnumerable<(Action schema, VariableSubstitution substitution)> GetApplicableActionDetails(IState state, IQueryable<Action> actionSchemas)
    {
        // Local method to (recursively) match a set of (remaining) goal elements to the given state.
        // goalElements: The remaining elements of the goal to be matched
        // unifier: The VariableSubstitution established so far (by matching earlier goal elements)
        // returns: An enumerable of VariableSubstitutions that can be applied to the goal elements to make the given state meet them 
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
                        if (Unifier.TryUpdate((Literal)stateElement, firstGoalElement, unifier, out var firstGoalElementUnifier))
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
                    foreach (var firstGoalElementUnifier in GetAllPossibleSubstitutions(firstGoalElement.Predicate, state.GetAllConstants(), unifier))
                    {
                        var possiblePredicate = firstGoalElementUnifier.ApplyTo(firstGoalElement.Predicate);

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
        // the state's elements meet the action precondition (after the variable substitution is applied to it).
        // First, we iterate the action schemas:
        foreach (var actionSchema in actionSchemas)
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
    /// <param name="actionSchemas">The available action schemas.</param>
    /// <param name="state">The state to retrieve the applicable actions for.</param>
    /// <returns>The actions that are applicable from the given state.</returns>
    public static IEnumerable<Action> GetApplicableActions(IState state, IQueryable<Action> actionSchemas)
    {
        // Now we can try to find appropriate variable substitutions, which is what this (recursive) MatchWithState method does:
        foreach (var (Schema, Substitution) in GetApplicableActionDetails(state, actionSchemas))
        {
            // For each substitution, apply it to the action schema and return it:
            yield return new VariableSubstitutionActionTransformation(Substitution).ApplyTo(Schema);
        }
    }

    /// <summary>
    /// <para>
    /// Gets the (action schema, *ground* variable substitution) pairings that represent actions that are relevant to a given goal in a given problem.
    /// </para>
    /// <para>
    /// NB: All the results here are ground results - which is of course rather (potentially extremely) inefficient if the problem is large.
    /// See <see cref="GetRelevantLiftedActionDetails"/> for a variable-preserving equivalent to this method.
    /// </para>
    /// </summary>
    /// <param name="goal">The goal to retrieve the relevant actions for.</param>
    /// <param name="actionSchemas">The available actions.</param>
    /// <param name="constants">The available constants.</param>
    /// <returns>The actions that are relevant to the given state.</returns>
    public static IEnumerable<(Action schema, VariableSubstitution substitution)> GetRelevantGroundActionDetails(Goal goal, IQueryable<Action> actionSchemas, IEnumerable<Function> constants)
    {
        // Local method to create variable subsitutions such that the negation of the effects elements transformed by the substitution do not match any of the goal's elements.
        // effectElements: The (remaining) elements of the effect to be matched.
        // returns: An enumerable of VariableSubstitutions that can be applied to the effect elements to make none of them match the negation of a goal element
        IEnumerable<VariableSubstitution> ExpandNonMatchesWithGoalNegation(IEnumerable<Literal> effectElements, VariableSubstitution substitution)
        {
            if (!effectElements.Any())
            {
                yield return substitution;
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
                foreach (var firstEffectElementUnifier in GetAllPossibleSubstitutions(firstEffectElement.Predicate, constants, substitution))
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
                    if (Unifier.TryCreate(goalElement, effectElement, out var unifier))
                    {
                        yield return unifier;
                    }
                }
            }
        }

        foreach (var actionSchema in actionSchemas)
        {
            foreach (var potentialSubstitution in MatchWithGoal(actionSchema.Effect.Elements))
            {
                foreach (var substitution in ExpandNonMatchesWithGoalNegation(actionSchema.Effect.Elements, potentialSubstitution))
                {
                    yield return (actionSchema, substitution);
                }
            }
        }
    }

    /// <summary>
    /// <para>
    /// Gets the *ground* actions that are relevant to a given goal in a given problem.
    /// </para>
    /// <para>
    /// NB: All the results here are ground results - which is of course rather (potentially extremely) inefficient if the problem is large.
    /// See <see cref="GetRelevantLiftedActions"/> for a variable-preserving equivalent to this method.
    /// </para>
    /// </summary>
    /// <param name="goal">The goal to retrieve the relevant actions for.</param>
    /// <param name="actionSchemas">The available actions.</param>
    /// <param name="constants">The available constants.</param>
    /// <returns>The actions that are relevant to the given state.</returns>
    public static IEnumerable<Action> GetRelevantGroundActions(Goal goal, IQueryable<Action> actionSchemas, IEnumerable<Function> constants)
    {
        foreach (var (Schema, Substitution) in GetRelevantGroundActionDetails(goal, actionSchemas, constants))
        {
            yield return new VariableSubstitutionActionTransformation(Substitution).ApplyTo(Schema);
        }
    }

    /// <summary>
    /// Gets the (action schema, variable substitution, variable constraints) tuples that represent actions that are relevant to a given goal in a given problem.
    /// </summary>
    /// <param name="goal">The goal to retrieve the relevant actions for.</param>
    /// <param name="actionSchemas">The available actions.</param>
    /// <returns>The details of the actions that are relevant to the given state.</returns>
    public static IEnumerable<(Action schema, VariableSubstitution substitution, Goal constraints)> GetRelevantLiftedActionDetails(Goal goal, IQueryable<Action> actionSchemas)
    {
        // Local method to find any constraints that apply to a given substitution for none of the goal's elements to be negated.
        bool TryGetConstraints(IEnumerable<Literal> effectElements, VariableSubstitution substitution, [MaybeNullWhen(false)] out Goal constraints)
        {
            List<Literal> constraintElements = new();

            foreach (var effectElement in effectElements)
            {
                foreach (var goalElement in goal.Elements)
                {
                    if (Unifier.TryUpdate(goalElement, substitution.ApplyTo(effectElement).Negate(), substitution, out var clashingSubstitution))
                    {
                        var precludedBindings = clashingSubstitution.Bindings.Where(k => !substitution.Bindings.ContainsKey(k.Key));

                        if (precludedBindings.Any())
                        {
                            foreach (var kvp in precludedBindings)
                            {
                                constraintElements.Add(new Literal(new Predicate(EqualityIdentifier.Instance, kvp.Key, kvp.Value), true));
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
                    if (Unifier.TryCreate(goalElement, effectElement, out var unifier))
                    {
                        yield return unifier;
                    }
                }
            }
        }

        foreach (var schema in actionSchemas)
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
    /// <param name="goal">The goal to retrieve the relevant actions for.</param>
    /// <param name="actionSchemas">The available action schemas.</param>
    /// <returns>The actions that are relevant to the given state.</returns>
    public static IEnumerable<Action> GetRelevantLiftedActions(Goal goal, IQueryable<Action> actionSchemas)
    {
        foreach (var (Schema, Substitution, Constraints) in GetRelevantLiftedActionDetails(goal, actionSchemas))
        {
            yield return new SchemaTransformation(Substitution, Constraints).ApplyTo(Schema);
        }
    }

    /// <summary>
    /// <para>
    /// Gets the variable substitution that must be made to transform the matching action schema (with the matching identifier)
    /// in the <see cref="Problem.ActionSchemas"/> of the given problem to the given action.
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
    public static VariableSubstitution GetMappingFromSchema(Action action, IQueryable<Action> actionSchemas)
    {
        // TODO: All rationalisations aside, this is a bit naff. Sort me out.

        // Note that this is mostly awkward due to the unordered nature of elements. If we preserved the order of things in our model classes then
        // matching would of course be much easier. HOWEVER, of course when it comes to equality (super important) we need order NOT to matter.
        // We could of course offer the best of both worlds, but I'm not ready to add any more complexity than is absolutely needed to our model just yet..
        IEnumerable<VariableSubstitution> MatchWithSchemaElements(IEnumerable<Literal> actionElements, IEnumerable<Literal> schemaElements, VariableSubstitution unifier)
        {
            if (!schemaElements.Any())
            {
                yield return unifier;
            }
            else
            {
                var firstSchemaElement = schemaElements.First();

                foreach (var actionElement in actionElements)
                {
                    if (Unifier.TryUpdate(firstSchemaElement, actionElement, unifier, out var firstSchemaElementUnifier))
                    {
                        foreach (var restOfSchemaElementsUnifier in MatchWithSchemaElements(actionElements, schemaElements.Skip(1), firstSchemaElementUnifier))
                        {
                            yield return restOfSchemaElementsUnifier;
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
            actionSchemas.SingleOrDefault(a => a.Identifier.Equals(action.Identifier))
            ?? throw new ArgumentException($"Action not found! There is no action in the domain with identifier '{action.Identifier}'");

        // It's possible for more than one entry here - but where they do exist they will be duplicates
        // Probably worth an investigation into performance here, but just returning the first hit is fine.
        return MatchWithSchema(action, schema, new VariableSubstitution()).First();
    }

    /// <summary>
    /// <para>
    /// Gets all possible variable substitutions that populate each of the arguments of a given predicate with a constant.
    /// </para>
    /// <para>
    /// NB: be careful - the risk of combinatorial explosion is significant here.
    /// </para>
    /// </summary>
    /// <param name="predicate">The predicate to create all possible substitutions for.</param>
    /// <param name="constants">The available constants.</param>
    /// <returns>All possible subsitutions that populate each of the arguments of the given predicate with a constant.</returns>
    public static IEnumerable<VariableSubstitution> GetAllPossibleSubstitutions(Predicate predicate, IEnumerable<Function> constants)
    {
        return GetAllPossibleSubstitutions(predicate, constants, new VariableSubstitution());
    }

    /// <summary>
    /// <para>
    /// Gets all possible variable substitutions that populate each of the arguments of a given predicate with a constant.
    /// Uses an existing substitution as a constraint.
    /// </para>
    /// <para>
    /// NB: be careful - the risk of combinatorial explosion is significant here.
    /// </para>
    /// </summary>
    /// <param name="predicate">The predicate to create all possible substitutions for.</param>
    /// <param name="constants">The available constants.</param>
    /// <param name="constraint">The substitution to use as a constraint.</param>
    /// <returns>All possible subsitutions that populate each of the arguments of the given predicate with a constant.</returns>
    public static IEnumerable<VariableSubstitution> GetAllPossibleSubstitutions(Predicate predicate, IEnumerable<Function> constants, VariableSubstitution constraint)
    {
        IEnumerable<VariableSubstitution> allPossibleSubstitutions = new List<VariableSubstitution>() { constraint };
        var unboundVariables = predicate.Arguments.OfType<VariableReference>().Except(constraint.Bindings.Keys);

        foreach (var unboundVariable in unboundVariables)
        {
            allPossibleSubstitutions = allPossibleSubstitutions.SelectMany(
                u => constants.Select(o => u.CopyAndAdd(KeyValuePair.Create(unboundVariable, (Term)o)))
            );
        }

        return allPossibleSubstitutions;
    }

    private class Standardisation : RecursiveActionTransformation
    {
        private readonly Dictionary<VariableDeclaration, VariableDeclaration> mapping = new();

        public override VariableDeclaration ApplyTo(VariableDeclaration variableDeclaration)
        {
            if (!mapping.TryGetValue(variableDeclaration, out var standardisedVariableDeclaration))
            {
                standardisedVariableDeclaration = mapping[variableDeclaration] = new VariableDeclaration(new StandardisedVariableSymbol(variableDeclaration.Identifier));
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
