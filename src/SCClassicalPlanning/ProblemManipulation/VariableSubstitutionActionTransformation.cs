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
