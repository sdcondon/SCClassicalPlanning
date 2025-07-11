﻿// Copyright 2022-2024 Simon Condon
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

namespace SCClassicalPlanning.ProblemManipulation;

public static class IStateExtensions
{
    public static IEnumerable<Function> GetAllConstants(this IState state)
    {
        return state.Elements.SelectMany(p => ConstantFinder.GetAllConstants(p)).Distinct();
    }

    public static IEnumerable<Predicate> GetAllPredicates(this IQueryable<Action> actions)
    {
        return actions.SelectMany(a => PredicateFinder.GetAllPredicates(a)).Distinct();
    }

    /// <summary>
    /// Utility class to find constants within the elements of a <see cref="IState"/>, and add them to a given <see cref="HashSet{T}"/>.
    /// </summary>
    private class ConstantFinder : RecursiveSentenceVisitor<HashSet<Function>>
    {
        private static readonly ConstantFinder _instance = new();

        private ConstantFinder() { }

        public static IEnumerable<Function> GetAllConstants(Predicate predicate)
        {
            HashSet<Function> result = new();
            _instance.Visit(predicate, result);
            return result;
        }

        /// <inheritdoc/>
        public override void Visit(Function function, HashSet<Function> constants)
        {
            if (function.IsGroundTerm)
            {
                constants.Add(function);
            }
        }
    }

    private class PredicateFinder : RecursiveActionVisitor<HashSet<Predicate>>
    {
        private static readonly PredicateFinder _instance = new();

        private PredicateFinder() { }

        public static IEnumerable<Predicate> GetAllPredicates(Action action)
        {
            HashSet<Predicate> result = new();
            _instance.Visit(action, result);
            return result;
        }

        public override void Visit(Predicate predicate, HashSet<Predicate> predicates)
        {
            // Standardise the arguments so that we unify all occurences of the 'same' predicate (with the same symbol and same number of arguments)
            // Yeah, could be safer, but realistically not going to have a predicate with this many parameters..
            // Ultimately, it might be a nice quality-of-life improvement to keep variable names as-is if its appropriate (e.g. if its the same
            // in all copies of this predicate) - but can come back to that.
            var standardisedParameters = Enumerable.Range(0, predicate.Arguments.Count).Select(i => new VariableReference(((char)('A' + i)).ToString())).ToArray();
            predicates.Add(new Predicate(predicate.Identifier, standardisedParameters));
        }
    }
}
