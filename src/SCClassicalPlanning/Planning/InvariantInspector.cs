using SCClassicalPlanning.ProblemManipulation;
using SCFirstOrderLogic;
using SCFirstOrderLogic.Inference;

namespace SCClassicalPlanning.Planning
{
    public class InvariantInspector
    {
        public static bool IsGoalPrecludedByInvariants(Goal goal, IKnowledgeBase invariantsKB)
        {
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
                if (invariantsKB.Ask(new Negation(goalSentence)))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Removes any elements from the goal that are entailed by the invariants of the problem.
        /// </summary>
        /// <param name="goal"></param>
        /// <param name="invariantsKB"></param>
        /// <returns>A goal with all of the trivial elements removed.</returns>
        public static Goal RemoveTrivialElements(Goal goal, IKnowledgeBase invariantsKB)
        {
            var modified = false;
            var elements = goal.Elements;
            foreach (var element in goal.Elements)
            {
                if (invariantsKB.Ask(element.ToSentence()))
                {
                    elements = elements.Remove(element);
                    modified = true;
                }
            }

            if (modified)
            {
                return new(elements);
            }
            else
            {
                return goal;
            }
        }

        private class GoalVariableFinder : RecursiveGoalVisitor<HashSet<VariableDeclaration>>
        {
            public static GoalVariableFinder Instance { get; } = new();

            public override void Visit(VariableDeclaration variable, HashSet<VariableDeclaration> variables) => variables.Add(variable);
        }
    }
}
