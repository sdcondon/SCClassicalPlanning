using SCFirstOrderLogic;
using SCFirstOrderLogic.SentenceManipulation;

namespace SCClassicalPlanning.Planning.GraphPlan
{
    /// <summary>
    /// Planning graph representation. 
    /// <para/>
    /// Lazily populated.
    /// </summary>
    public class PlanningGraph
    {
        private readonly Domain domain;
        private readonly List<IEnumerable<Literal>> propositionLayers = new();
        private readonly List<IEnumerable<Action>> actionLayers = new();

        private int expandedToLevel;
        //private bool levelledOff;
        //private int levelsOffAtLayer;

        /// <summary>
        /// Initialises a new instance of the <see cref="PlanningGraph"/> class.
        /// </summary>
        public PlanningGraph(Domain domain, State state, IEnumerable<Constant> objects)
        {
            this.domain = domain;

            // Planning graphs only work with propositions - no variables allowed.
            // So here we iterate every possible ground predicate (by subsituting every combination of known constants
            // for its arguments - add positive if its in the initial state, otherwise negative
            var layer0 = new List<Literal>();
            foreach (var predicateTemplate in domain.Predicates)
            {
                foreach (var substitution in ProblemInspector.GetAllPossibleSubstitutions(objects, predicateTemplate, new VariableSubstitution()))
                {
                    // Ugh - compiler assuming wrong overload - perhaps because conversion is implicit and method is more concrete? Implicit conversion a mistake, I think.
                    Predicate predicate = (Predicate)substitution.ApplyTo(predicateTemplate).ToSentence();

                    if (state.Elements.Contains(predicate))
                    {
                        layer0.Add(predicate);
                    }
                    else
                    {
                        layer0.Add(new Literal(predicate, true));
                    }
                }
            }

            propositionLayers.Add(layer0);
            expandedToLevel = 0; // Jus' for emphasis..
        }


        /// <summary>
        /// Gets the (ground!) literals that are present at a given layer of the graph - 
        /// with layer 0 being the initial state.
        /// </summary>
        /// <param name="step"></param>
        /// <returns></returns>
        public IEnumerable<Literal> GetPropositions(int step)
        {
            while (expandedToLevel < step)
            {
                Expand();
            }

            return propositionLayers[step];
        }

        /// <summary>
        /// Gets the actions that are present at a given layer of the graph - 
        /// with layer 0 being the initial state.
        /// </summary>
        /// <param name="step"></param>
        /// <returns></returns>
        public IEnumerable<Action> GetActions(int step)
        {
            while (expandedToLevel < step + 1)
            {
                Expand();
            }

            return actionLayers[step];
        }

        /// <summary>
        /// Gets the layer in which a given proposition first occurs.
        /// </summary>
        /// <param name="proposition"></param>
        /// <returns></returns>
        public int FindProposition(Literal proposition)
        {
            throw new NotImplementedException();
        }

        private void Expand()
        {
            // get all applicable actions given the "state" defined by the current layer
            var currentState = new State(propositionLayers[expandedToLevel].Where(p => p.IsPositive).Select(p => p.Predicate));// index me!

            throw new NotImplementedException();
            //ProblemInspector.GetApplicableActions(problem, currentState);
        }

#if false
        /// <summary>
        /// Interface for nodes of a planning graph.
        /// </summary>
        public interface INode : INode<INode, IEdge>
        {

        }

        /// <summary>
        /// Interface for nodes of a planning graph.
        /// </summary>
        public interface INode : INode<INode, IEdge>
        {

        }

        public struct PropositionNode : INode
        {

        }

        /// <summary>
        /// Container for information about an edge that connects a <see cref="PropositionNode"/> to
        /// an <see cref="ActionNode"/> in the same layer.
        /// </summary>
        public struct ActionEdge : IEdge
        {

        }

        public struct ActionNode : INode
        {

        }

        /// <summary>
        /// Container for information about an edge that connects an <see cref="ActionNode"/> to
        /// a <see cref="PropositionNode"/> in the next layer.
        /// </summary>
        public struct PropositionEdge : IEdge
        {

        }
#endif

    }
}