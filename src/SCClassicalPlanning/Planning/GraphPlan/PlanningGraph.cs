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
using SCClassicalPlanning.Planning.Utilities;
using SCClassicalPlanning.ProblemManipulation;
using SCFirstOrderLogic;
using SCFirstOrderLogic.SentenceManipulation;
using SCFirstOrderLogic.SentenceManipulation.Unification;

namespace SCClassicalPlanning.Planning.GraphPlan
{
    /// <summary>
    /// <para>
    /// Planning graph representation.
    /// </para>
    /// <para>
    /// NB: Lazily populated - levels will be created as they are called for.
    /// </para>
    /// </summary>
    // TODO: Should probably implement IEnumerable<IPlanningGraphLevel>? Or at least have a Levels prop?
    // Or perhaps even IReadOnlyList<PlanningGraphLevel> - though Count is effectively infinite..
    public class PlanningGraph
    {
        /// <summary>
        /// The identifier used for the persistence actions in <see cref="PlanningGraph"/> instances.
        /// </summary>
        // TODO: this identifier is not guaranteed to be unique.
        public const string PersistenceActionIdentifier = "NOOP";

        private readonly Problem problem;
        private readonly List<Dictionary<Literal, PlanningGraphPropositionNode>> propositionLevels = new();
        private readonly List<Dictionary<Action, PlanningGraphActionNode>> actionLevels = new();

        private int currentLevelPropositionMutexCount = 0;
        private int expandedToLevel = 0;
        private int? levelsOffAtLevel = null;

        /// <summary>
        /// Initialises a new instance of the <see cref="PlanningGraph"/> class.
        /// </summary>
        /// <param name="problem">The problem being solved.</param>
        public PlanningGraph(Problem problem)
        {
            this.problem = problem;

            // Planning graphs only work with propositions - no variables allowed.
            // So here we iterate every possible ground predicate (by substituting every combination of known constants
            // for its arguments - add positive if it's in the initial state, otherwise negative)
            var propositionLevel0 = new Dictionary<Literal, PlanningGraphPropositionNode>();
            foreach (var predicateTemplate in problem.Domain.Predicates)
            {
                foreach (var substitution in ProblemInspector.GetAllPossibleSubstitutions(problem, predicateTemplate, new VariableSubstitution()))
                {
                    // Ugh - compiler assuming wrong overload - perhaps because conversion is implicit and method is more concrete?
                    // Implicit conversion of predicate to literal a mistake, I think.
                    var predicate = (Predicate)substitution.ApplyTo(predicateTemplate).ToSentence();
                    var proposition = new Literal(predicate, !problem.InitialState.Elements.Contains(predicate));
                    propositionLevel0.Add(proposition, new PlanningGraphPropositionNode(proposition));
                }
            }

            propositionLevels.Add(propositionLevel0);
        }

        /// <summary>
        /// Gets the index of the level at which the graph levels off. This is the index
        /// of the last level that differs from the one before, after which all further 
        /// levels are identical. Implicitly fully expands the graph if it isn't already.
        /// </summary>
        public int LevelsOffAtLevel
        {
            get
            {
                FullyExpand();
                return levelsOffAtLevel!.Value;
            }
        }

        /// <summary>
        /// Gets the level at which a given proposition first occurs.
        /// </summary>
        /// <param name="proposition">The proposition to look for.</param>
        /// <returns>The level at which the given proposition first occurs, or -1 if it does not occur.</returns>
        public int GetLevelCost(Literal proposition)
        {
            // Meh, will do for now - yes its a loop, but each iteration is a dictionary key lookup
            var level = 0;
            while (!GetLevel(level).Contains(proposition))
            {
                if (level == levelsOffAtLevel)
                {
                    return -1;
                }

                level++;
            }

            return level;
        }

        /// <summary>
        /// Gets the "set level" of a set of propositions. That is, the first level at which all of the propositions occur, with no pair being mutually exclusive.
        /// </summary>
        /// <param name="propositions">The set of propositions to look for.</param>
        /// <returns>The level at which all of a set of propositions first occurs, with no pair being mutually exclusive.</returns>
        public int GetLevelCost(IEnumerable<Literal> propositions)
        {
            // Meh, will do for now - yes its a loop, but.. meh.
            var level = 0;
            while (!GetLevel(level).ContainsNonMutex(propositions))
            {
                if (level == levelsOffAtLevel)
                {
                    return -1;
                }

                level++;
            }

            return level;
        }

        /// <summary>
        /// Retrieves an object representing a particular (proposition) level within the graph. Expands the graph to this level if necessary.
        /// </summary>
        /// <param name="index">The index of the level to retrieve.</param>
        /// <returns>An object representing the level.</returns>
        public PlanningGraphLevel GetLevel(int index)
        {
            while (expandedToLevel < index && !levelsOffAtLevel.HasValue)
            {
                MakeNextLevel();
            }

            return new(this, index);
        }

        /// <summary>
        /// Fully expands the graph to the point at which it levels off - ensuring that future calls to <see cref="GetLevel"/> will be fast.
        /// </summary>
        public void FullyExpand()
        {
            while (!levelsOffAtLevel.HasValue)
            {
                MakeNextLevel();
            }
        }

        internal IReadOnlyDictionary<Literal, PlanningGraphPropositionNode> GetNodesByProposition(int levelIndex)
        {
            return propositionLevels[Math.Min(levelIndex, levelsOffAtLevel ?? int.MaxValue)];
        }

        internal bool IsLevelledOff(int levelIndex)
        {
            return levelIndex > levelsOffAtLevel;
        }

        // TODO-V1: a fair amount of refactoring to be done here..
        private void MakeNextLevel()
        {
            var currentPropositionLevel = new PlanningGraphLevel(this, expandedToLevel);
            var newActionLevel = new Dictionary<Action, PlanningGraphActionNode>();
            var newPropositionLevel = new Dictionary<Literal, PlanningGraphPropositionNode>();
            var newPropositionLevelMutexCount = 0;

            // Iterate all the possible actions - ultimately to build the next action and proposition layers.
            foreach (var action in GetPossibleActions(problem, currentPropositionLevel.Propositions))
            {
                // Add an action node to the new action layer, and link all of its preconditions to it:
                var actionNode = newActionLevel[action] = new PlanningGraphActionNode(action);
                foreach (var preconditionElement in action.Precondition.Elements)
                {
                    var preconditionElementNode = currentPropositionLevel.NodesByProposition[preconditionElement];
                    preconditionElementNode.Actions.Add(actionNode);
                    actionNode.Preconditions.Add(preconditionElementNode);
                }

                // Iterate all of the action's effect elements, and add them to the next proposition layer (if they aren't already there):
                foreach (var effectElement in action.Effect.Elements)
                {
                    // Multiple actions can of course have the same effect elements, and we obviously don't want duplicate
                    // proposition nodes, so only create a new proposition node if we need to:
                    if (!newPropositionLevel.TryGetValue(effectElement, out var propositionNode))
                    {
                        propositionNode = newPropositionLevel[effectElement] = new PlanningGraphPropositionNode(effectElement);
                    }

                    // Link the new action node to the (possibly new) proposition node as an effect,
                    // and link the proposition node back to the action node as a cause.
                    actionNode.Effects.Add(propositionNode);
                    propositionNode.Causes.Add(actionNode);
                }
            }

            // Add action mutexes
            var actionIndex = 0;
            foreach (var (action, actionNode) in newActionLevel)
            {
                foreach (var (otherAction, otherActionNode) in newActionLevel.Take(actionIndex))
                {
                    if (otherAction.Effect.Elements.Overlaps(action.Effect.Elements.Select(l => l.Negate())) // inconsistent effects
                        || otherAction.Effect.Elements.Overlaps(action.Precondition.Elements.Select(l => l.Negate())) // interference
                        || otherAction.Precondition.Elements.Overlaps(action.Effect.Elements.Select(l => l.Negate())) // interference the other way around
                        || otherActionNode.Preconditions.Any(p => p.Mutexes.Any(m => actionNode.Preconditions.Contains(m)))) // competing needs - nb ref equality on node - fine cos no dupes
                    {
                        actionNode.Mutexes.Add(otherActionNode);
                        otherActionNode.Mutexes.Add(actionNode);
                    }
                }

                actionIndex++;
            }

            // Add proposition mutexes
            var propositionIndex = 0;
            foreach (var (proposition, propositionNode) in newPropositionLevel)
            {
                foreach (var (otherProposition, otherPropositionNode) in newPropositionLevel.Take(propositionIndex))
                {
                    bool AllActionsMutex()
                    {
                        foreach (var actionNode in propositionNode.Causes)
                        {
                            foreach (var otherActionNode in otherPropositionNode.Causes)
                            {
                                if (!actionNode.Mutexes.Any(m => m.Action.Equals(otherActionNode.Action)))
                                {
                                    return false;
                                }
                            }
                        }

                        return true;
                    }

                    if (proposition.Negate().Equals(otherProposition) // negation
                        || AllActionsMutex()) // inconsistent support
                    {
                        propositionNode.Mutexes.Add(otherPropositionNode);
                        otherPropositionNode.Mutexes.Add(propositionNode);
                        newPropositionLevelMutexCount++;
                    }
                }

                propositionIndex++;
            }

            if (newPropositionLevel.Count == currentPropositionLevel.NodesByProposition.Count
                && newPropositionLevelMutexCount == currentLevelPropositionMutexCount)
            {
                levelsOffAtLevel = expandedToLevel;
            }
            else
            {
                actionLevels.Add(newActionLevel);
                propositionLevels.Add(newPropositionLevel);
                expandedToLevel++;
                currentLevelPropositionMutexCount = newPropositionLevelMutexCount;
            }
        }

        private static IEnumerable<Action> GetPossibleActions(Problem problem, IEnumerable<Literal> possiblePropositions)
        {
            // Local method to (recursively) match a set of (remaining) goal elements to the possiblePropositions.
            // goalElements: The remaining elements of the goal to be matched
            // unifier: The VariableSubstitution established so far (by matching earlier goal elements)
            // returns: An enumerable of VariableSubstitutions that can be applied to the goal elements to make them match some subset of the possiblePropositions
            IEnumerable<VariableSubstitution> MatchWithPossiblePropositions(IEnumerable<Literal> goalElements, VariableSubstitution unifier)
            {
                if (!goalElements.Any())
                {
                    yield return unifier;
                }
                else
                {
                    // Here we iterate through ALL possible propositions, trying to find unifications with the goal element.
                    // For each unification found, we then recurse for the rest of the elements of the goal.
                    foreach (var proposition in possiblePropositions)
                    {
                        var firstGoalElementUnifier = new VariableSubstitution(unifier);

                        if (LiteralUnifier.TryUpdateUnsafe(proposition, goalElements.First(), firstGoalElementUnifier))
                        {
                            foreach (var restOfGoalElementsUnifier in MatchWithPossiblePropositions(goalElements.Skip(1), firstGoalElementUnifier))
                            {
                                yield return restOfGoalElementsUnifier;
                            }
                        }
                    }
                }
            }

            // The overall task to be accomplished here is to find (action schema, variable substitution) pairings such that
            // some subset of the possible propositions matches the action precondition (after the variable substitution is
            // applied to it). First, we iterate the action schemas:
            foreach (var actionSchema in problem.Domain.Actions)
            {
                // For each, we try to find appropriate variable substitutions, which is what this (recursive) MatchWithState method does:
                foreach (var substitution in MatchWithPossiblePropositions(actionSchema.Precondition.Elements, new VariableSubstitution()))
                {
                    // For each substitution, apply it to the action schema and return it:
                    yield return new VariableSubstitutionActionTransformation(substitution).ApplyTo(actionSchema);
                }
            }

            // Now we need to also yield the persistence ("no-op") actions:
            foreach (var proposition in possiblePropositions)
            {
                // NB: while an EMPTY goal and effect would at first glance seem to be intuitive - it is
                // defined like this to assist with mutex creation, and because of the idiosyncracies of
                // plan extraction in GraphPlan. Still feels awkward to me, but meh, never mind.
                yield return new(PersistenceActionIdentifier, new(proposition), new(proposition));
            }
        }
    }
}