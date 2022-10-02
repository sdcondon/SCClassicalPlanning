using SCFirstOrderLogic;

namespace SCClassicalPlanning.ProblemManipulation
{
    /// <summary>
    /// Base class for recursive transformations of <see cref="Sentence"/> instances to other <see cref="Sentence"/> instances.
    /// </summary>
    public abstract class RecursiveGoalTransformation : IGoalTransformation<Goal>
    {
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
        /// Applies this transformation to a <see cref="Literal"/> instance. 
        /// The default implementation returns a <see cref="Literal"/> with the same positivity as te existing literal and a predicate that it the result of calling <see cref="ApplyTo(Predicate)"/> on the existing predicate.
        /// </summary>
        /// <param name="literal">The <see cref="Literal"/> instance to visit.</param>
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
        /// Applies this transformation to a <see cref="Predicate"/> instance. 
        /// The default implementation returns a <see cref="Predicate"/> with the same Symbol and with an argument list that is the result of calling <see cref="ApplyTo(Term)"/> on all of the existing arguments.
        /// </summary>
        /// <param name="predicate">The <see cref="Predicate"/> instance to visit.</param>
        /// <returns>The transformed literal.</returns>
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
