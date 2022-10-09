using SCFirstOrderLogic;
using SCFirstOrderLogic.Inference;

namespace SCClassicalPlanning.Planning.StateSpaceSearch.Heuristics
{
    /// <summary>
    /// A decorator heuristic that checks whether the goal violates any known invariants
    /// before invoking the inner heuristic. If any invariants are violated, returns <see cref="float.PositiveInfinity"/>.
    /// Intended to be of use for early pruning of unreachable goals when backward searching.
    /// <para/>
    /// This heuristic isn't driven by any particular source material, but given that it's a fairly
    /// obvious idea, I'm assuming the approach has a name - I'll update it to use standard terminology as and when.
    /// <para/>
    /// Note that ultimately it should be possible to derive the invariants by examining the problem.
    /// The simplest example of this is if a predicate doesn't appear in any effects. If this is true, the
    /// the occurences of this predicate in the initial state must persist throughout the problem.
    /// Might research / play with this idea at some point.
    /// </summary>
    public class InvariantCheck
    {
        private readonly Func<State, Goal, float> innerHeuristic;
        private readonly IKnowledgeBase knowledgeBase;

        /// <summary>
        /// Initializes a new instance of the <see cref="InvariantCheck"/>.
        /// </summary>
        /// <param name="invariantsKnowledgeBase">A knowledge base containing all of the invariants of the problem.</param>
        /// <param name="innerHeuristic">The inner heuristic to invoke if no invariants are violated by the goal.</param>
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
        /// <returns><see cref="float.PositiveInfinity"/> if any invariants are violated by the goal. Otherwise, the cost estimated by the inner heuristic.</returns>
        public float EstimateCost(State state, Goal goal)
        {
            if (goal.Elements.Count == 0)
            {
                return 0;
            }

            // Performance hit - goals are essentially already in CNF, but our knowledge bases want to do the conversion themselves.. Meh, never mind.
            // TODO: Perhaps a ToSentence in Goal? (and others..)
            var goalSentence = goal.Elements.Skip(1).Aggregate(goal.Elements.First().ToSentence(), (c, e) => new Conjunction(c, e.ToSentence()));

            // Note the negation here. We're not asking if the invariants mean that the goal MUST
            // be true (that will of course generally not be the case!), we're asking if the goal
            // CANNOT be true - that is, if its NEGATION must be true.
            // TODO: issues with variables in the goal here - which in goals are effectively existentially
            // quantified, but KBs will i think assume universal. Q.. i *do* wonder if taking the Skolem
            // function approach for goal vars is a more justifiable approach? But the book makes a
            // big deal of CP being functionless..
            if (knowledgeBase.Ask(new Negation(goalSentence)))
            {
                return float.PositiveInfinity;
            }

            return innerHeuristic(state, goal);
        }
    }
}
