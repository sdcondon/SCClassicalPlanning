using SCClassicalPlanning.ProblemManipulation;
using SCFirstOrderLogic;
using SCFirstOrderLogic.SentenceManipulation;
using SCFirstOrderLogic.SentenceManipulation.Unification;

namespace SCClassicalPlanning.Planning.StateSpaceSearch.Heuristics
{
    /// <summary>
    /// State space search heuristic that ignores preconditions and uses a greedy set cover algorithm
    /// to provide its estimate. Not "admissable" (mostly because greedy set cover can overestimate) - 
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
            // hmm. *almost* what we want, I think..
            //var relevantEffects = ProblemInspector.GetRelevantActions(problem, goal).Select(a => a.Effect); 
            var relevantEffects = GetRelevantEffects(goal); 
            var coveringActionCount = GetCoveringActionCount(GetUnsatisfiedLiterals(state, goal), relevantEffects);

            if (coveringActionCount == -1)
            {
                return float.PositiveInfinity;
            }
            else
            {
                return coveringActionCount;
            }
        }

        /// So very very slow.
        public IEnumerable<Effect> GetRelevantEffects(Goal goal)
        {
            foreach (var actionSchema in problem.Domain.Actions)
            {
                foreach (var effectElement in actionSchema.Effect.Elements)
                {
                    foreach (var goalElement in goal.Elements)
                    {
                        if (LiteralUnifier.TryCreate(goalElement, effectElement, out var unifier))
                        {
                            yield return new VariableSubstitutionEffectTransformation(unifier).ApplyTo(actionSchema.Effect);
                        }
                    }
                }
            }
        }

        private static HashSet<Literal> GetUnsatisfiedLiterals(State state, Goal goal)
        {
            // At some point might want to test whether the cost of keeping elements ordered outweighs the cost of having to do stuff like this the long way..
            var uncovered = new HashSet<Literal>();
            foreach (var goalElement in goal.Elements)
            {
                if (goalElement.IsPositive && !state.Elements.Contains(goalElement.Predicate)
                    || goalElement.IsNegated && state.Elements.Contains(goalElement.Predicate))
                {
                    uncovered.Add(goalElement);
                }
            }

            return uncovered;
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
                var bestMatch = relevantEffects.MaxBy(e => e.Elements.Intersect(uncovered).Count());

                // Yeah, a repeat calculation. Succinct code (LINQ-y goodness..) over actual efficiency is fine
                // given that the purpose of this lib is learning and experimentation, not production-ready code.
                if (bestMatch.Elements.Intersect(uncovered).Count() > 0) 
                {
                    coveringActionCount++;
                    uncovered.ExceptWith(bestMatch.Elements);
                }
                else
                {
                    // we couldn't cover any elements - unsolvable!
                    return -1;
                }
            }

            return coveringActionCount;
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
