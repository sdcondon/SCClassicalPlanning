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
using SCFirstOrderLogic;

namespace SCClassicalPlanning.ProblemManipulation
{
    /// <summary>
    /// Base class for recursive visitors of <see cref="Effect"/> instances that reference external visitation state
    /// (as opposed to maintaining state as fields of the visitor).
    /// </summary>
    public abstract class RecursiveEffectVisitor<TState>
    {
        /// <summary>
        /// Visits a <see cref="Effect"/> instance.
        /// The default implementation just visits all of the effect's elements.
        /// </summary>
        /// <param name="effect">The <see cref="Effect"/> instance to visit.</param>
        /// <param name="visitState">The state of this visit.</param>
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
        /// <param name="visitState">The state of this visit.</param>
        public virtual void Visit(Literal literal, TState visitState)
        {
            Visit(literal.Predicate, visitState);
        }

        /// <summary>
        /// Visits a <see cref="Predicate"/> instance. 
        /// The default implementation just visits each of the arguments.
        /// </summary>
        /// <param name="predicate">The <see cref="Predicate"/> instance to visit.</param>
        /// <param name="visitState">The state of this visit.</param>
        public virtual void Visit(Predicate predicate, TState visitState)
        {
            for (int i = 0; i < predicate.Arguments.Count; i++)
            {
                Visit(predicate.Arguments[i], visitState);
            }
        }

        /// <summary>
        /// Visits a <see cref="Term"/> instance.
        /// The default implementation invokes the Visit method appropriate to the runtime type of the term.
        /// </summary>
        /// <param name="term">The term to visit.</param>
        /// <param name="visitState">The state of this visit.</param>
        public virtual void Visit(Term term, TState visitState)
        {
            switch (term)
            {
                case VariableReference variableReference:
                    Visit(variableReference, visitState);
                    break;
                case Constant constant:
                    Visit(constant, visitState);
                    break;
                case Function function:
                    Visit(function, visitState);
                    break;
                default:
                    throw new ArgumentException("Unsupported term type", nameof(term));
            };
        }

        /// <summary>
        /// Visits a <see cref="VariableReference"/> instance.
        /// The default implementation just visits the variable's declaration
        /// </summary>
        /// <param name="variableReference">The variable reference to visit.</param>
        /// <param name="visitState">The state of this visit.</param>
        public virtual void Visit(VariableReference variableReference, TState visitState)
        {
            Visit(variableReference.Declaration, visitState);
        }

        /// <summary>
        /// Visits a <see cref="Constant"/> instance.
        /// The default implementation doesn't do anything.
        /// </summary>
        /// <param name="constant">The constant to visit.</param>
        /// <param name="visitState">The state of this visit.</param>
        public virtual void Visit(Constant constant, TState visitState)
        {
        }

        /// <summary>
        /// Visits a <see cref="Function"/> instance.
        /// The default implementation just visits each of the arguments.
        /// </summary>
        /// <param name="function">The function to visit.</param>
        /// <param name="visitState">The state of this visit.</param>
        public virtual void Visit(Function function, TState visitState)
        {
            for (int i = 0; i < function.Arguments.Count; i++)
            {
                Visit(function.Arguments[i], visitState);
            }
        }

        /// <summary>
        /// Visits a <see cref="VariableDeclaration"/> instance.
        /// The default implementation doesn't do anything.
        /// </summary>
        /// <param name="variableDeclaration">The variable declaration to visit.</param>
        /// <param name="visitState">The state of this visit.</param>
        public virtual void Visit(VariableDeclaration variableDeclaration, TState visitState)
        {
        }
    }
}
