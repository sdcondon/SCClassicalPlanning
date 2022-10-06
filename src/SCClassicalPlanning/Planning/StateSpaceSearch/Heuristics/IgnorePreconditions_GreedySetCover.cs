using SCClassicalPlanning.ProblemManipulation;
using SCFirstOrderLogic;
using SCFirstOrderLogic.SentenceManipulation.Unification;
using SCFirstOrderLogic.SentenceManipulation;

namespace SCClassicalPlanning.Planning.StateSpaceSearch.Heuristics
{
    /// <summary>
    /// State space search heuristic that ignores preconditions and uses a greedy set cover algorithm
    /// to provide its estimate. Not admissable, but 
    /// </summary>
    public class IgnorePreconditions_GreedySetCover
    {
        private readonly Problem problem;

        /// <summary>
        /// Initializes a new instance of the <see cref="IgnorePreconditions_GreedySetCover"/> class.
        /// </summary>
        /// <param name="problem">The problem being solved.</param>
        public IgnorePreconditions_GreedySetCover(Problem problem) => this.problem = problem;

        public float EstimateCost(State state, Goal goal)
        {
            // hmm. *almost* what we want, I think.. note 
            var relevantActions = problem.GetRelevantActions(goal); 
            var coveringActionCount = GetCoveringActionCount(GetUnsatisfiedLiterals(state, goal), relevantActions);

            if (coveringActionCount == -1)
            {
                return float.PositiveInfinity;
            }
            else
            {
                return coveringActionCount;
            }
        }

        ////public IEnumerable<Action> GetRelevantActions(Goal goal)
        ////{
        ////    foreach (var actionSchema in problem.Domain.Actions)
        ////    {
        ////        foreach (var effectElement in actionSchema.Effect.Elements)
        ////        {
        ////            // Here we iterate through ALL elements of the goal, trying to find unifications with the effect element.
        ////            // Using some kind of index here would of course speed things up (support for this is a TODO).
        ////            // We return each unification we find immediately - for an effect to be relevant it only needs to match at least one element of the goal.
        ////            foreach (var goalElement in goal.Elements)
        ////            {
        ////                // TODO: using LiteralUnifier is perhaps overkill given that we know we're functionless,
        ////                // but will do for now. (doesn't necessarily cost more..)
        ////                if (LiteralUnifier.TryCreate(goalElement, effectElement, out var unifier))
        ////                {
        ////                    yield return new Action(
        ////                        actionSchema.Identifier,
        ////                        new VariableSubstitutionGoalTransformation(unifier).ApplyTo(actionSchema.Precondition),
        ////                        new VariableSubstitutionEffectTransformation(unifier).ApplyTo(actionSchema.Effect));
        ////                }
        ////            }
        ////        }
        ////    }
        ////}

        private static HashSet<Literal> GetUnsatisfiedLiterals(State state, Goal goal)
        {
            // sloooow..
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
        private static int GetCoveringActionCount(IEnumerable<Literal> target, IEnumerable<Action> relevantActions)
        {
            var uncovered = new HashSet<Literal>(target);
            var coveringActionCount = 0;

            while (uncovered.Count > 0)
            {
                // The best match is the one that intersects the most with the remaining uncovered literals:
                var bestMatch = relevantActions.MaxBy(a => a.Effect.Elements.Intersect(uncovered).Count());

                // Yeah, a repeat calculation. Succinct code (LINQ-y goodness..) over actual efficiency is fine
                // given that the purpose of this lib is learning and experimentation, not production-ready code.
                if (bestMatch.Effect.Elements.Intersect(uncovered).Count() > 0) 
                {
                    coveringActionCount++;
                    uncovered.ExceptWith(bestMatch.Effect.Elements);
                }
                else
                {
                    // we couldn't cover any elements - unsolvable!
                    return -1;
                }
            }

            return coveringActionCount;
        }
    }
}
