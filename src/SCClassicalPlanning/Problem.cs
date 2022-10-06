using SCClassicalPlanning.ProblemManipulation;
using SCFirstOrderLogic;
using SCFirstOrderLogic.SentenceManipulation;
using SCFirstOrderLogic.SentenceManipulation.Unification;
using System.Collections.ObjectModel;

namespace SCClassicalPlanning
{
    /// <summary>
    /// Encapsulates a planning problem.
    /// <para/>
    /// Problems exist within a <see cref="SCClassicalPlanning.Domain"/>, and consist of an initial <see cref="State"/>, an end <see cref="SCClassicalPlanning.Goal"/>,
    /// and a set of objects (represented by <see cref="Constant"/>s from the SCFirstOrderLogic library) that exist within the scope of the problem.
    /// </summary>
    public class Problem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Problem"/> class.
        /// </summary>
        /// <param name="domain">The domain in which this problem resides.</param>
        /// <param name="initialState">The initial state of the problem.</param>
        /// <param name="goal">The goal of the problem.</param>
        /// <param name="objects">The objects that exist in this problem.</param>
        public Problem(Domain domain, State initialState, Goal goal, IList<Constant> objects)
        {
            Domain = domain;
            Objects = new ReadOnlyCollection<Constant>(objects);
            InitialState = initialState;
            Goal = goal;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Problem"/> class, in which the objects that exist are inferred from the constants that are present in the initial state and the goal.
        /// </summary>
        /// <param name="domain">The domain in which this problem resides.</param>
        /// <param name="initialState">The initial state of the problem.</param>
        /// <param name="goal">The goal of the problem.</param>
        public Problem(Domain domain, State initialState, Goal goal)
        {
            Domain = domain;
            InitialState = initialState;
            Goal = goal;

            var constants = new HashSet<Constant>();
            StateConstantFinder.Instance.Visit(initialState, constants);
            GoalConstantFinder.Instance.Visit(goal, constants);
            Objects = new ReadOnlyCollection<Constant>(constants.ToArray());
        }

        /// <summary>
        /// Gets the domain in which this problem resides.
        /// </summary>
        public Domain Domain { get; }

        /// <summary>
        /// Gets the objects that exist in the problem.
        /// <para/>
        /// TODO-SIGNIFICANT: Problematic design-wise.. Large? IO? Fairly big deal because could have significant impact.
        /// Should Objects and InitialState be replaced with something like an 'IStateStore'? Which would perhaps mean that State should be IState?
        /// Explore this. Later.
        /// </summary>
        public ReadOnlyCollection<Constant> Objects { get; }

        /// <summary>
        /// Gets the initial state of the problem.
        /// <para/>
        /// TODO-SIGNIFICANT: Problematic design-wise.. Large? IO? Fairly big deal because could have significant impact.
        /// Should Objects and InitialState be replaced with something like an 'IStateStore'? Which would perhaps mean that State should be IState?
        /// Explore this. Later.
        /// </summary>
        public State InitialState { get; }

        /// <summary>
        /// Gets the goal of the problem.
        /// </summary>
        public Goal Goal { get; }

        /// <summary>
        /// Gets the (ground) actions that are applicable from a given state.
        /// </summary>
        /// <param name="state">The state to retrieve the applicable actions for.</param>
        /// <returns>The actions that are applicable from the given state.</returns>
        public IEnumerable<Action> GetApplicableActions(State state)
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

                            // TODO: using LiteralUnifier is perhaps overkill given that we know we're functionless,
                            // but will do for now. (doesn't really cost much more, so perhaps fine long-term).
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
                        IEnumerable<VariableSubstitution> allPossibleUnifiers = new List<VariableSubstitution>() { unifier };
                        var unboundVariables = firstGoalElement.Predicate.Arguments.OfType<VariableReference>().Except(unifier.Bindings.Keys);
                        foreach (var unboundVariable in unboundVariables)
                        {
                            allPossibleUnifiers = allPossibleUnifiers.SelectMany(u => Objects.Select(o =>
                            {
                                var newBindings = new Dictionary<VariableReference, Term>(u.Bindings);
                                newBindings[unboundVariable] = o;
                                return new VariableSubstitution(newBindings);
                            }));
                        }

                        foreach (var firstGoalElementUnifier in allPossibleUnifiers)
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

            // The overall task to be accomplished here is to find (action, variable substituion) pairings such that
            // the state's elements satisfy the action precondition (after the variable substitution is applied to it).
            // First. iterate the available actions:
            foreach (var actionTemplate in Domain.Actions)
            {
                // Note than when trying to match elements of the precondition to elements of the state, we consider positive
                // elements of the goal first - on the assumption that these will narrow down the search far more than negative elements
                // (i.e. there'll be far fewer objects for which a given predicate DOES hold than for which a given predicate DOESN'T hold).
                // Beyond that, there's no specific element ordering - we just look at them in the order they happen to fall.
                // Ideally, we'd do more to order the elements in a way that minimises the amount of work we have to do - but it would require
                // some analysis of the problem, and is something we'd likely want to abstract to allow for different approaches (this is a TODO).
                var orderedElements = actionTemplate.Precondition.PositiveElements.Concat(actionTemplate.Precondition.NegativeElements);

                // Now we can try to find appropriate variable substitutions, which is what this (recursive) MatchWithState method does:
                foreach (var substitution in MatchWithState(orderedElements, new VariableSubstitution()))
                {
                    yield return new Action(
                        actionTemplate.Identifier,
                        new VariableSubstitutionGoalTransformation(substitution).ApplyTo(actionTemplate.Precondition),
                        new VariableSubstitutionEffectTransformation(substitution).ApplyTo(actionTemplate.Effect));
                }
            }
        }

        /// <summary>
        /// Gets the (ground) actions that are relevant to a given goal.
        /// <para/>
        /// TODO/NB: All the results here are ground results - which is rather (potentially extremely) inefficient if the problem is large.
        /// It'd be nice to be able to have an equivalent nethod (in <see cref="Domain"/>) that can return <see cref="Action"/>s that have
        /// some (potentially constrained) variable references in them. That's a TODO..
        /// </summary>
        /// <param name="goal">The goal to retrieve the relevant actions for.</param>
        /// <returns>The actions that are relevant to the given state.</returns>
        public IEnumerable<Action> GetRelevantActions(Goal goal)
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

                    // At this point we have a unifier that includes a valid binding to match at least one of the effects elements to an
                    // element of the goal, and none of the preceding effect elements to an effect of the goal.
                    // If this covers all the variables that occur in *this* effect element, all we need to do is check that the effect element
                    // transformed by the existing unifier does not occur in the goal. However, if there are any that
                    // do not occur, we need to check for the existence of the negation of the literal formed by substituting EVERY combination of
                    // objects in the problem for the as yet unbound variables. This is obviously VERY expensive for large problems with lots of objects -
                    // though I guess clever indexing could help (support for indexing is TODO). Would be nice to do this at the domain level, if constrained
                    // variables aren't a problem..
                    IEnumerable<VariableSubstitution> allPossibleUnifiers = new List<VariableSubstitution>() { unifier };
                    var unboundVariables = firstEffectElement.Predicate.Arguments.OfType<VariableReference>().Except(unifier.Bindings.Keys);
                    foreach (var unboundVariable in unboundVariables)
                    {
                        allPossibleUnifiers = allPossibleUnifiers.SelectMany(u => Objects.Select(o =>
                        {
                            var newBindings = new Dictionary<VariableReference, Term>(u.Bindings);
                            newBindings[unboundVariable] = o;
                            return new VariableSubstitution(newBindings);
                        }));
                    }

                    foreach (var firstEffectElementUnifier in allPossibleUnifiers)
                    {
                        var possibleLiteral = firstEffectElementUnifier.ApplyTo(firstEffectElement);

                        if (!goal.Elements.Contains(possibleLiteral.Negate()))
                        {
                            foreach (var restOfGoalElementsUnifier in UnmatchWithGoalNegation(effectElements.Skip(1), firstEffectElementUnifier))
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

            foreach (var actionTemplate in Domain.Actions)
            {
                foreach (var potentialSubsitution in MatchWithGoal(actionTemplate.Effect.Elements))
                {
                    foreach (var substitution in UnmatchWithGoalNegation(actionTemplate.Effect.Elements, potentialSubsitution))
                    {
                        yield return new Action(
                            actionTemplate.Identifier,
                            new VariableSubstitutionGoalTransformation(substitution).ApplyTo(actionTemplate.Precondition),
                            new VariableSubstitutionEffectTransformation(substitution).ApplyTo(actionTemplate.Effect));
                    }
                }
            }
        }

        /// <summary>
        /// Utility class to find <see cref="Constant"/> instances within the elements of a <see cref="State"/>, and add them to a given <see cref="HashSet{T}"/>.
        /// </summary>
        private class StateConstantFinder : RecursiveStateVisitor<HashSet<Constant>>
        {
            /// <summary>
            /// Gets a singleton instance of the <see cref="StateConstantFinder"/> class.
            /// </summary>
            public static StateConstantFinder Instance { get; } = new();

            /// <inheritdoc/>
            public override void Visit(Constant constant, HashSet<Constant> constants) => constants.Add(constant);
        }

        /// <summary>
        /// Utility class to find <see cref="Constant"/> instances within the elements of a <see cref="SCClassicalPlanning.Goal"/>, and add them to a given <see cref="HashSet{T}"/>.
        /// </summary>
        private class GoalConstantFinder : RecursiveGoalVisitor<HashSet<Constant>>
        {
            /// <summary>
            /// Gets a singleton instance of the <see cref="GoalConstantFinder"/> class.
            /// </summary>
            public static GoalConstantFinder Instance { get; } = new();

            /// <inheritdoc/>
            public override void Visit(Constant constant, HashSet<Constant> constants) => constants.Add(constant);
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
