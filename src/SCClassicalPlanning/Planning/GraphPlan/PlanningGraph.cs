using SCFirstOrderLogic;
using SCFirstOrderLogic.SentenceManipulation;
using SCGraphTheory;
using System.Collections.ObjectModel;

namespace SCClassicalPlanning.Planning.GraphPlan
{
    /// <summary>
    /// Planning graph representation. 
    /// <para/>
    /// Lazily populated.
    /// </summary>
    public class PlanningGraph
    {
        private readonly Problem problem;
        private readonly List<Dictionary<Literal, PropositionNode>> propositionLevels = new();
        private readonly List<Dictionary<Action, ActionNode>> actionLevels = new();

        private int expandedToLevel = 0;

        /// <summary>
        /// Initialises a new instance of the <see cref="PlanningGraph"/> class.
        /// </summary>
        /// <param name="problem">The problem being solved.</param>
        public PlanningGraph(Problem problem)
        {
            this.problem = problem;

            // Planning graphs only work with propositions - no variables allowed.
            // So here we iterate every possible ground predicate (by subsituting every combination of known constants
            // for its arguments - add positive if it's in the initial state, otherwise negative
            var propositionLevel0 = new Dictionary<Literal, PropositionNode>();
            foreach (var predicateTemplate in problem.Domain.Predicates)
            {
                foreach (var substitution in ProblemInspector.GetAllPossibleSubstitutions(problem.Objects, predicateTemplate, new VariableSubstitution()))
                {
                    // Ugh - compiler assuming wrong overload - perhaps because conversion is implicit and method is more concrete?
                    // Implicit conversion of predicate to literal a mistake, I think.
                    var predicate = (Predicate)substitution.ApplyTo(predicateTemplate).ToSentence();
                    var proposition = new Literal(predicate, !problem.InitialState.Elements.Contains(predicate));
                    propositionLevel0.Add(proposition, new PropositionNode(proposition));
                }
            }

            propositionLevels.Add(propositionLevel0);
        }

        /// <summary>
        /// Gets a value indicating whether the graph has levelled off.
        /// </summary>
        public bool LevelledOff => LevelledOffAtLevel.HasValue;

        /// <summary>
        /// Gets the level at which the graph levelled off - or null if the graph has not yet levelled off.
        /// </summary>
        public int? LevelledOffAtLevel { get; private set; } = null;

        /// <summary>
        /// Gets the (ground!) literals that are present at a given level of the graph - 
        /// with level 0 being the initial state.
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public IEnumerable<Literal> GetPropositions(int level)
        {
            return GetPropositionLevel(level).Keys;
        }

        /// <summary>
        /// Gets the actions that are present at a given level of the graph - 
        /// with level 0 being the initial state.
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public IEnumerable<Action> GetActions(int level)
        {
            return GetActionLevel(level).Keys;
        }

        /// <summary>
        /// Gets the level at which a given proposition first occurs.
        /// </summary>
        /// <param name="proposition">The proposition to look for.</param>
        /// <returns>The level at which the given proposition first occurs.</returns>
        public int GetLevel(Literal proposition)
        {
            // meh, will do for now - yes its a loop, but each layer is a dictionary
            var level = 0;
            while (!GetPropositionLevel(level).ContainsKey(proposition))
            {
                if (level == LevelledOffAtLevel)
                {
                    return -1;
                }

                level++;
            }

            return level;
        }

        /// <summary>
        /// Gets the level at which all of a set of propositions first occurs, with no pair being mutually exclusive.
        /// </summary>
        /// <param name="propositions">The set of propositions to look for.</param>
        /// <returns>The level at which all of a set of propositions first occurs, with no pair being mutually exclusive.</returns>
        public int GetSetLevel(IEnumerable<Literal> propositions)
        {
            // Meh, will do for now - yes its a loop, but each layer is a dictionary
            var level = 0;
            while (!SetPresentWithNoMutexesAt(propositions, level))
            {
                if (level == LevelledOffAtLevel)
                {
                    return -1;
                }

                level++;
            }

            return level;
        }

        /// <summary>
        /// Gets a value indicating whether all of a set of propositions are present at a given level,
        /// with no pair of them being mutually exclusive.
        /// </summary>
        /// <param name="propositions">The propositions to look for.</param>
        /// <param name="level">The graph level to consider.</param>
        /// <returns></returns>
        public bool SetPresentWithNoMutexesAt(IEnumerable<Literal> propositions, int level)
        {
            var propositionIndex = 0;
            foreach (var proposition in propositions)
            {
                if (!GetPropositionLevel(level).TryGetValue(proposition, out var node))
                {
                    return false;
                }

                if (node.Mutexes.Select(e => e.Proposition).Intersect(propositions.Take(propositionIndex)).Any())
                {
                    return false;
                }

                propositionIndex++;
            }

            return true;
        }

        private Dictionary<Literal, PropositionNode> GetPropositionLevel(int level)
        {
            while (expandedToLevel < level && !LevelledOff)
            {
                MakeNextLevel();
            }

            return propositionLevels[Math.Min(level, LevelledOffAtLevel ?? int.MaxValue)];
        }

        private Dictionary<Action, ActionNode> GetActionLevel(int level)
        {
            while (expandedToLevel < level + 1 && !LevelledOff)
            {
                MakeNextLevel();
            }

            return actionLevels[Math.Min(level, LevelledOffAtLevel ?? int.MaxValue)];
        }

        private void MakeNextLevel()
        {
            var currentPropositionLevel = propositionLevels[expandedToLevel];

            var newActionLevel = new Dictionary<Action, ActionNode>();
            var newPropositionLevel = new Dictionary<Literal, PropositionNode>();
            var changesOccured = false;

            // First we need to get all applicable actions from the "state" defined by the current layer.
            // indexing support? somehow keying positive versus negative feels useful? Meh, slow anyway..
            var currentState = new State(currentPropositionLevel.Keys.Where(p => p.IsPositive).Select(p => p.Predicate));

            // Now we iterate all those applicable actions - ultimately to build the next action and proposition layers.
            foreach (var action in ProblemInspector.GetApplicableActions(problem, currentState))
            {
                // Add an action node to the new action layer:
                var actionNode = newActionLevel[action] = new ActionNode(action);

                // Link all of the preconditions to the new action node:
                foreach (var preconditionElement in action.Precondition.Elements)
                {
                    var preconditionElementNode = currentPropositionLevel[preconditionElement];
                    preconditionElementNode.Actions.Add(actionNode);
                }

                // Make a note if this action isn't in the current layer - it means that the graph hasn't levelled off yet:
                // NB: ..because looking at the propositions isn't enough - different actions could lead to the same
                // propositions WITH DIFFERENT MUTEXES
                if (expandedToLevel == 0 || !actionLevels[expandedToLevel - 1].ContainsKey(action))
                {
                    changesOccured = true;
                }

                // Iterate all of the action's effect elements to build the next proposition layer:
                foreach (var effectElement in action.Effect.Elements)
                {
                    // Multiple actions can of course have the same effect elements, and we don't want duplicate proposition nodes -
                    // this graph is memory-hogging enough as it is.. So only create a new proposition node if we need to:
                    if (!newPropositionLevel.TryGetValue(effectElement, out var propositionNode))
                    {
                        propositionNode = newPropositionLevel[effectElement] = new PropositionNode(effectElement);

                        // Make a note if this effect isn't in the current layer - it means that the graph hasn't levelled off yet:
                        if (!currentPropositionLevel.ContainsKey(effectElement))
                        {
                            changesOccured = true;
                        }
                    }

                    // Link the new action node to the (*possibly* new) proposition node as an effect,
                    // and link the proposition node back to the action node as a cause.
                    actionNode.Effects.Add(propositionNode);
                    propositionNode.Causes.Add(actionNode);
                }
            }

            // Now we need to add the maintenance/no-op actions:
            foreach (var (proposition, propositionNode) in currentPropositionLevel)
            {
                // Add a no-op action & link its precondition
                var action = MakeNoOp(proposition);
                var actionNode = newActionLevel[action] = new ActionNode(action);
                propositionNode.Actions.Add(actionNode);

                // Make a note if this action isn't in the current layer - it means that the graph hasn't levelled off yet:
                // NB: ..because looking at the propositions isn't enough - different actions could lead to the same
                // propositions WITH DIFFERENT MUTEXES
                if (expandedToLevel == 0 || !actionLevels[expandedToLevel - 1].ContainsKey(action))
                {
                    changesOccured = true;
                }

                // Create a new proposition node if we need to:
                if (!newPropositionLevel.TryGetValue(proposition, out var newPropositionNode))
                {
                    newPropositionNode = newPropositionLevel[proposition] = new PropositionNode(proposition);
                }

                actionNode.Effects.Add(newPropositionNode);
                newPropositionNode.Causes.Add(actionNode);
            }

            // Add action mutexes
            var actionIndex = 0;
            foreach (var (action, actionNode) in newActionLevel)
            {
                foreach (var (otherAction, otherActionNode) in newActionLevel.Take(actionIndex))
                {
                    if (otherAction.Effect.Elements.Overlaps(action.Effect.Elements.Select(l => l.Negate())) // inconsistent effects
                        || otherAction.Effect.Elements.Overlaps(action.Precondition.Elements.Select(l => l.Negate())) // interference
                        || otherAction.Precondition.Elements.Overlaps(action.Precondition.Elements.Select(l => l.Negate()))) // competing needs
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
                    }
                }

                propositionIndex++;
            }

            if (!changesOccured)
            {
                LevelledOffAtLevel = expandedToLevel;
            }
            else
            {
                expandedToLevel++;
                actionLevels.Add(newActionLevel);
                propositionLevels.Add(newPropositionLevel);
            }
        }

        // TODO: strictly speaking, this identifier is not guaranteed to be unique.
        // NB: while an EMPTY goal and effect would at first glance seem to be intuitive - it is
        // defined like this to assist with mutex creation. Feels hacky to me, but this is
        // apparently what is done..
        internal static Action MakeNoOp(Literal proposition) => new("NOOP", new(proposition), new(proposition));

        /// <summary>
        /// Representation of a proposition node in a planning graph.
        /// <para/>
        /// NB: We don't make use of the SCGraphTheory abstraction for the planning graph because none of the algorithms that use 
        /// it query it via graph theoretical algorithms - so it would be needless complexity. Easy enough to change
        /// should we ever want to do that.
        /// </summary>
        private class PropositionNode
        {
            internal PropositionNode(Literal proposition) => Proposition = proposition;

            public Literal Proposition { get; }

            internal Collection<ActionNode> Actions { get; } = new();

            internal Collection<ActionNode> Causes { get; } = new();

            internal Collection<PropositionNode> Mutexes { get; } = new();
        }

        /// <summary>
        /// Representation of an action node in a planning graph.
        /// <para/>
        /// NB: We don't make use of SCGraphTheory abstraction for the planning graph because none of the algorithms that use 
        /// it query it via graph theoretical algorithms - so it would be needless complexity. Easy enough to change
        /// should we ever want to do that.
        /// </summary>
        private class ActionNode
        {
            internal ActionNode(Action action) => Action = action;

            public Action Action { get; }

            internal Collection<PropositionNode> Effects { get; } = new();

            internal Collection<ActionNode> Mutexes { get; } = new();
        }
    }
}