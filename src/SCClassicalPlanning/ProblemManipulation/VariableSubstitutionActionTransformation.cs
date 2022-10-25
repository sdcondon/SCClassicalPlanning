using SCFirstOrderLogic;
using SCFirstOrderLogic.SentenceManipulation;

namespace SCClassicalPlanning.ProblemManipulation
{
    /// <summary>
    /// Utility class to transform <see cref="Action"/> instances using a given <see cref="VariableSubstitution"/>.
    /// </summary>
    public class VariableSubstitutionActionTransformation : RecursiveActionTransformation
    {
        private readonly VariableSubstitution substitution;

        /// <summary>
        /// Initialises a new instance of the <see cref="VariableSubstitutionActionTransformation"/> class.
        /// </summary>
        /// <param name="substitution">The substitution to apply.</param>
        public VariableSubstitutionActionTransformation(VariableSubstitution substitution) => this.substitution = substitution;

        /// <inheritdoc/>
        public override Literal ApplyTo(Literal literal) => substitution.ApplyTo(literal);
    }
}
