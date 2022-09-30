﻿#if false
using System.Collections.Generic;

namespace SCClassicalPlanningAlternatives.OwnFolModel.SentenceManipulation
{
    /// <summary>
    /// Sentence transformation class that makes some substitutions for variable terms.
    /// In addition to the <see cref="RecursiveStateTransformation.ApplyTo(Sentence)"/> method
    /// offered by the base class, this also offers an <see cref="ApplyTo(CNFLiteral)"/> method.
    /// </summary>
    public class VariableSubstitution : RecursiveStateTransformation
    {
        private readonly Dictionary<VariableReference, Term> bindings;

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableSubstitution"/> class that is empty.
        /// </summary>
        public VariableSubstitution()
        {
            bindings = new();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableSubstitution"/> class that is a clone of another.
        /// </summary>
        /// <param name="substitution">The substitution to clone.</param>
        public VariableSubstitution(VariableSubstitution substitution)
            : this(substitution.bindings)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableSubstitution"/> class that uses a given set of bindings.
        /// </summary>
        /// <param name="bindings">The bindings to use.</param>
        internal VariableSubstitution(IReadOnlyDictionary<VariableReference, Term> bindings)
        {
            this.bindings = new (bindings);
        }

        /// <summary>
        /// Gets the substitions applied by this transformation.
        /// </summary>
        public IReadOnlyDictionary<VariableReference, Term> Bindings => bindings;

        /// <summary>
        /// Adds a binding.
        /// </summary>
        /// <param name="variable">A reference to the variable to be substituted out.</param>
        /// <param name="term">The term to be substituted in.</param>
        internal void AddBinding(VariableReference variable, Term term)
        {
            bindings.Add(variable, term);
        }

        /// <summary>
        /// Applies the unifier to a literal.
        /// </summary>
        /// <param name="literal">The literal to apply the unifier to.</param>
        /// <returns>The unified version of the literal.</returns>
        public CNFLiteral ApplyTo(CNFLiteral literal)
        {
            return new CNFLiteral((Predicate)base.ApplyTo(literal.Predicate), literal.IsNegated);
        }

        /// <inheritdoc />
        public override Term ApplyTo(VariableReference variable)
        {
            if (Bindings.TryGetValue(variable, out var substitutedTerm))
            {
                // Don't need base.ApplyTo because we apply this as we add each substitution.
                return substitutedTerm;
            }

            return variable;
        }
    }
}
#endif