using SCClassicalPlanning.ProblemManipulation;
using SCFirstOrderLogic;
using SCFirstOrderLogic.Inference;
using SCFirstOrderLogic.SentenceManipulation;

namespace SCClassicalPlanning.Planning.StateSpaceSearch.Heuristics
{
    /// <summary>
    /// A decorator heuristic that checks whether the goal violates any known invariants
    /// before invoking the inner heuristic. If any invariants are violated, returns <see cref="float.PositiveInfinity"/>.
    /// Intended to be of use for early pruning of unreachable goals when backward searching.
    /// <para/>
    /// NB #1: This heuristic isn't driven by any particular source material, but given that it's a fairly
    /// obvious idea, there could well be some terminology that I'm not using - I may rename/refactor it as and when.
    /// <para/>
    /// NB #2: Checking invariants obviously comes at a performance cost (though fact that goals consist only of unit
    /// clauses likely mitigates this quite a lot - because it means that the negation of the query we ask our KB it
    /// consists only of unit clauses).
    /// The question is whether the benefit it provides outweighs the cost. I do wonder if we can somehow check
    /// only the stuff that has changed.
    /// <para/>
    /// NB #3: Ultimately it should be possible to derive the invariants by examining the problem.
    /// The simplest example of this is if a predicate doesn't appear in any effects. If this is true, the
    /// the occurences of this predicate in the initial state must persist throughout the problem.
    /// Might research / play with this idea at some point.
    /// </summary>
    public class GoalInvariantCheck
    {
        private readonly Func<State, Goal, float> innerHeuristic;
        private readonly IKnowledgeBase knowledgeBase;

        /// <summary>
        /// Initializes a new instance of the <see cref="GoalInvariantCheck"/>.
        /// </summary>
        /// <param name="invariantsKnowledgeBase">A knowledge base containing all of the invariants of the problem.</param>
        /// <param name="innerHeuristic">The inner heuristic to invoke if no invariants are violated by the goal.</param>
        public GoalInvariantCheck(IKnowledgeBase invariantsKnowledgeBase, Func<State, Goal, float> innerHeuristic)
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
            // One would assume that the inner heuristic would return 0 if there are no elements
            // in the goal - but its not our business to shortcut that
            if (goal.Elements.Count > 0)
            {
                var variables = new HashSet<VariableDeclaration>();
                GoalVariableFinder.Instance.Visit(goal, variables);

                // Annoying performance hit - goals are essentially already in CNF, but our knowledge bases want to do the conversion themselves.. Meh, never mind.
                // TODO: Perhaps a ToSentence in Goal? (and others..)
                var goalSentence = goal.Elements.Skip(1).Aggregate(goal.Elements.First().ToSentence(), (c, e) => new Conjunction(c, e.ToSentence()));

                foreach (var variable in variables)
                {
                    goalSentence = new ExistentialQuantification(variable, goalSentence);
                }

                // Note the negation here. We're not asking if the invariants mean that the goal MUST
                // be true (that will of course generally not be the case!), we're asking if the goal
                // CANNOT be true - that is, if its NEGATION must be true.
                if (knowledgeBase.Ask(new Negation(goalSentence)))
                {
                    return float.PositiveInfinity;
                }
            }

            return innerHeuristic(state, goal);
        }

        /// <summary>
        /// Utility class to find <see cref="Constant"/> instances within the elements of a <see cref="SCClassicalPlanning.Goal"/>, and add them to a given <see cref="HashSet{T}"/>.
        /// </summary>
        private class GoalVariableFinder : RecursiveGoalVisitor<HashSet<VariableDeclaration>>
        {
            /// <summary>
            /// Gets a singleton instance of the <see cref="GoalVariableFinder"/> class.
            /// </summary>
            public static GoalVariableFinder Instance { get; } = new();

            /// <inheritdoc/>
            public override void Visit(VariableDeclaration variable, HashSet<VariableDeclaration> variables) => variables.Add(variable);
        }
    }
}
