using SCFirstOrderLogic;
using System.Collections.Immutable;
using static SCFirstOrderLogic.SentenceCreation.OperableSentenceFactory;

namespace SCClassicalPlanning.ProblemCreation
{
    /// <summary>
    /// Utility logic for the declaration of <see cref="Problem"/> instances, prioritising succinct C# above all else.
    /// <para/>
    /// Allows for declaring actions with preconditions and effects stated as <see cref="OperableSentenceFactory.OperableSentence"/> instances.
    /// </summary>
    public static class OperableProblemFactory
    {
        public class OperableAction
        {
            public OperableAction(object identifier, OperableGoal precondition, OperableEffect effect) => (Identifier, Precondition, Effect) = (identifier, precondition, effect);

            internal object Identifier { get; }

            internal OperableGoal Precondition { get; }

            internal OperableEffect Effect { get; }

            public static implicit operator OperableAction(Action action) => new(action.Identifier, action.Precondition, action.Effect);

            public static implicit operator Action(OperableAction action) => new(action.Identifier, action.Precondition, action.Effect);
        }

        public class OperableGoal
        {
            internal OperableGoal(IEnumerable<Literal> elements) => Elements = elements.ToImmutableHashSet();

            internal IReadOnlySet<Literal> Elements { get; }

            public static implicit operator OperableGoal(Goal goal) => new(goal.Elements);

            public static implicit operator Goal(OperableGoal goal) => new(goal.Elements);

            public static implicit operator OperableGoal(OperableSentence sentence) => null; //TODO
        }

        public class OperableState
        {
            internal OperableState(IEnumerable<Predicate> elements) => Elements = elements.ToImmutableHashSet();

            internal IReadOnlySet<Predicate> Elements { get; }

            public static implicit operator OperableState(State state) => new(state.Elements);

            public static implicit operator State(OperableState state) => new(state.Elements);

            public static implicit operator OperableState(OperableSentence sentence) => null; //TODO
        } 

        public class OperableEffect
        {
            public OperableEffect(IEnumerable<Literal> elements) => Elements = elements.ToImmutableHashSet();

            public IReadOnlySet<Literal> Elements { get; }

            public static implicit operator OperableEffect(Effect effect) => new(effect.Elements);

            public static implicit operator Effect(OperableEffect effect) => new(effect.Elements);

            public static implicit operator OperableEffect(OperableSentence sentence) => null; //TODO
        }
    }
}
