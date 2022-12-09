using SCClassicalPlanning.ProblemManipulation;
using SCFirstOrderLogic;
using SCFirstOrderLogic.Inference;

namespace SCClassicalPlanning.Planning.Utilities
{
    /// <summary>
    /// Utility logic for making use of invariants (that is, statements that hold true in all reachable states of a problem).
    /// </summary>
    // TODO-EXTENSION?: Given that inference can take a while, might be interesting to play with non-trivial asynchronicity here at some point
    // (almost certainly as an extension rather than in this package). That is, create higher-level logic that queues up the methods here
    // and post-hoc prunes/updates search branches as appropriate when they finish.
    public class InvariantInspector
    {
        private readonly IKnowledgeBase invariantsKB;
        private readonly Dictionary<Goal, bool> isPrecludedGoalResultCache = new() { [Goal.Empty] = false };
        private readonly Dictionary<Literal, bool> isTrivialElementResultCache = new();

        /// <summary>
        /// Initialises a new instance of the <see cref="InvariantInspector"/> class.
        /// </summary>
        /// <param name="knowledgeBase">A knowledge base that contains all of the invariants.</param>
        public InvariantInspector(IKnowledgeBase knowledgeBase)
        {
            invariantsKB = knowledgeBase ?? throw new ArgumentNullException(nameof(knowledgeBase));
        }

        /// <summary>
        /// Gets a value indicating whether the invariants mean that a given goal is impossible to achieve.
        /// </summary>
        /// <param name="goal">The goal to check.</param>
        /// <param name="cancellationToken">A cancellation token for the operation.</param>
        /// <returns>A (task that returns a) value indicating whether the invariants mean that the given goal is impossible to achieve.</returns>
        public async Task<bool> IsGoalPrecludedByInvariantsAsync(Goal goal, CancellationToken cancellationToken = default)
        {
            if (!isPrecludedGoalResultCache.TryGetValue(goal, out bool isPrecludedGoal))
            {
                var variables = new HashSet<VariableDeclaration>();
                GoalVariableFinder.Instance.Visit(goal, variables);

                // NB: can safely skip here because otherwise the goal is empty - and we initialise
                // the result cache with the empty goal - so the TryGetValue above would have succeeded.
                // TODO-SCFIRSTORDERLOGIC-MAYBE: Annoying performance hit - goals are essentially already in CNF, but our knowledge bases want to do the conversion themselves.. Meh, never mind.
                // TODO: Perhaps a ToSentence in Goal? (and others..)
                var goalSentence = goal.Elements.Skip(1).Aggregate(goal.Elements.First().ToSentence(), (c, e) => new Conjunction(c, e.ToSentence()));

                foreach (var variable in variables)
                {
                    goalSentence = new ExistentialQuantification(variable, goalSentence);
                }

                // Note the negation here. We're not asking if the invariants mean that the goal MUST
                // be true (that will of course generally not be the case!), we're asking if the goal
                // CANNOT be true - that is, if its NEGATION must be true.
                isPrecludedGoal = isPrecludedGoalResultCache[goal] = await invariantsKB.AskAsync(new Negation(goalSentence), cancellationToken);
            }

            return isPrecludedGoal;
        }

        /// <summary>
        /// Gets a value indicating whether the invariants mean that a given goal is impossible to achieve.
        /// </summary>
        /// <param name="goal">the goal to check.</param>
        /// <param name="cancellationToken">A cancellation token for the operation.</param>
        /// <returns>A value indicating whether the invariants mean that the given goal is impossible to achieve.</returns>
        public bool IsGoalPrecludedByInvariants(Goal goal, CancellationToken cancellationToken = default)
        {
            return IsGoalPrecludedByInvariantsAsync(goal, cancellationToken).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Removes the elements that are entailed by the invariants from a given goal.
        /// </summary>
        /// <param name="goal">The goal to remove trivial elements from.</param>
        /// <param name="cancellationToken">A cancellation token for the operation.</param>
        /// <returns>A (task that returns a) goal with all of the trivial elements removed.</returns>
        public async Task<Goal> RemoveTrivialElementsAsync(Goal goal, CancellationToken cancellationToken = default)
        {
            var modified = false;
            var remainingElements = goal.Elements;

            foreach (var element in goal.Elements)
            {
                if (!isTrivialElementResultCache.TryGetValue(element, out bool isTrivialElement))
                {
                    isTrivialElement = isTrivialElementResultCache[element] = await invariantsKB.AskAsync(element.ToSentence(), cancellationToken);
                }

                if (isTrivialElement)
                {
                    remainingElements = remainingElements.Remove(element);
                    modified = true;
                }
            }

            if (modified)
            {
                return new(remainingElements);
            }
            else
            {
                return goal;
            }
        }

        /// <summary>
        /// Removes the elements that are entailed by the invariants from a given goal.
        /// </summary>
        /// <param name="goal">The goal to remove trivial elements from.</param>
        /// <param name="cancellationToken">A cancellation token for the operation.</param>
        /// <returns>A goal with all of the trivial elements removed.</returns>
        public Goal RemoveTrivialElements(Goal goal, CancellationToken cancellationToken = default)
        {
            return RemoveTrivialElementsAsync(goal, cancellationToken).GetAwaiter().GetResult();
        }

        private class GoalVariableFinder : RecursiveGoalVisitor<HashSet<VariableDeclaration>>
        {
            public static GoalVariableFinder Instance { get; } = new();

            public override void Visit(VariableDeclaration variable, HashSet<VariableDeclaration> variables) => variables.Add(variable);
        }
    }
}
