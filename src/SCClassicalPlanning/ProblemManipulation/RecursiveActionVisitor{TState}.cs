﻿using SCFirstOrderLogic;

namespace SCClassicalPlanning.ProblemManipulation
{
    /// <summary>
    /// Base class for recursive visitors of <see cref="Action"/> instances that reference external visitation state
    /// (as opposed to maintaining state as fields of the visitor).
    /// </summary>
    public abstract class RecursiveActionVisitor<TState>
    {
        /// <summary>
        /// Visits an <see cref="Action"/> instance.
        /// <para/>
        /// The default implementation just visits the action's precondition and effect.
        /// </summary>
        /// <param name="action">The action to visit.</param>
        public virtual void Visit(Action action, TState visitState)
        {
            Visit(action.Precondition, visitState);
            Visit(action.Effect, visitState);
        }

        /// <summary>
        /// Visits a <see cref="Goal"/> instance.
        /// The default implementation just visits all of the goal's elements.
        /// </summary>
        /// <param name="goal">The <see cref="Goal"/> instance to visit.</param>
        public virtual void Visit(Goal goal, TState visitState)
        {
            foreach (var literal in goal.Elements)
            {
                Visit(literal, visitState);
            }
        }

        /// <summary>
        /// Visits a <see cref="Effect"/> instance.
        /// The default implementation just visits all of the effect's elements.
        /// </summary>
        /// <param name="effect">The <see cref="Effect"/> instance to visit.</param>
        public virtual void Visit(Effect effect, TState visitState)
        {
            foreach (var literal in effect.Elements)
            {
                Visit(literal, visitState);
            }
        }

        /// <summary>
        /// Visits a <see cref="Literal"/> instance. 
        /// The default implementation just visits the underlying predicate.
        /// </summary>
        /// <param name="literal">The <see cref="Literal"/> instance to visit.</param>
        public virtual void Visit(Literal literal, TState visitState)
        {
            Visit(literal.Predicate, visitState);
        }

        /// <summary>
        /// Visits a <see cref="Predicate"/> instance. 
        /// The default implementation just visits each of the arguments.
        /// </summary>
        /// <param name="predicate">The <see cref="Predicate"/> instance to visit.</param>
        public virtual void Visit(Predicate predicate, TState visitState)
        {
            foreach (var argument in predicate.Arguments)
            {
                Visit(argument, visitState);
            }
        }

        /// <summary>
        /// Visits a <see cref="Term"/> instance.
        /// The default implementation invokes the Visit method appropriate to the runtime type of the term.
        /// </summary>
        /// <param name="term">The term to visit.</param>
        public virtual void Visit(Term term, TState visitState)
        {
            switch (term)
            {
                case Constant constant:
                    Visit(constant, visitState);
                    break;
                case VariableReference variableReference:
                    Visit(variableReference, visitState);
                    break;
                default:
                    // NB: FUNCTIONS UNSUPPORTED
                    throw new ArgumentException("Unsupported term type", nameof(term));
            };
        }

        /// <summary>
        /// Visits a <see cref="Constant"/> instance.
        /// The default implementation doesn't do anything.
        /// </summary>
        /// <param name="constant">The constant to visit.</param>
        public virtual void Visit(Constant constant, TState visitState)
        {
        }

        /// <summary>
        /// Visits a <see cref="Function"/> instance.
        /// Since all terms in classical planning should be functionless, the default implementation throws an exception.
        /// </summary>
        /// <param name="function">The function to visit.</param>
        public virtual void Visit(Function function, TState visitState)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Visits a <see cref="VariableReference"/> instance.
        /// The default implementation just visits the variable's declaration
        /// </summary>
        /// <param name="variableReference">The variable reference to visit.</param>
        public virtual void Visit(VariableReference variableReference, TState visitState)
        {
            Visit(variableReference.Declaration, visitState);
        }

        /// <summary>
        /// Visits a <see cref="VariableDeclaration"/> instance.
        /// The default implementation doesn't do anything.
        /// </summary>
        /// <param name="variableDeclaration">The variable declaration to visit.</param>
        public virtual void Visit(VariableDeclaration variableDeclaration, TState visitState)
        {
        }
    }
}