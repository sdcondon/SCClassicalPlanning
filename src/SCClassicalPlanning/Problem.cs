﻿using SCClassicalPlanning.ProblemManipulation;
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
    /// and a set of objects (represented here by <see cref="Constant"/>s from the SCFirstOrderLogic library) that exist within the scope of the problem.
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
        /// And everything that entails - Effect.ApplyTo(Effect) probably becomes IState.Apply(Effect) and so on. Explore this. Later.
        /// </summary>
        public ReadOnlyCollection<Constant> Objects { get; }

        /// <summary>
        /// Gets the initial state of the problem.
        /// <para/>
        /// TODO-SIGNIFICANT: Problematic design-wise.. Large? IO? Fairly big deal because could have significant impact.
        /// Should Objects and InitialState be replaced with something like an 'IStateStore'? Which would perhaps mean that State should be IState?
        /// And everything that entails - Effect.ApplyTo(Effect) probably becomes IState.Apply(Effect) and so on. Explore this. Later.
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

                            // TODO: using LiteralUnifier is perhaps overkill given that we know we're functionless, but will do for now.
                            // (doesn't really cost much more..)
                            if (LiteralUnifier.TryUpdateUnsafe(stateElement, goalElements.First(), firstGoalElementUnifier))
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

                            // TODO: perhaps even slower than needed - exhaustively iterating state n times..
                            // Can we check for them all at once - e.g. above foreach - allPossibleUnifiers.Select(ApplyTo(firstGoalElement)).Except(state.Elements) kinda thing?
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
                // (i.e. there'll be far less objects for which a given predicate DOES hold than for which a given predicate DOESN'T hold).
                // Beyond that, there's no specific element ordering - we just look at them in the order they happen to fall.
                // Ideally, we'd do more to order the elements in a way that minimises the amount of work we have to do - but it would require
                // some analysis of the problem, and is something we'd likely want to abstract to allow for different approaches (this is a TODO).
                var orderedElements = actionTemplate.Precondition.PositiveElements.Concat(actionTemplate.Precondition.NegativeElements);

                // Now we can try to find appropriate variable substitutions, which is what this (recursive) MatchWithState method does:
                foreach (var substitution in MatchWithState(actionTemplate.Precondition.Elements, new VariableSubstitution()))
                {
                    yield return new Action(
                        actionTemplate.Identifier,
                        new VariableSubstitutionGoalTransformation(substitution).ApplyTo(actionTemplate.Precondition),
                        new VariableSubstitutionEffectTransformation(substitution).ApplyTo(actionTemplate.Effect));
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
        /// Utility class to find <see cref="Constant"/> instances within the elements of a <see cref="Goal"/>, and add them to a given <see cref="HashSet{T}"/>.
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
