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
        private bool levelledOff = false;
        private int levelledOffAtLayer = int.MaxValue;

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
        /// Gets the (ground!) literals that are present at a given level of the graph - 
        /// with level 0 being the initial state.
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public IEnumerable<(Literal Proposition, IEnumerable<Literal> Mutexes)> GetPropositionsWithMutexes(int level)
        {
            return GetPropositionLevel(level).Select(kvp => (kvp.Key, kvp.Value.Mutexes.Select(e => e.Proposition)));
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
        /// Gets the actions that are present at a given level of the graph - 
        /// with level 0 being the initial state.
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public IEnumerable<(Action Action, IEnumerable<Action> Mutexes)> GetActionsWithMutexes(int level)
        {
            return GetActionLevel(level).Select(kvp => (kvp.Key, kvp.Value.Mutexes.Select(e => e.Action)));
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
                if (level == levelledOffAtLayer)
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
                if (level == levelledOffAtLayer)
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
        bool SetPresentWithNoMutexesAt(IEnumerable<Literal> propositions, int level)
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
            while (expandedToLevel < level && !levelledOff)
            {
                PopulateNextLevel();
            }

            return propositionLevels[Math.Min(level, levelledOffAtLayer)];
        }

        private Dictionary<Action, ActionNode> GetActionLevel(int level)
        {
            while (expandedToLevel < level + 1 && !levelledOff)
            {
                PopulateNextLevel();
            }

            return actionLevels[Math.Min(level, levelledOffAtLayer)];
        }

        private void PopulateNextLevel()
        {
            var currentPropositionLevel = propositionLevels[expandedToLevel];

            var newActionLevel = new Dictionary<Action, ActionNode>();
            var newPropositionLevel = new Dictionary<Literal, PropositionNode>();
            var changesOccured = false;

            // We don't need propositions to link back to the actions that they are the effects of *long-term*
            // (i.e. included in the graph), but we do need it temporarily to establish mutexes. So just use a 
            // dictionary. Yeah, a bit hacky - efficiency improvements to follow at some point. Maybe.
            var newActionsByNewProposition = new Dictionary<Literal, List<Action>>();

            // First we need to get all applicable actions from the "state" defined by the current layer.
            // TODO: indexing support? somehow keying positive versus negative feels useful? Meh, slow anyway..
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
                        newActionsByNewProposition[effectElement] = new List<Action>();
                    }

                    // Link the new action node to nodes of its effect elements:
                    actionNode.Effects.Add(propositionNode);

                    // also make a (temporary!) note of the actions that led to each effect - for mutex calculation
                    newActionsByNewProposition[effectElement].Add(action);

                    // Make a note if this effect isn't in the current layer - it means that the graph hasn't levelled off yet:
                    if (!currentPropositionLevel.ContainsKey(effectElement))
                    {
                        changesOccured = true;
                    }
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
                    newActionsByNewProposition[proposition] = new List<Action>();
                }

                actionNode.Effects.Add(newPropositionNode);
                newActionsByNewProposition[proposition].Add(action);
            }

            // Add action mutexes
            var actionIndex = 0;
            foreach (var (action, actionNode) in newActionLevel)
            {
                foreach (var (otherAction, otherActionNode) in newActionLevel.Take(actionIndex))
                {
                    // check for inconsistent effects:
                    if (otherAction.Effect.Elements.Overlaps(action.Effect.Elements.Select(l => l.Negate())))
                    {
                        actionNode.Mutexes.Add(otherActionNode);
                        otherActionNode.Mutexes.Add(actionNode);
                    }
                    // check for interference:
                    else if (otherAction.Effect.Elements.Overlaps(action.Precondition.Elements.Select(l => l.Negate())))
                    {
                        actionNode.Mutexes.Add(otherActionNode);
                        otherActionNode.Mutexes.Add(actionNode);
                    }
                    // check for competing needs:
                    else if (otherAction.Precondition.Elements.Overlaps(action.Precondition.Elements.Select(l => l.Negate())))
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
                        foreach (var action in newActionsByNewProposition[proposition])
                        {
                            foreach (var otherAction in newActionsByNewProposition[otherProposition])
                            {
                                if (!newActionLevel[action].Mutexes.Any(m => m.Action.Equals(newActionLevel[otherAction].Action)))
                                {
                                    return false;
                                }
                            }
                        }

                        return true;
                    }

                    // check for negation:
                    if (proposition.Negate().Equals(otherProposition))
                    {
                        propositionNode.Mutexes.Add(otherPropositionNode);
                        otherPropositionNode.Mutexes.Add(propositionNode);
                    }
                    // check for inconsistent support:
                    else if (AllActionsMutex())
                    {
                        propositionNode.Mutexes.Add(otherPropositionNode);
                        otherPropositionNode.Mutexes.Add(propositionNode);
                    }
                }

                propositionIndex++;
            }

            if (!changesOccured)
            {
                levelledOff = true;
                levelledOffAtLayer = expandedToLevel;
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
        /// Representation of the proposition node in a planning graph.
        /// <para/>
        /// NB: We don't make use of SCGraphTheory for the planning graph because none of the algorithms that use 
        /// it query it via graph theoretical algorithms - so it would be needless complexity. Easy enough to change
        /// should we ever want to do that.
        /// </summary>
        private class PropositionNode
        {
            internal PropositionNode(Literal proposition) => Proposition = proposition;

            public Literal Proposition { get; }

            internal Collection<ActionNode> Actions { get; } = new();

            internal Collection<PropositionNode> Mutexes { get; } = new();
        }

        /// <summary>
        /// Representation of the action node in a planning graph.
        /// <para/>
        /// NB: We don't make use of SCGraphTheory for the planning graph because none of the algorithms that use 
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