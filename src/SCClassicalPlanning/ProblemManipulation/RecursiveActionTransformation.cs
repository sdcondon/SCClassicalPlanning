// Copyright 2022 Simon Condon
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
    /// Base class for recursive transformations of <see cref="Action"/> instances to other <see cref="Action"/> instances.
    /// </summary>
    public abstract class RecursiveActionTransformation
    {
        /// <summary>
        /// Applies this transformation to a <see cref="Action"/> instance.
        /// <para/>
        /// The default implementation returns a <see cref="Action"/> with the same identifier, and with a goal and effect that is the result of applying the transformation to the action's goal and effect respectively.
        /// </summary>
        /// <param name="action">The action to visit.</param>
        /// <returns>The transformed <see cref="Action"/>.</returns>
        public virtual Action ApplyTo(Action action)
        {
            var precondition = ApplyTo(action.Precondition);
            var effect = ApplyTo(action.Effect);

            if (precondition != action.Precondition || effect != action.Effect)
            {
                return new(action.Identifier, precondition, effect);
            }

            return action;
        }

        /// <summary>
        /// Applies this transformation to a <see cref="Goal"/> instance.
        /// <para/>
        /// The default implementation returns a <see cref="Goal"/> with an element list that is the result of calling <see cref="ApplyTo(Literal)"/> on all of the existing elements.
        /// </summary>
        /// <param name="goal">The sentence to visit.</param>
        /// <returns>The transformed <see cref="Goal"/>.</returns>
        public virtual Goal ApplyTo(Goal goal)
        {
            var elements = goal.Elements.Select(a => ApplyTo(a)).ToList();

            if (elements.Zip(goal.Elements, (x, y) => (x, y)).Any(t => t.x != t.y))
            {
                return new Goal(elements);
            }

            return goal;
        }

        /// <summary>
        /// Applies this transformation to a <see cref="Effect"/> instance.
        /// <para/>
        /// The default implementation returns a <see cref="Effect"/> with an element list that is the result of calling <see cref="ApplyTo(Literal)"/> on all of the existing elements.
        /// </summary>
        /// <param name="effect">The effect to transform.</param>
        /// <returns>The transformed effect.</returns>
        public virtual Effect ApplyTo(Effect effect)
        {
            var elements = effect.Elements.Select(a => ApplyTo(a)).ToList();

            if (elements.Zip(effect.Elements, (x, y) => (x, y)).Any(t => t.x != t.y))
            {
                return new Effect(elements);
            }

            return effect;
        }

        /// <summary>
        /// Applies this transformation to a <see cref="Literal"/> instance. 
        /// The default implementation returns a <see cref="Literal"/> with the same positivity as the existing literal and a predicate that it the result of calling <see cref="ApplyTo(Predicate)"/> on the existing predicate.
        /// </summary>
        /// <param name="literal">The <see cref="Literal"/> instance to visit.</param>
        /// <returns>The transformed predicate.</returns>
        public virtual Literal ApplyTo(Literal literal)
        {
            var predicate = ApplyTo(literal.Predicate);
            if (predicate != literal.Predicate)
            {
                return new Literal(predicate, literal.IsNegated);
            }

            return literal;
        }

        /// <summary>
        /// Applies this transformation to a <see cref="Predicate"/> instance. 
        /// The default implementation returns a <see cref="Predicate"/> with the same Symbol and with an argument list that is the result of calling <see cref="ApplyTo(Term)"/> on all of the existing arguments.
        /// </summary>
        /// <param name="predicate">The <see cref="Predicate"/> instance to visit.</param>
        /// <returns>The transformed predicate.</returns>
        public virtual Predicate ApplyTo(Predicate predicate)
        {
            var arguments = predicate.Arguments.Select(a => ApplyTo(a)).ToList();

            if (arguments.Zip(predicate.Arguments, (x, y) => (x, y)).Any(t => t.x != t.y))
            {
                return new Predicate(predicate.Symbol, arguments);
            }

            return predicate;
        }

        /// <summary>
        /// Applies this transformation to a <see cref="Term"/> instance.
        /// The default implementation simply invokes the ApplyTo method appropriate to the type of the term.
        /// </summary>
        /// <param name="term">The term to visit.</param>
        /// <returns>The transformed term.</returns>
        public virtual Term ApplyTo(Term term)
        {
            return term switch
            {
                Constant constant => ApplyTo(constant),
                VariableReference variable => ApplyTo(variable),
                Function function => ApplyTo(function),
                _ => throw new ArgumentException($"Unsupported Term type '{term.GetType()}'", nameof(term))
            };
        }

        /// <summary>
        /// Applies this transformation to a <see cref="Constant"/> instance.
        /// The default implementation simply returns the constant unchanged.
        /// </summary>
        /// <param name="constant">The constant to visit.</param>
        /// <returns>The transformed term.</returns>
        public virtual Term ApplyTo(Constant constant)
        {
            return constant;
        }

        /// <summary>
        /// Applies this transformation to a <see cref="VariableReference"/> instance.
        /// The default implementation returns a <see cref="VariableReference"/> referring to the variable that is the result of calling <see cref="ApplyTo(VariableDeclaration)"/> on the current declaration.
        /// </summary>
        /// <param name="variable">The variable to visit.</param>
        /// <returns>The transformed term.</returns>
        public virtual Term ApplyTo(VariableReference variable)
        {
            var variableDeclaration = ApplyTo(variable.Declaration);
            if (variableDeclaration != variable.Declaration)
            {
                return new VariableReference(variableDeclaration);
            }

            return variable;
        }

        /// <summary>
        /// Applies this transformation to a <see cref="VariableDeclaration"/> instance.
        /// The default implementation simply returns the passed value.
        /// </summary>
        /// <param name="variableDeclaration">The <see cref="VariableDeclaration"/> instance to transform.</param>
        /// <returns>The transformed <see cref="VariableReference"/> declaration.</returns>
        public virtual VariableDeclaration ApplyTo(VariableDeclaration variableDeclaration)
        {
            return variableDeclaration;
        }
    }
}
