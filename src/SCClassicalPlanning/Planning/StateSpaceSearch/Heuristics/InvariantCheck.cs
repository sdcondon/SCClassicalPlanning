using SCFirstOrderLogic.Inference;

namespace SCClassicalPlanning.Planning.StateSpaceSearch.Heuristics
{
    /// <summary>
    /// A decorator heuristic that checks whether the goal violates any known invariants
    /// before invoking the inner heuristic. If any invariants are violated, returns float.PositiveInfinity.
    /// Intended to be of use for pruning unreachable goals when backward searching.
    /// <para/>
    /// Invariants (i.e. facts that are true in any reachable state of a problem) are an important
    /// concept in classical planning. This heuristic isn't driven by any particular source material,
    /// but given that it's a fairly obvious idea, I'm assuming it has a name - I'll update it to be
    /// more .
    /// <para/>
    /// Note that ultimately it should be possible to derive the invariants by examining the problem.
    /// For example, if a predicate doesn't appear in any effects, the occurences in the initial state
    /// will persist throughout the problem. More complex invariants should be derivable by looking at
    /// what state changes happen together. It'll be interesting to play with this - but in the meantime
    /// this heuristic can be used with hand-crafted invariants.
    /// </summary>
    public class InvariantCheck
    {
        private readonly Func<State, Goal, float> innerHeuristic;
        private readonly IKnowledgeBase knowledgeBase;

        /// <summary>
        /// Initializes a new instance of the <see cref="InvariantCheck"/>.
        /// </summary>
        /// <param name="invariantsKnowledgeBase">A knowledge base containing all of the invariants of the problem.</param>
        /// <param name="innerHeuristic">the inner heuristic to invoke if no invariants are violated by the goal.</param>
        public InvariantCheck(IKnowledgeBase invariantsKnowledgeBase, Func<State, Goal, float> innerHeuristic)
        {
            this.innerHeuristic = innerHeuristic;
            this.knowledgeBase = invariantsKnowledgeBase;
        }

        /// <summary>
        /// Estimates the cost of getting from the given state to a state that satisfies the given goal.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <param name="goal">The goal.</param>
        /// <returns>float.PositiveInfinity if any invariants are violated by the goal. Otherwise, the cost estimated by the inner heuristic.</returns>
        public float EstimateCost(State state, Goal goal)
        {
            foreach (var element in goal.Elements)
            {
                if (knowledgeBase.Ask(element.Negate().ToSentence()))
                {
                    return float.PositiveInfinity;
                }
            }

            return innerHeuristic(state, goal);
        }
    }
}
