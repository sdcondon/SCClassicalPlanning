using SCClassicalPlanning.ProblemManipulation;
using SCFirstOrderLogic;
using SCFirstOrderLogic.SentenceManipulation;
using SCFirstOrderLogic.SentenceManipulation.Unification;
using System.Diagnostics;

namespace SCClassicalPlanning.Planning.StateSpaceSearch.Heuristics
{
    /// <summary>
    /// State space search heuristic that ignores preconditions and uses a greedy set cover algorithm
    /// to provide its estimate.
    /// <para/>
    /// Not "admissable" (mostly because greedy set cover can overestimate) - 
    /// so the plans discovered using it won't necessarily be optimal, but better than heuristics
    /// that don't examine the available actions at all..
    /// </summary>
    public class IgnorePreconditionsGreedySetCover
    {
        private readonly Problem problem;

        /// <summary>
        /// Initializes a new instance of the <see cref="IgnorePreconditionsGreedySetCover"/> class.
        /// </summary>
        /// <param name="problem">The problem being solved.</param>
        public IgnorePreconditionsGreedySetCover(Problem problem) => this.problem = problem;

        /// <summary>
        /// Estimates the cost of getting from the given state to a state that satisfies the given goal.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <param name="goal">The goal.</param>
        /// <returns>An estimate of the cost of getting from the given state to a state that satisfies the given goal.</returns>
        public float EstimateCost(State state, Goal goal)
        {
            var unsatisfiedGoalElements = GetUnsatisfiedGoalElements(state, goal);
            if (unsatisfiedGoalElements.Count() == 0)
            {
                return 0;
            }

            var relevantEffects = GetRelevantEffects(unsatisfiedGoalElements, state.Elements);
            if (relevantEffects.Count() == 0)
            {
                return float.PositiveInfinity;
            }

            var coveringActionCount = GetCoveringActionCount(unsatisfiedGoalElements, relevantEffects);
            if (coveringActionCount == -1)
            {
                return float.PositiveInfinity;
            }
            else
            {
                return coveringActionCount;
            }
        }

        private static HashSet<Literal> GetUnsatisfiedGoalElements(State state, Goal goal)
        {
            ////// At some point might want to test whether the cost of keeping elements ordered outweighs the cost of having to do stuff like this the long way..
            ////var uncovered = new HashSet<Literal>();
            ////foreach (var goalElement in goal.Elements)
            ////{
            ////    if (goalElement.IsPositive && !state.Elements.Contains(goalElement.Predicate)
            ////        || goalElement.IsNegated && state.Elements.Contains(goalElement.Predicate))
            ////    {
            ////        uncovered.Add(goalElement);
            ////    }
            ////}
            ////return uncovered;

            var uncovered = new HashSet<Literal>(goal.Elements);
            foreach (var goalElement in goal.Elements)
            {
                // Unifying here because positive elements of the goal can include variables..
                if (goalElement.IsPositive && state.Elements.Any(e => LiteralUnifier.TryCreate(goalElement, e, out var _)))
                {
                    uncovered.Remove(goalElement);
                }
                else if (goalElement.IsNegated && !state.Elements.Contains(goalElement.Predicate))
                {
                    uncovered.Remove(goalElement);
                }
            }
            return uncovered;
        }

        /// <summary>
        /// Gets the transformed effect of each action (given the terms in the goal)
        /// whose effect matches at least one element of the goal
        /// </summary>
        /// <param name="goal"></param>
        /// <returns></returns>
        private IEnumerable<Effect> GetRelevantEffects(IEnumerable<Literal> goalElements, IEnumerable<Predicate> stateElements)
        {
            foreach (var goalElement in goalElements)
            {
                foreach (var actionSchema in problem.Domain.Actions)
                {
                    foreach (var effectElement in actionSchema.Effect.Elements)
                    {
                        if (LiteralUnifier.TryCreate(effectElement, goalElement, out var unifier))
                        {
                            var transformedEffectElement = new VariableSubstitutionEffectTransformation(unifier).ApplyTo(actionSchema.Effect);

                            var message = $"Goal {goalElement} matched by effect {transformedEffectElement} from {actionSchema.Identifier}({string.Join(",", unifier.Bindings.Select(kvp => $"{kvp.Key}:{kvp.Value}"))})";
                            Debug.WriteLine(message);

                            yield return transformedEffectElement;
                        }
                    }
                }

                ////// could of course do this in getunsatisfied goal elements - but having experimented with it a bit,
                ////// it really feels like they should count against the cost.. todo: alternative and benchmarking..
                ////foreach (var stateElement in stateElements)
                ////{
                ////    if (LiteralUnifier.TryCreate(stateElement, goalElement, out var unifier))
                ////    {
                ////        yield return new Effect(goalElement);
                ////    }
                ////}
            }
        }

        /// <summary>
        /// Implements a greedy set cover algorithm of the (effects of) the given actions over the given target set of literals.
        /// Returns the count of how many actions were needed, or -1 if it failed.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="relevantActions"></param>
        /// <returns></returns>
        private static int GetCoveringActionCount(IEnumerable<Literal> target, IEnumerable<Effect> relevantEffects)
        {
            var uncovered = new HashSet<Literal>(target);
            var coveringActionCount = 0;

            while (uncovered.Count > 0)
            {
                // The best match is the one that intersects the most with the remaining uncovered literals:
                var bestMatch = relevantEffects.MaxBy(e => e.Elements.Intersect(uncovered).Count);

                // Yeah, a repeat calculation. Succinct code (LINQ-y goodness..) over actual efficiency is fine
                // given that the purpose of this lib is learning and experimentation, not production-ready code.
                if (bestMatch.Elements.Intersect(uncovered).Count > 0) 
                {
                    coveringActionCount++;
                    uncovered.ExceptWith(bestMatch.Elements);
                }
                else
                {
                    // we couldn't cover any more elements - unsolvable!
                    return -1;
                }
            }

            return coveringActionCount;
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
