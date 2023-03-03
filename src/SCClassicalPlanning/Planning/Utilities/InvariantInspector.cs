// Copyright 2022-2023 Simon Condon
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
using SCClassicalPlanning.ProblemManipulation;
using SCFirstOrderLogic;
using SCFirstOrderLogic.Inference;
using SCFirstOrderLogic.Inference.Resolution;
using System.Diagnostics;

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
            // NB For "true" values, we *could* be looking for a non-trivial subset of this goal as well.
            // We'd probably want a structure other than a hash table for that, though (something trie-like).
            // If we did it, we'd also be justified in removing all existing supersets when adding a goal with a "true" value.
            // Vice-versa (i.e switch subset and superset in the above) for "false" values.
            // except its not just sub or supersets here, is it? - its subsuming/subsumed goals.
            if (!isPrecludedGoalResultCache.TryGetValue(goal, out bool isPrecludedGoal))
            {
                var variables = new HashSet<VariableDeclaration>();
                GoalVariableFinder.Instance.Visit(goal, variables);

                // NB: can safely skip here because otherwise the goal is empty - and we initialise
                // the result cache with the empty goal - so the TryGetValue above would have succeeded.
                // TODO-SCFIRSTORDERLOGIC-MAYBE: Annoying performance hit - goals are essentially already in CNF,
                // but our knowledge bases want to do the conversion themselves.. Meh, never mind.
                // TODO: Perhaps a ToSentence in Goal? (and others..)
                var goalSentence = goal.Elements.Skip(1).Aggregate(goal.Elements.First().ToSentence(), (c, e) => new Conjunction(c, e.ToSentence()));

                foreach (var variable in variables)
                {
                    goalSentence = new ExistentialQuantification(variable, goalSentence);
                }

                // Note the negation here. We're not asking if the invariants mean that the goal MUST
                // be true (that will of course generally not be the case!), we're asking if the goal
                // CANNOT be true - that is, if its NEGATION must be true.
#if true
                isPrecludedGoal = isPrecludedGoalResultCache[goal] = await invariantsKB.AskAsync(new Negation(goalSentence), cancellationToken);
#else // temp...
                Stopwatch sw = Stopwatch.StartNew();
                var query = await invariantsKB.CreateQueryAsync(new Negation(goalSentence), cancellationToken);
                isPrecludedGoal = isPrecludedGoalResultCache[goal] = await query.ExecuteAsync(cancellationToken);
                sw.Stop();
                if (isPrecludedGoal)
                {
                    Debug.WriteLine($"GOAL {goal} PRECLUDED BY INVARIANTS IN {sw.Elapsed}");
                    Debug.WriteLine(((ResolutionQuery)query).ResultExplanation);
                }
#endif
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
                    var elementSentence = element.ToSentence();

                    var variables = new HashSet<VariableDeclaration>();
                    GoalVariableFinder.Instance.Visit(element, variables);

                    foreach (var variable in variables)
                    {
                        elementSentence = new UniversalQuantification(variable, elementSentence);
                    }

#if true
                    isTrivialElement = isTrivialElementResultCache[element] = await invariantsKB.AskAsync(elementSentence, cancellationToken);
#else // temp...
                    Stopwatch sw = Stopwatch.StartNew();
                    var query = await invariantsKB.CreateQueryAsync(elementSentence, cancellationToken);
                    isTrivialElement = await query.ExecuteAsync(cancellationToken);
                    sw.Stop();
                    if (isTrivialElement)
                    {
                        Debug.WriteLine($"ELEMENT {element} TRIVIAL BY INVARIANTS IN {sw.Elapsed}");
                        Debug.WriteLine(((ResolutionQuery)query).ResultExplanation);
                    }
#endif
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
