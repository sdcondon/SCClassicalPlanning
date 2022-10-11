using SCFirstOrderLogic;
using SCFirstOrderLogic.SentenceManipulation;
using SCGraphTheory;

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
        private readonly List<IEnumerable<Literal>> propositionLayers = new();
        private readonly List<IEnumerable<Action>> actionLayers = new();

        private int expandedToLevel;
        //private bool levelledOff;
        //private int levelsOffAtLayer;

        /// <summary>
        /// Initialises a new instance of the <see cref="PlanningGraph"/> class.
        /// </summary>
        /// <param name="problem">The problem that the planning graph represents.</param>
        public PlanningGraph(Problem problem)
        {
            this.problem = problem;

            // Planning graphs only work with propositions - no variables allowed.
            // So here we iterate every possible ground predicate (by subsituting every combination of known constants
            // for its arguments - add positive if its in the initial state, otherwise negative
            var layer0 = new List<Literal>();
            foreach (var predicateTemplate in problem.Domain.Predicates)
            {
                foreach (var substitution in ProblemInspector.GetAllPossibleSubstitutions(problem, predicateTemplate, new VariableSubstitution()))
                {
                    Predicate predicate = (Predicate)substitution.ApplyTo(predicateTemplate).ToSentence(); // Ugh - why is compiler assuming this overload? because conversion is implicit and method is more concrete?

                    if (problem.InitialState.Elements.Contains(predicate))
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
            expandedToLevel = 0;
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
                throw new NotImplementedException();
            }

            return propositionLayers[step];
        }

#if false
        /// <summary>
        /// Gets the actions that are available 
        /// </summary>
        /// <param name="step"></param>
        /// <returns></returns>
        public IEnumerable<Action> GetActions(int step)
        {
        }

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