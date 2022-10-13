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
        /// <param name="state">The state to start from.</param>
        public PlanningGraph(Problem problem, State state)
        {
            this.problem = problem;

            // Planning graphs only work with propositions - no variables allowed.
            // So here we iterate every possible ground predicate (by subsituting every combination of known constants
            // for its arguments - add positive if its in the initial state, otherwise negative
            var propositionLevel0 = new Dictionary<Literal, PropositionNode>();
            foreach (var predicateTemplate in problem.Domain.Predicates)
            {
                foreach (var substitution in ProblemInspector.GetAllPossibleSubstitutions(problem.Objects, predicateTemplate, new VariableSubstitution()))
                {
                    // Ugh - compiler assuming wrong overload - perhaps because conversion is implicit and method is more concrete?
                    // Implicit conversion of predicate to literal a mistake, I think.
                    var predicate = (Predicate)substitution.ApplyTo(predicateTemplate).ToSentence();
                    var proposition = new Literal(predicate, !state.Elements.Contains(predicate));
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
            return GetPropositionLevel(level).Select(kvp => (kvp.Key, kvp.Value.Edges.OfType<PropositionMutexEdge>().Select(e => e.To.Proposition)));
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
            return GetActionLevel(level).Select(kvp => (kvp.Key, kvp.Value.Edges.OfType<ActionMutexEdge>().Select(e => e.To.Action)));
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
            bool SetPresentWithNoMutexesAt(int level)
            {
                var i = 0; // bleugh. first pass..
                foreach (var proposition in propositions)
                {
                    if (!GetPropositionLevel(level).TryGetValue(proposition, out var node))
                    {
                        return false;
                    }

                    if (node.Edges.OfType<PropositionMutexEdge>().Select(e => e.To.Proposition).Intersect(propositions.Take(i)).Any())
                    {
                        return false;
                    }

                    i++;
                }

                return true;
            }

            // Meh, will do for now - yes its a loop, but each layer is a dictionary
            var level = 0;
            while (!SetPresentWithNoMutexesAt(level))
            {
                if (level == levelledOffAtLayer)
                {
                    return -1;
                }

                level++;
            }

            return level;
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

            // We don't need propositions to link back to the actions that they are the effects of long-term
            // (i.e. included in the graph), but we do need it temporarily to establish mutexes. So just use a 
            // dictionary. Efficiency improvements to follow at some point. Maybe.
            var newActionsByNewProposition = new Dictionary<Literal, List<Action>>();

            // First we need to get all applicable actions from the "state" defined by the current layer.
            // TODO: indexing support? somehow keying positive versus negative feels useful? Meh, slow anyway..
            var currentState = new State(currentPropositionLevel.Keys.Where(p => p.IsPositive).Select(p => p.Predicate));

            // Now we iterate all those applicable actions - ultimately to build the next action and proposition layers.
            foreach (var action in ProblemInspector.GetApplicableActions(problem, currentState))
            {
                // Add an action node to the new action layer:
                var actionNode = new ActionNode(action);
                newActionLevel.Add(action, actionNode);

                // Link all of the preconditions to the new action node:
                foreach (var preconditionElement in action.Precondition.Elements)
                {
                    var preconditionElementNode = currentPropositionLevel[preconditionElement];
                    preconditionElementNode.Edges.Add(new ActionEdge(preconditionElementNode, actionNode));
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
                    actionNode.Edges.Add(new PropositionEdge(actionNode, propositionNode));

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
            foreach (var kvp in currentPropositionLevel)
            {
                // Add a no-op action & link its precondition
                var action = MakeNoOp(kvp.Key);
                var actionNode = new ActionNode(action);
                newActionLevel.Add(action, actionNode);
                kvp.Value.Edges.Add(new ActionEdge(kvp.Value, actionNode));

                // Make a note if this action isn't in the current layer - it means that the graph hasn't levelled off yet:
                // NB: ..because looking at the propositions isn't enough - different actions could lead to the same
                // propositions WITH DIFFERENT MUTEXES
                if (expandedToLevel == 0 || !actionLevels[expandedToLevel - 1].ContainsKey(action))
                {
                    changesOccured = true;
                }

                // Create a new proposition node if we need to:
                if (!newPropositionLevel.TryGetValue(kvp.Key, out var newPropositionNode))
                {
                    newPropositionNode = newPropositionLevel[kvp.Key] = new PropositionNode(kvp.Key);
                    newActionsByNewProposition[kvp.Key] = new List<Action>();
                }

                actionNode.Edges.Add(new PropositionEdge(actionNode, newPropositionNode));
                newActionsByNewProposition[kvp.Key].Add(action);
            }

            // Add action mutexes
            int i = 0; // bleugh
            foreach (var kvp in newActionLevel)
            {
                var (action, actionNode) = (kvp.Key, kvp.Value);

                foreach (var otherKvp in newActionLevel.Take(i))
                {
                    var (otherAction, otherActionNode) = (otherKvp.Key, otherKvp.Value);

                    // check for inconsistent effects:
                    if (otherAction.Effect.Elements.Overlaps(action.Effect.Elements.Select(l => l.Negate())))
                    {
                        actionNode.Edges.Add(new ActionMutexEdge(actionNode, otherActionNode)); // could include type of mutex in edge, but meh
                        otherActionNode.Edges.Add(new ActionMutexEdge(otherActionNode, actionNode));
                    }
                    // check for interference:
                    else if (otherAction.Effect.Elements.Overlaps(action.Precondition.Elements.Select(l => l.Negate())))
                    {
                        actionNode.Edges.Add(new ActionMutexEdge(actionNode, otherActionNode));
                        otherActionNode.Edges.Add(new ActionMutexEdge(otherActionNode, actionNode));
                    }
                    // check for competing needs:
                    else if (otherAction.Precondition.Elements.Overlaps(action.Precondition.Elements.Select(l => l.Negate())))
                    {
                        actionNode.Edges.Add(new ActionMutexEdge(actionNode, otherActionNode));
                        otherActionNode.Edges.Add(new ActionMutexEdge(otherActionNode, actionNode));
                    }
                }

                i++;
            }

            // Add proposition mutexes
            i = 0;
            foreach (var kvp in newPropositionLevel)
            {
                var (proposition, propositionNode) = (kvp.Key, kvp.Value);

                foreach (var otherKvp in newPropositionLevel.Take(i))
                {
                    var (otherProposition, otherPropositionNode) = (otherKvp.Key, otherKvp.Value);

                    bool AllActionsMutex()
                    {
                        foreach (var action in newActionsByNewProposition[proposition])
                        {
                            foreach (var otherAction in newActionsByNewProposition[otherProposition])
                            {
                                if (!newActionLevel[action].Edges.Any(e => e is ActionMutexEdge mutex && mutex.To.Action.Equals(newActionLevel[otherAction].Action)))
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
                        propositionNode.Edges.Add(new PropositionMutexEdge(propositionNode, otherPropositionNode));
                        otherPropositionNode.Edges.Add(new PropositionMutexEdge(otherPropositionNode, propositionNode));
                    }
                    // check for inconsistent support:
                    else if (AllActionsMutex())
                    {
                        propositionNode.Edges.Add(new PropositionMutexEdge(propositionNode, otherPropositionNode));
                        otherPropositionNode.Edges.Add(new PropositionMutexEdge(otherPropositionNode, propositionNode));
                    }
                }

                i++;
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
        public static Action MakeNoOp(Literal proposition) => new("NOOP", new(proposition), new(proposition));

        /// <summary>
        /// Interface for nodes of a planning graph.
        /// <para/>
        /// TODO: Empty interface is obviously bad design. We should probably consider the precondition-and-effect graph and the mutex graph
        /// as two separate graphs (that happen to contain the same nodes). For the next pass. Although, more to the point - unless any of the
        /// algorithms actually use this as a graph (i.e. apply graph algorithms to it), there's absolutely no point in modelling it as one..
        /// </summary>
        public interface INode : INode<INode, IEdge>
        {
        }

        /// <summary>
        /// Interface for edges of a planning graph.
        /// <para/>
        /// TODO: Empty interface is obviously bad design. We should probably consider the precondition-and-effect graph and the mutex graph
        /// as two separate graphs (that happen to contain the same nodes). For the next pass. Although, more to the point - unless any of the
        /// algorithms actually use this as a graph (i.e. apply graph algorithms to it), there's absolutely no point in modelling it as one..
        /// </summary>
        public interface IEdge : IEdge<INode, IEdge>
        {
        }

        public class PropositionNode : INode
        {
            internal PropositionNode(Literal proposition) => Proposition = proposition;

            public Literal Proposition { get; }

            internal Collection<IEdge> Edges { get; } = new Collection<IEdge>();

            IReadOnlyCollection<IEdge> INode<INode, IEdge>.Edges => Edges;
        }

        /// <summary>
        /// Container for information about an edge that connects a <see cref="PropositionNode"/> to
        /// an <see cref="ActionNode"/> in the same layer.
        /// </summary>
        public class ActionEdge : IEdge
        {
            internal ActionEdge(PropositionNode from, ActionNode to) => (From, To) = (from, to);

            public PropositionNode From { get; }

            public ActionNode To { get; }

            INode IEdge<INode, IEdge>.From => From;

            INode IEdge<INode, IEdge>.To => To;
        }

        public class ActionMutexEdge : IEdge
        {
            internal ActionMutexEdge(ActionNode from, ActionNode to) => (From, To) = (from, to);

            public ActionNode From { get; }

            public ActionNode To { get; }

            INode IEdge<INode, IEdge>.From => From;

            INode IEdge<INode, IEdge>.To => To;
        }

        public class PropositionMutexEdge : IEdge
        {
            internal PropositionMutexEdge(PropositionNode from, PropositionNode to) => (From, To) = (from, to);

            public PropositionNode From { get; }

            public PropositionNode To { get; }

            INode IEdge<INode, IEdge>.From => From;

            INode IEdge<INode, IEdge>.To => To;
        }

        public class ActionNode : INode
        {
            internal ActionNode(Action action) => Action = action;

            public Action Action { get; }

            internal Collection<IEdge> Edges { get; } = new Collection<IEdge>();

            IReadOnlyCollection<IEdge> INode<INode, IEdge>.Edges => Edges;
        }

        /// <summary>
        /// Container for information about an edge that connects an <see cref="ActionNode"/> to
        /// a <see cref="PropositionNode"/> in the next layer.
        /// </summary>
        public class PropositionEdge : IEdge
        {
            internal PropositionEdge(ActionNode from, PropositionNode to) => (From, To) = (from, to);

            public ActionNode From { get; }

            public PropositionNode To { get; }

            INode IEdge<INode, IEdge>.From => From;

            INode IEdge<INode, IEdge>.To => To;
        }
    }
}