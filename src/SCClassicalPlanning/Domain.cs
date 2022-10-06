using SCFirstOrderLogic;
using SCFirstOrderLogic.SentenceManipulation;
using SCFirstOrderLogic.SentenceManipulation.Unification;
using System.Collections.ObjectModel;

namespace SCClassicalPlanning
{
    /// <summary>
    /// Container for information about a domain.
    /// <para/>
    /// A domain defines the aspects that are common to of all problems that occur within it.
    /// Specifically, the <see cref="Action"/>s available within it.
    /// </summary>
    public class Domain
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Domain"/> class.
        /// </summary>
        /// <param name="actions">The set of actions that exist within the domain.</param>
        public Domain(IList<Action> actions)
        {
            Actions = new ReadOnlyCollection<Action>(actions);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Domain"/> class.
        /// </summary>
        /// <param name="actions">The set of actions that exist within the domain.</param>
        public Domain(params Action[] actions) : this((IList<Action>)actions) { }

        /// <summary>
        /// Gets the set of actions that exist within the domain.
        /// </summary>
        public ReadOnlyCollection<Action> Actions { get; }

#if false
TODO: more useful than predicates..
        /// <summary>
        /// Gets the set of constants that exist within the domain
        /// </summary>
        public ReadOnlyCollection<Constant> Constants { get; }
#endif

        /// <summary>
        /// Gets the variable substitution that must be made to convert the matching action schema (with the matching identifier)
        /// in the <see cref="Actions"/> collection to a given action.
        /// <para/>
        /// Intended to be useful for succinct output of plan steps. We don't want to "bloat" our action model with this
        /// information (planners won't and shouldn't care what the original variable name was), but it is useful when
        /// producing human-readable information.
        /// <para/>
        /// Note that we are effectively recreating the substitutions built by <see cref="Problem.GetApplicableActions(State)"/>
        /// and <see cref="Problem.GetRelevantActions(Goal)"/> here. An alternative approach would of course be for those methods to
        /// return both the schema and the substitution (rather than just the transformed action), so that the algorithm can keep
        /// track itself if it wants to. For now at least though, I'm prioritising keep the actual planning as lean and mean as
        /// possible over making the action formatting super snappy.
        /// </summary>
        /// <returns>A <see cref="VariableSubstitution"/> that maps the variables as defined in the schema to the terms referred to in the provided action.</returns>
        public VariableSubstitution GetMappingFromSchema(Action action)
        {
            // NB: All rationalisations aside, this is a bit naff. Sort me out.

            // Note that this is mostly awkward due to the unordered nature of elements. if we preserved the order of things in our model classes then
            // matching would of course be much easier. HOWEVER, of course when it comes to equality (super important) we need order NOT to matter.
            // We could of course offer the best of both worlds, but I'm not ready to add any more complexity than is absolutely needed to our model just yet..
            IEnumerable<VariableSubstitution> MatchWithSchemaElements(IEnumerable<Literal> actionElements, IEnumerable<Literal> schemaElements, VariableSubstitution unifier)
            {
                if (!actionElements.Any())
                {
                    yield return unifier;
                }
                else
                {
                    var firstActionElement = actionElements.First();

                    foreach (var schemaElement in schemaElements)
                    {
                        var firstActionElementUnifier = new VariableSubstitution(unifier);

                        if (LiteralUnifier.TryUpdateUnsafe(schemaElement, firstActionElement, firstActionElementUnifier))
                        {
                            foreach (var restOfActionElementsUnifier in MatchWithSchemaElements(actionElements.Skip(1), schemaElements.Except(new[] { schemaElement }), firstActionElementUnifier))
                            {
                                yield return restOfActionElementsUnifier;
                            }
                        }
                    }
                }
            }

            IEnumerable<VariableSubstitution> MatchWithSchema(Action action, Action schema, VariableSubstitution unifier)
            {
                foreach (var preconditionSub in MatchWithSchemaElements(action.Precondition.Elements, schema.Precondition.Elements, unifier))
                {
                    foreach (var sub in MatchWithSchemaElements(action.Effect.Elements, schema.Effect.Elements, unifier))
                    {
                        yield return sub;
                    }
                }
            }

            var schema =
                Actions.SingleOrDefault(a => a.Identifier.Equals(action.Identifier))
                ?? throw new ArgumentException("Action not found! There is no action in the domain with a matching identifier");

            // I suspect its possible for this to return more than one result.. investigate, write tests
            return MatchWithSchema(action, schema, new VariableSubstitution()).Single();
        }
    }
}
